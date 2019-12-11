using Minor.Miffy.RabbitMQBus;
using RabbitMQ.Client;

namespace AuditLog.MiffyExtension
{
    public class RabbitMqBusReplayContext : RabbitMqBusContext
    {
        public string ReplayExchangePrefix { get; }

        public RabbitMqBusReplayContext(IConnection connection, string exchangeName, string replayExchangePrefix) :
            base(connection, exchangeName) => ReplayExchangePrefix = replayExchangePrefix;

        public RabbitMqReplayMessageReceiver CreateReplayMessageReceiver(string queueName) =>
            new RabbitMqReplayMessageReceiver(this, queueName);
    }
}