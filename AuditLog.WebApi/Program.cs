using System;
using System.Diagnostics.CodeAnalysis;
using AuditLog.DAL;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Minor.Miffy;
using Minor.Miffy.MicroServices.Host;
using Minor.Miffy.RabbitMQBus;
using RabbitMQ.Client;

namespace AuditLog.WebApi
{
    [ExcludeFromCodeCoverage]
    public static class Program
    {
        public static void Main(string[] args)
        {            
            var loggerFactory = LoggerFactory.Create(builder => builder.SetMinimumLevel(LogLevel.Debug));

            MiffyLoggerFactory.LoggerFactory = loggerFactory;

            RabbitMqLoggerFactory.LoggerFactory = loggerFactory;

            AuditLogLoggerFactory.LoggerFactory = loggerFactory;

            var logger = loggerFactory.CreateLogger("Program");
            
            var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING") ??
                                   "server=localhost;uid=root;pwd=root;database=AuditLogDB";
                
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
            var commandListener = new AuditLogCommandListener(repository, eventReplayer, routingKeyMatcher, eventBus);
            eventBus.AddEventListener(eventListener, "#");
            eventBus.AddCommandListener(commandListener, "AuditLog");

            CreateHostBuilder(args).Build().Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}