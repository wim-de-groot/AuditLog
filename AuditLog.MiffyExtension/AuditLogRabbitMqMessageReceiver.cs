using Microsoft.Extensions.Logging;
using Minor.Miffy;
using Minor.Miffy.RabbitMQBus;
using RabbitMQ.Client;

namespace AuditLog.MiffyExtension
{
    public class AuditLogRabbitMqMessageReceiver : RabbitMqMessageReceiver
    {
        public AuditLogRabbitMqMessageReceiver(IBusContext<IConnection> context, string exchangeName, string queueName)
            : base(context, queueName, null) =>
            ExchangeName = exchangeName;

        public string ExchangeName { get; }

        public override void StartReceivingMessages()
        {
            if (IsListening)
            {
                throw new BusConfigurationException("Receiver is already listening to events!");
            }
            
            Logger.LogDebug($"Declaring queue {QueueName} with direct exchange {ExchangeName}");
            Model.ExchangeDeclare(ExchangeName, ExchangeType.Direct);
            Model.QueueDeclare(QueueName);
            Model.QueueBind(QueueName, ExchangeName, QueueName);
            
            IsListening = true;
        }
    }
}