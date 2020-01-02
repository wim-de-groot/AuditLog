using System.Diagnostics.CodeAnalysis;

namespace AuditLog.Domain
{
    [ExcludeFromCodeCoverage]
    public class ReplayEventsResponse : DomainCommand
    {
        public ReplayEventsResponse() : base("AuditLog") { }
        public int Code { get; set; }
        public string Status { get; set; }
    }
}