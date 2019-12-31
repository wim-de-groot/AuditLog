using System;
using RabbitMQ.Client;

namespace AuditLog.Abstractions
{
    public interface IEventBus : IDisposable
    {
        IConnection Connection { get; }
        IModel Model { get; }
        string ExchangeName { get; }
        IEventBus AddEventListener(IEventListener eventListener, string topic);
        IEventBus AddCommandListener(ICommandListener commandListener, string queueName);
    }
}