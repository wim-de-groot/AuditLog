using AuditLog.MiffyExtension.Abstractions;

namespace AuditLog.MiffyExtension
{
    public class ReplayEventListener : IReplayEventListener
    {
        public string Queue { get; set; }
    }
}