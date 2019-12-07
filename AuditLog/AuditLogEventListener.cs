using System.Diagnostics.CodeAnalysis;
using AuditLog.Abstractions;
using AuditLog.Domain;
using Microsoft.Extensions.Logging;
using Minor.Miffy.MicroServices.Events;
using Newtonsoft.Json;

namespace AuditLog
{
    public class AuditLogEventListener
    {
        private readonly IAuditLogRepository<LogEntry, long> _repository;
        private readonly ILogger<AuditLogEventListener> _logger;

        public AuditLogEventListener(IAuditLogRepository<LogEntry, long> repository)
        {
            _repository = repository;
            _logger = AuditLogLoggerFactory.CreateInstance<AuditLogEventListener>();
        }

        [EventListener("AuditLog")]
        [Topic("#")]
        public void Handle(string message)
        {
            var logEntry = JsonConvert.DeserializeObject<LogEntry>(message);
            _logger.LogTrace(
                $"Log entry for event: {logEntry.EventType} with routing key: {logEntry.RoutingKey} deserialized.");

            _repository.Create(logEntry);
            _logger.LogTrace(
                $"Log entry for event: {logEntry.EventType} with routing key: {logEntry.RoutingKey} saved.");
        }
    }
}