using System;
using System.Text;
using AuditLog.Abstractions;
using AuditLog.Domain;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AuditLog
{
    public class EventBus : IEventBus
    {
        private bool _disposed;

        public EventBus(IConnection connection, string exchangeName)
        {
            Connection = connection;
            ExchangeName = exchangeName;
            Model = connection.CreateModel();
        }
        public IConnection Connection { get; }
        public string ExchangeName { get; }
        public IModel Model { get; }
        public IEventBus AddEventListener(IEventListener eventListener, string topic)
        {
            var queueName = Model.QueueDeclare().QueueName;
            Model.QueueBind(exchange: ExchangeName,
                queue: queueName,
                routingKey: topic);
            
            var consumer = new EventingBasicConsumer(Model);
            consumer.Received += eventListener.Handle;
            Model.BasicConsume(consumer, queueName);

            return this;
        }
        public IEventBus AddCommandListener(ICommandListener commandListener, string queueName)
        {
             Model.QueueDeclare(queueName, true, false, false, null);
             
             var consumer = new EventingBasicConsumer(Model);
             consumer.Received += commandListener.Handle;

             Model.BasicConsume(queueName, false, Guid.NewGuid().ToString(), false, false, null, consumer);
                 
             return this;
        }
        public void PublishCommand(DomainCommand command)
        {
            var json = JsonConvert.SerializeObject(command);
            var body = Encoding.UTF8.GetBytes(json);
            var basicProperties = Model.CreateBasicProperties();
            basicProperties.Type = command.GetType().Name;
            basicProperties.Timestamp = new AmqpTimestamp(new DateTime(2019, 5, 3).Ticks);
            Model.BasicPublish("", "TestQueue", false, basicProperties, body);
        }
        public void Dispose()
        { 
            Dispose(true);
            GC.SuppressFinalize(this);           
        }
   
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }
      
            if (disposing) {
                Model.Dispose();
                Connection.Dispose();
            }
      
            _disposed = true;
        }
    }
}