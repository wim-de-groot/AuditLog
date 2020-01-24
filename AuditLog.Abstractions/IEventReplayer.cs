using System.Collections.Generic;
using AuditLog.Domain;

namespace AuditLog.Abstractions
{
    public interface IEventReplayer
    {
        void RegisterReplayExchange(string replayExchangeName);
        void ReplayLogEntries(IEnumerable<LogEntry> logEntries);
        void ReplayLogEntry(LogEntry logEntry);
    }
}