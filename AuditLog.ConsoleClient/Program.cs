using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using AuditLog.DAL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace AuditLog.ConsoleClient
{
    [ExcludeFromCodeCoverage]
    public static class Program
    {
        private static readonly ManualResetEvent _stopEvent = new ManualResetEvent(false);

        public static void Main(string[] args)
        {
            var logLevel = Environment.GetEnvironmentVariable("LOG_LEVEL") ??
                           throw new InvalidEnvironmentException(
                               "Environment variable [LOG_LEVEL] was not provided.");

            Enum.TryParse(logLevel, true, out LogLevel result);
            
            var loggerFactory = LoggerFactory.Create(builder => builder.SetMinimumLevel(result).AddConsole());

            AuditLogLoggerFactory.LoggerFactory = loggerFactory;

            var logger = loggerFactory.CreateLogger("Program");

            try
            {
                var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING") ??
                                       throw new InvalidEnvironmentException(
                                           "Environment variable [CONNECTION_STRING] was not provided.");

                var options = new DbContextOptionsBuilder<AuditLogContext>()
                    .UseMySql(connectionString)
                    .Options;
                using var context = new AuditLogContext(options);
                
                var createdAndSeeded = false;
                const int waitTime = 1000;
                while (!createdAndSeeded)
                {
                    try
                    {
                        context.Database.EnsureCreated();
                        createdAndSeeded = true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        Thread.Sleep(waitTime);
                    }
                }
                
                var repository = new AuditLogRepository(context);
                var routingKeyMatcher = new RoutingKeyMatcher();
                var eventListener = new AuditLogEventListener(repository);
                var eventBusBuilder = new EventBusBuilder().FromEnvironment();
                using var eventBus = eventBusBuilder.CreateEventBus(new ConnectionFactory());
                var eventReplayer = new EventReplayer(eventBus);
                var commandListener =
                    new AuditLogCommandListener(repository, eventReplayer, routingKeyMatcher, eventBus);
                eventBus.AddEventListener(eventListener, "#");
                eventBus.AddCommandListener(commandListener, "AuditLog");

                logger.LogTrace("Host started, audit logger ready to log");

                _stopEvent.WaitOne();
            }
            catch (Exception e)
            {
                logger.LogError($"Error occured while running the client with message: {e.Message}");
            }
        }
    }
}