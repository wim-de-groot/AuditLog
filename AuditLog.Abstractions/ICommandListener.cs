using RabbitMQ.Client.Events;

namespace AuditLog.Abstractions
{
    public interface ICommandListener
    {
        void Handle(object sender, BasicDeliverEventArgs basicDeliverEventArgs);
    }
}