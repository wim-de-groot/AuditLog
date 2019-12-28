using RabbitMQ.Client;

namespace AuditLog.Abstractions
{
    public interface IEventBusBuilder
    {
        string HostName { get; }
        int Port { get; }
        string UserName { get; }
        string Password { get; }
        string ExchangeName { get; }
        IEventBusBuilder FromEnvironment();
        IEventBus CreateEventBus(IConnectionFactory factory);
        IEventBus CreateEventBus();
    }
}