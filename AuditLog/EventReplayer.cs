using System.Collections.Generic;
using System.Text;
using AuditLog.Abstractions;
using AuditLog.Domain;
using Microsoft.Extensions.Logging;
using Minor.Miffy;
using RabbitMQ.Client;

namespace AuditLog
{
    public class EventReplayer : IEventReplayer
    {
        private readonly IBusContext<IConnection> _context;
        private readonly ILogger<EventReplayer> _logger;
        public EventReplayer(IBusContext<IConnection> context)
        {
            _context = context;
            _logger = AuditLogLoggerFactory.CreateInstance<EventReplayer>();
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
            using var channel = _context.Connection.CreateModel();
            _logger.LogTrace("Opened channel");

            var properties = channel.CreateBasicProperties();
            _logger.LogTrace("Created basic properties");
            
            properties.Timestamp = new AmqpTimestamp(logEntry.Timestamp);
            properties.Type = logEntry.EventType;
            _logger.LogTrace("Added timestamp and type to properties");

            var body = Encoding.Unicode.GetBytes(logEntry.EventJson);
            _logger.LogTrace("Encoded Json message");
            
            channel.BasicPublish(
                exchange: string.Empty,
                routingKey: $"Replay.{logEntry.RoutingKey}",
                basicProperties: properties,
                body: body
                );
            
            _logger.LogTrace($"Published log entry: {logEntry.Id}");
        }
    }
}