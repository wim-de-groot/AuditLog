using System;
using AuditLog.Abstractions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AuditLog
{
    public class EventBus : IEventBus
    {
        public string HostName { get; private set; }
        public int Port { get; private set; }
        public string UserName { get; private set; }
        public string Password { get; private set; }
        public string ExchangeName { get; private set; }
        public IConnection Connection { get; private set; }
        public IModel Model { get; private set; }

        public IEventBus FromEnvironment()
        {
            HostName = Environment.GetEnvironmentVariable("HOSTNAME");
            Port = int.Parse(Environment.GetEnvironmentVariable("PORT")?? "5672");
            UserName = Environment.GetEnvironmentVariable("USERNAME");
            Password = Environment.GetEnvironmentVariable("PASSWORD");
            ExchangeName = Environment.GetEnvironmentVariable("PASSWORD");
            Password = Environment.GetEnvironmentVariable("PASSWORD");

            return this;
        }

        public IEventBus CreateConnection()
        {
            Connection = new ConnectionFactory
            {
                HostName = HostName,
                Port = Port,
                UserName = UserName,
                Password = Password
            }.CreateConnection();
            
            return this;
        }

        public IEventBus CreateModel()
        {
            Model = Connection.CreateModel();

            return this;
        }

        public IEventBus CreateExchange()
        {
            Model.ExchangeDeclare(exchange: ExchangeName,
                type: ExchangeType.Topic,
                durable: false, autoDelete: false, arguments: null);
            
            return this;
        }

        public void AddEventListener(IEventListener eventListener, string topic)
        {
            var queueName = Model.QueueDeclare().QueueName;
            Model.QueueBind(exchange: ExchangeName,
                queue: queueName,
                routingKey: topic);
            
            var consumer = new EventingBasicConsumer(Model);
            consumer.Received += eventListener.Handle;
        }

        public void Dispose()
        {
            Model?.Dispose();
            Connection?.Dispose();
        }
    }
}