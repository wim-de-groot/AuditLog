using RabbitMQ.Client.Events;

namespace AuditLog.Abstractions
{
    public interface IEventListener
    {
        void Handle(object sender, BasicDeliverEventArgs basicDeliverEventArgs);
    }
}