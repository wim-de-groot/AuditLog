using System;
using System.Threading;
using AuditLog.Abstractions;
using AuditLog.Domain;
using Microsoft.Extensions.Logging;
using Minor.Miffy.MicroServices.Host;
using Minor.Miffy.RabbitMQBus;

namespace AuditLog.ExampleApp
{
    public class Program
    {
        private static readonly ManualResetEvent _stopEvent = new ManualResetEvent(false);

        public static void Main(string[] args)
        {
            var loggerFactory = LoggerFactory.Create(configure => configure.SetMinimumLevel(LogLevel.Debug));

            var contextBuilder = new RabbitMqContextBuilder()
                .ReadFromEnvironmentVariables();

            using var context = contextBuilder
                .CreateContext();

            var builder = new MicroserviceHostBuilder()
                .WithBusContext(context)
                .SetLoggerFactory(loggerFactory)
                .RegisterDependencies(services => { })
                .UseConventions();

            using var host = builder.CreateHost();

            host.Start();
            
            _stopEvent.WaitOne();
        }
    }
}