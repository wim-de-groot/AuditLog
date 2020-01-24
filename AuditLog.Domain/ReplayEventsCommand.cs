using System.Diagnostics.CodeAnalysis;

namespace AuditLog.Domain
{
    [ExcludeFromCodeCoverage]
    public class ReplayEventsCommand : DomainCommand
    {
        public ReplayEventsCommand() : base("AuditLog") { }
        public long? FromTimestamp { get; set; }
        public long? ToTimestamp { get; set; }
        public string EventType { get; set; }
        public string RoutingKey { get; set; }
        public string ReplayExchangeName { get; set; }
    }
}