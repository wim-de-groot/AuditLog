using System.Linq;
using AuditLog.Abstractions;
using AuditLog.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Minor.Miffy.MicroServices.Events;

namespace AuditLog
{
    public class AuditLogCommandListener
    {
        private readonly ILogger<AuditLogCommandListener> _logger;
        private readonly IAuditLogRepository<LogEntry, long> _repository;
        private readonly IEventReplayer _eventReplayer;
        private readonly IRoutingKeyMatcher _routingKeyMatcher;

        public AuditLogCommandListener(IAuditLogRepository<LogEntry, long> repository, IEventReplayer eventReplayer,
            IRoutingKeyMatcher routingKeyMatcher)
        {
            _repository = repository;
            _eventReplayer = eventReplayer;
            _routingKeyMatcher = routingKeyMatcher;
            _logger = AuditLogLoggerFactory.CreateInstance<AuditLogCommandListener>();
        }

        [CommandListener("AuditLog")]
        public ReplayEventsResponse ReplayEvents(ReplayEventsCommand command)
        {
            try
            {
                var criteria = new LogEntryCriteria
                {
                    EventType = command.EventType,
                    RoutingKey = command.RoutingKey,
                    FromTimestamp = command.FromTimestamp,
                    ToTimestamp = command.ToTimestamp
                };

                var logEntries = _repository.FindBy(criteria).ToList();
                _logger.LogTrace($"Found {logEntries.Count} log entries");

                logEntries = logEntries
                    .Where(entry => _routingKeyMatcher.IsMatch(criteria.RoutingKey, entry.RoutingKey)).ToList();
                _logger.LogTrace($"Filtered log entries, which results in {logEntries.Count} log entries");

                _eventReplayer.ReplayLogEntries(logEntries);
                _logger.LogTrace($"Replayed {logEntries.Count} log entries");
            }
            catch
            {
                _logger.LogError("Internal error occured");
                return new ReplayEventsResponse {Code = StatusCodes.Status500InternalServerError, Status = "Internal Error"};
            }

            _logger.LogTrace("Sending response");
            return new ReplayEventsResponse {Code = StatusCodes.Status200OK, Status = "OK"};
        }
    }
}