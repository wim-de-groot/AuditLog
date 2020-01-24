using System.Collections.Generic;
using System.Text;
using AuditLog.Abstractions;
using AuditLog.Domain;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace AuditLog
{
    public class EventReplayer : IEventReplayer
    {
        private readonly IEventBus _eventBus;
        private readonly ILogger<EventReplayer> _logger;
        private string _replayExchangeName;

        public EventReplayer(IEventBus eventBus)
        {
            _eventBus = eventBus;
            _replayExchangeName = string.Empty;
            _logger = AuditLogLoggerFactory.CreateInstance<EventReplayer>();
        }

        public void RegisterReplayExchange(string replayExchangeName)
        {
            _replayExchangeName = replayExchangeName;
        }

        public void ReplayLogEntries(IEnumerable<LogEntry> logEntries)
        {
            foreach (var logEntry in logEntries)
            {
                ReplayLogEntry(logEntry);
            }
        }

        public void ReplayLogEntry(LogEntry logEntry)
        {
            using var channel = _eventBus.Connection.CreateModel();
            _logger.LogTrace("Opened channel");

            var properties = channel.CreateBasicProperties();
            _logger.LogTrace("Created basic properties");
            
            properties.Timestamp = new AmqpTimestamp(logEntry.Timestamp);
            properties.Type = logEntry.EventType;
            _logger.LogTrace("Added timestamp and type to properties");

            var body = Encoding.Unicode.GetBytes(logEntry.EventJson);
            _logger.LogTrace("Encoded Json message");
            
            channel.BasicPublish(
                exchange: _replayExchangeName,
                routingKey: $"Replay.{logEntry.RoutingKey}",
                basicProperties: properties,
                body: body
                );
            
            _logger.LogTrace($"Published log entry: {logEntry.Id}");
        }
    }
}