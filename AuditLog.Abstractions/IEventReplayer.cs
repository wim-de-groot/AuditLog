using System;
using System.Collections.Generic;
using AuditLog.Domain;

namespace AuditLog.Abstractions
{
    public interface IEventReplayer
    {
        void ReplayLogEntries(IEnumerable<LogEntry> logEntries);
        void ReplayLogEntry(LogEntry logEntry);
    }
}