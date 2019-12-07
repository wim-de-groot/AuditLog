using System.Diagnostics.CodeAnalysis;

namespace AuditLog.Domain
{
    [ExcludeFromCodeCoverage]
    public class LogEntry
    {
        public long Id { get; set; }
        public long Timestamp { get; set; }
        public string RoutingKey { get; set; }
        public string EventType { get; set; }
        public string EventJson { get; set; }
    }
}