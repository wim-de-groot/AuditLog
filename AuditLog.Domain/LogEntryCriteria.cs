using System.Diagnostics.CodeAnalysis;

namespace AuditLog.Domain
{
    [ExcludeFromCodeCoverage]
    public class LogEntryCriteria
    {
        public long? FromTimestamp { get; set; }
        public long? ToTimestamp { get; set; }
        public string EventType { get; set; }
        public string RoutingKey { get; set; }
    }
}