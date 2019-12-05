namespace AuditLog.Abstractions
{
    public interface IRoutingKeyMatcher
    {
        bool IsMatch(string criteriaRoutingKey, string entryRoutingKey);
    }
}