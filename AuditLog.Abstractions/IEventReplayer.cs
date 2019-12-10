using System.Collections.Generic;
using AuditLog.Domain;

namespace AuditLog.Abstractions
{
    public interface IEventReplayer
    {
        void ReplayLogEntries(IEnumerable<LogEntry> logEntries, string replayQueue);
        void ReplayLogEntry(LogEntry logEntry, string replayQueue);
    }
}