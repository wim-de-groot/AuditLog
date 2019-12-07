using System.Collections.Generic;
using System.Linq;
using AuditLog.Abstractions;
using AuditLog.Domain;

namespace AuditLog.DAL
{
    public class AuditLogRepository : IAuditLogRepository<LogEntry, long>
    {
        private readonly AuditLogContext _context;

        public AuditLogRepository(AuditLogContext context) => 
            _context = context;

        public IEnumerable<LogEntry> FindAll() =>
            _context.LogEntries;

        public IEnumerable<LogEntry> FindBy(LogEntryCriteria criteria) =>
            _context.LogEntries
                .Where(entry => criteria.FromTimestamp == null || entry.Timestamp >= criteria.FromTimestamp)
                .Where(entry => criteria.ToTimestamp == null || entry.Timestamp <= criteria.ToTimestamp)
                .Where(entry => criteria.EventType == null || entry.EventType == criteria.EventType);

        public void Create(LogEntry entity)
        {
            _context.LogEntries.Add(entity);
            _context.SaveChanges();
        }
    }
}