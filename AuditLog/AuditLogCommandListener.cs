using System;
using System.Linq;
using System.Text;
using AuditLog.Abstractions;
using AuditLog.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client.Events;

namespace AuditLog
{
    public class AuditLogCommandListener : ICommandListener
    {
        private readonly ILogger<AuditLogCommandListener> _logger;
        private readonly IAuditLogRepository<LogEntry, long> _repository;
        private readonly IEventReplayer _eventReplayer;
        private readonly IRoutingKeyMatcher _routingKeyMatcher;
        private readonly IEventBus _eventBus;

        public AuditLogCommandListener(IAuditLogRepository<LogEntry, long> repository, IEventReplayer eventReplayer,
            IRoutingKeyMatcher routingKeyMatcher, IEventBus eventBus)
        {
            _eventBus = eventBus;
            _repository = repository;
            _eventReplayer = eventReplayer;
            _routingKeyMatcher = routingKeyMatcher;
            _logger = AuditLogLoggerFactory.CreateInstance<AuditLogCommandListener>();
        }
        public void Handle(object sender, BasicDeliverEventArgs basicDeliverEventArgs)
        {
            var body = Encoding.UTF8.GetString(basicDeliverEventArgs.Body);

            var command = JsonConvert.DeserializeObject<ReplayEventsCommand>(body);
            
            var response = ReplayEvents(command);
            
            _eventBus.PublishCommand(response);
            
            _eventBus.Model.BasicAck(basicDeliverEventArgs.DeliveryTag, false);
        }
        private ReplayEventsResponse ReplayEvents(ReplayEventsCommand command)
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
                
                _eventReplayer.RegisterReplayExchange(command.ReplayExchangeName);

                _eventReplayer.ReplayLogEntries(logEntries);
                _logger.LogTrace($"Replayed {logEntries.Count} log entries");
            }
            catch(Exception exception)
            {
                _logger.LogError($"Internal error occured, with exception: {exception.Message}");
                return new ReplayEventsResponse {Code = StatusCodes.Status500InternalServerError, Status = "Internal Error"};
            }

            _logger.LogTrace("Sending response");
            return new ReplayEventsResponse {Code = StatusCodes.Status200OK, Status = "OK"};
        }
    }
}