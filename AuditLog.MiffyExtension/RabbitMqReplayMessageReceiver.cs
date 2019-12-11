using AuditLog.MiffyExtension.Abstractions;
using Minor.Miffy;
using RabbitMQ.Client;

namespace AuditLog.MiffyExtension
{
    public class RabbitMqReplayMessageReceiver : IRabbitMqReplayMessageReceiver
    {
        public RabbitMqReplayMessageReceiver(IBusContext<IConnection> context, string queueName)
        {
            Context = context;
            QueueName = queueName;
            Channel = context.Connection.CreateModel();
        }

        public bool IsListening { get; }
        public IBusContext<IConnection> Context { get; }
        public IModel Channel { get; }
        public string QueueName { get; }
        public void StartReceivingMessages()
        {
            throw new System.NotImplementedException();
        }

        public void StartHandlingMessages(EventMessageReceivedCallback callback)
        {
            throw new System.NotImplementedException();
        }

        public void Dispose() => Channel?.Dispose();
    }
}