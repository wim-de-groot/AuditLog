using System;
using Minor.Miffy;
using Minor.Miffy.RabbitMQBus;
using RabbitMQ.Client;

namespace AuditLog.MiffyExtension
{
    public class RabbitMqReplayContextBuilder : RabbitMqContextBuilder
    {
        public string ReplayExchangePrefix { get; protected set; }

        public RabbitMqReplayContextBuilder WithReplayExchangePrefix(string replayExchangePrefix)
        {
            ReplayExchangePrefix = replayExchangePrefix;
            
            return this;
        }

        public override RabbitMqContextBuilder ReadFromEnvironmentVariables()
        {
            ReplayExchangePrefix = Environment.GetEnvironmentVariable("REPLAY_EXCHANGE_NAME") ??
                                   throw new BusConfigurationException("REPLAY_EXCHANGE_NAME variable not set");

            return base.ReadFromEnvironmentVariables();
        }
        
        public override IBusContext<IConnection> CreateContext(IConnectionFactory connectionFactory)
        {
            using var connection = connectionFactory.CreateConnection();
            using var model = connection.CreateModel();
            
            model.ExchangeDeclare(ReplayExchangePrefix, ExchangeType.Direct);

            return new RabbitMqBusReplayContext(connection, ExchangeName, ReplayExchangePrefix);
        }
    }
}