﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using AuditLog.Abstractions;
using AuditLog.DAL;
using AuditLog.Domain;
using Microsoft.EntityFrameworkCore;
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
            var loggerFactory = LoggerFactory.Create(builder => builder.SetMinimumLevel(LogLevel.Debug));

            MiffyLoggerFactory.LoggerFactory = loggerFactory;

            RabbitMqLoggerFactory.LoggerFactory = loggerFactory;

            AuditLogLoggerFactory.LoggerFactory = loggerFactory;

            var logger = loggerFactory.CreateLogger("Program");

            try
            {
                var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING") ??
                                       "server=localhost;uid=root;pwd=root;database=AuditLogDB";

                var contextBuilder = new RabbitMqContextBuilder()
                    .ReadFromEnvironmentVariables();

                using var context = contextBuilder
                    .CreateContext();

                var builder = new MicroserviceHostBuilder()
                    .WithBusContext(context)
                    .SetLoggerFactory(loggerFactory)
                    .RegisterDependencies(services =>
                    {
                        services.AddDbContext<AuditLogContext>(optionsBuilder => optionsBuilder
                            .UseMySql(connectionString)
                            .UseLoggerFactory(loggerFactory));
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