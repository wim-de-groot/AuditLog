namespace AuditLog.MiffyExtension.Abstractions
{
    public interface IReplayEventListener
    {
        string Queue { get; set; }
    }
}