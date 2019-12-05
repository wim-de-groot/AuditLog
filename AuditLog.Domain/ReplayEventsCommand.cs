using Minor.Miffy.MicroServices.Commands;

namespace AuditLog.Domain
{
    public class ReplayEventsCommand : DomainCommand
    {
        public ReplayEventsCommand() : base("AuditLog") { }
        public long? FromTimestamp { get; set; }
        public long? ToTimestamp { get; set; }
        public string EventType { get; set; }
        public string RoutingKey { get; set; }
    }
}