using System.Diagnostics.CodeAnalysis;
using System.Threading;
using AuditLog.MiffyExtension;
using Microsoft.Extensions.Logging;
using Minor.Miffy;
using Minor.Miffy.MicroServices.Events;
using Minor.Miffy.RabbitMQBus;

namespace ExampleService
{
    [ExcludeFromCodeCoverage]
    public static class Program
    {
        public static void Main(string[] args)
        {
            using var loggerFactory = LoggerFactory.Create(configure =>
            {
                configure.AddConsole().SetMinimumLevel(LogLevel.Error);
            });

            MiffyLoggerFactory.LoggerFactory = loggerFactory;
            RabbitMqLoggerFactory.LoggerFactory = loggerFactory;

            using var context = new RabbitMqReplayContextBuilder()
                .ReadFromEnvironmentVariables()
                .CreateContext();

            IEventPublisher eventPublisher = new EventPublisher(context);

            while (true)
            {
                // TODO Publish some events
                Thread.Sleep(4000);
            }
        }
    }
}