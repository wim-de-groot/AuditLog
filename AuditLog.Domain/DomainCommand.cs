using System;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace AuditLog.Domain
{
    [ExcludeFromCodeCoverage]
    public class DomainCommand
    {
        [JsonProperty]
        public long Timestamp { get; protected set; }

        [JsonProperty]
        public Guid Id { get; protected set; }

        [JsonProperty]
        public Guid ProcessId { get; protected set; }

        [JsonProperty]
        public string DestinationQueue { get; protected set; }

        protected DomainCommand(string destinationQueue)
        {
            this.Timestamp = DateTime.Now.Ticks;
            this.Id = Guid.NewGuid();
            this.DestinationQueue = destinationQueue;
        }

        protected DomainCommand(string destinationQueue, Guid processId)
            : this(destinationQueue)
        {
            this.ProcessId = processId;
        }
    }
}