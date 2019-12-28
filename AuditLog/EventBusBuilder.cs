using System;
using AuditLog.Abstractions;
using RabbitMQ.Client;

namespace AuditLog
{
    public class EventBusBuilder : IEventBusBuilder
    {
        public string HostName { get; private set; }
        public int Port { get; private set; }
        public string UserName { get; private set; }
        public string Password { get; private set; }
        public string ExchangeName { get; private set; }
        public IEventBusBuilder FromEnvironment()
        {
            HostName = Environment.GetEnvironmentVariable("HOSTNAME") ?? throw new ArgumentException("Environment variable [HOSTNAME] can not be null");
            UserName = Environment.GetEnvironmentVariable("USERNAME") ?? throw new ArgumentException("Environment variable [USERNAME] can not be null");
            Password = Environment.GetEnvironmentVariable("PASSWORD") ?? throw new ArgumentException("Environment variable [PASSWORD] can not be null");
            ExchangeName = Environment.GetEnvironmentVariable("EXCHANGE_NAME") ?? throw new ArgumentException("Environment variable [EXCHANGE_NAME] can not be null");
            Port = int.Parse(Environment.GetEnvironmentVariable("PORT") ?? throw new ArgumentException("Environment variable [PORT] can not be null"));
            return this;
        }
        public IEventBus CreateEventBus(IConnectionFactory factory)
        {
            var connection = factory.CreateConnection();
            
            connection
                .CreateModel()
                .ExchangeDeclare(ExchangeName, ExchangeType.Topic);
            
            return new EventBus(connection, ExchangeName);
        }

        public IEventBus CreateEventBus() => CreateEventBus(new ConnectionFactory());
    }
}