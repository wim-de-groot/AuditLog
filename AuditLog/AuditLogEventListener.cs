using System.Text;
using AuditLog.Abstractions;
using AuditLog.Domain;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client.Events;

namespace AuditLog
{
    public class AuditLogEventListener : IEventListener
    {
        private readonly IAuditLogRepository<LogEntry, long> _repository;
        private readonly ILogger<AuditLogEventListener> _logger;
        public AuditLogEventListener(IAuditLogRepository<LogEntry, long> repository)
        {
            _repository = repository;
            _logger = AuditLogLoggerFactory.CreateInstance<AuditLogEventListener>();
        }
        
        public void Handle(object sender, BasicDeliverEventArgs basicDeliverEventArgs)
        {
            var logEntry = new LogEntry
            {
                EventType = basicDeliverEventArgs.BasicProperties.Type,
                Timestamp = basicDeliverEventArgs.BasicProperties.Timestamp.UnixTime,
                RoutingKey = basicDeliverEventArgs.RoutingKey,
                EventJson = Encoding.UTF8.GetString(basicDeliverEventArgs.Body)
            };
            
            _logger.LogTrace(
                $"Log entry for event: {logEntry.EventType} with routing key: {logEntry.RoutingKey} deserialized.");

            _repository.Create(logEntry);
            
            _logger.LogTrace(
                $"Log entry for event: {logEntry.EventType} with routing key: {logEntry.RoutingKey} saved.");
        }
    }
}