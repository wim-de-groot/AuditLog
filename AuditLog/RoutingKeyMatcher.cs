using System.Text.RegularExpressions;
using AuditLog.Abstractions;

namespace AuditLog
{
    public class RoutingKeyMatcher : IRoutingKeyMatcher
    {
        public bool IsMatch(string criteriaRoutingKey, string entryRoutingKey)
        {
            if (criteriaRoutingKey == null) {return true;}

            var pattern = criteriaRoutingKey
                .Replace(".#", ".*")
                .Replace("#", "*");
            
            var regex = new Regex($"^{pattern}$");

            return regex.IsMatch(entryRoutingKey);
        }
    }
}