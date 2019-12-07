using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using AuditLog.Abstractions;
using AuditLog.DAL;
using AuditLog.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Minor.Miffy;
using Minor.Miffy.MicroServices.Host;
using Minor.Miffy.RabbitMQBus;

namespace AuditLog.ConsoleClient
{
    [ExcludeFromCodeCoverage]
    public static class Program
    {
        private static readonly ManualResetEvent _stopEvent = new ManualResetEvent(false);

        public static void Main(string[] args)
        {
            using var loggerFactory =
                LoggerFactory.Create(configure => configure.AddConsole().SetMinimumLevel(LogLevel.Trace));

            MiffyLoggerFactory.LoggerFactory = loggerFactory;

            RabbitMqLoggerFactory.LoggerFactory = loggerFactory;

            AuditLogLoggerFactory.LoggerFactory = loggerFactory;

            var logger = loggerFactory.CreateLogger("Program");

            try
            {
                var exchangeName = Environment.GetEnvironmentVariable("EXCHANGE_NAME") ?? "Default";

                var rabbitMqConnectionString = Environment.GetEnvironmentVariable("RMQ_CONNECTIONSTRING") ??
                                               "amqp://guest:guest@localhost";

                var contextBuilder = new RabbitMqContextBuilder()
                    .WithExchange(exchangeName)
                    .WithConnectionString(rabbitMqConnectionString);

                using var context = contextBuilder.CreateContext();

                var builder = new MicroserviceHostBuilder()
                    .WithBusContext(context)
                    .SetLoggerFactory(loggerFactory)
                    .RegisterDependencies(services =>
                    {
                        services.AddDbContext<AuditLogContext>();
                        services.AddTransient<IAuditLogRepository<LogEntry, long>, AuditLogRepository>();
                        services.AddTransient<IEventReplayer, EventReplayer>();
                        services.AddTransient<IRoutingKeyMatcher, RoutingKeyMatcher>();
                    })
                    .UseConventions();

                using var host = builder.CreateHost();

                host.Start();
                
                logger.LogTrace("Host started, audit logger ready to log");

                _stopEvent.WaitOne();
            }
            catch (Exception e)
            {
                logger.LogError($"Error occured while starting client with message: {e.Message}");
            }
        }
    }
}