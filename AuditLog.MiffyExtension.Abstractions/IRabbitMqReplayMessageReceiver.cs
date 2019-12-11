using System;
using Minor.Miffy;
using RabbitMQ.Client;

namespace AuditLog.MiffyExtension.Abstractions
{
    public interface IRabbitMqReplayMessageReceiver : IDisposable
    { 
        bool IsListening { get; }
        IBusContext<IConnection> Context { get; }
        IModel Channel { get; }
        string QueueName { get; }
        void StartReceivingMessages();
        void StartHandlingMessages(EventMessageReceivedCallback callback);
    }
}