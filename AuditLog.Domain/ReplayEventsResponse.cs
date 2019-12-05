using Minor.Miffy.MicroServices.Commands;

namespace AuditLog.Domain
{
    public class ReplayEventsResponse : DomainCommand
    {
        public ReplayEventsResponse() : base("AuditLog") { }
        public int Code { get; set; }
        public string Status { get; set; }
    }
}