using System;
using RabbitMQ.Client;

namespace AuditLog.Abstractions
{
    public interface IEventBus : IDisposable
    {
        string HostName { get; }
        int Port { get; }
        string UserName { get; }
        string Password { get; }
        IConnection Connection { get; }
        IModel Model { get; }
        string ExchangeName { get; }
        IEventBus FromEnvironment();
        IEventBus CreateConnection();
        IEventBus CreateModel();
        IEventBus CreateExchange();
        void AddEventListener(IEventListener eventListener, string topic);
    }
}