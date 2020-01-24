using System;
using System.Diagnostics.CodeAnalysis;
using AuditLog.DAL;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace AuditLog.WebApi
{
    [ExcludeFromCodeCoverage]
    public static class Program
    {
        public static void Main(string[] args)
        {
            var loggerFactory = LoggerFactory.Create(builder => builder.SetMinimumLevel(LogLevel.Debug));

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
                
                CreateHostBuilder(args).Build().Run();

                logger.LogTrace("AuditLog started ...");
            }
            catch (Exception e)
            {
                logger.LogError($"Error occured while running the client with message: {e.Message}");
            }
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}