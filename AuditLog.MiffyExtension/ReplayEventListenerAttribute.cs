using System;

namespace AuditLog.MiffyExtension
{
    public class ReplayEventListenerAttribute : Attribute
    {
        public string QueueName { get; }
        
        public ReplayEventListenerAttribute(string queueName) => QueueName = queueName;
    }
}