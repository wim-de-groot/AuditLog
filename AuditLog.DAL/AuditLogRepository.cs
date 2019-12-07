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
            _context.LogEntries.Where(entry =>
                IsNullOrGreaterOrEqualsThen(criteria.FromTimestamp, entry.Timestamp) &&
                IsNullOrSmallerOrEqualsThen(criteria.ToTimestamp, entry.Timestamp) &&
                IsNullOrEqualsTo(criteria.EventType, entry.EventType)
            );

        public void Create(LogEntry entity)
        {
            _context.LogEntries.Add(entity);
            _context.SaveChanges();
        }

        private static bool IsNullOrGreaterOrEqualsThen(long? criteriaTimestamp, long entryTimestamp) =>
            criteriaTimestamp == null || entryTimestamp >= criteriaTimestamp;

        private static bool IsNullOrSmallerOrEqualsThen(long? criteriaTimestamp, long entryTimestamp) =>
            criteriaTimestamp == null || entryTimestamp <= criteriaTimestamp;

        private static bool IsNullOrEqualsTo(string criteriaValue, string entryValue) =>
            criteriaValue == null || entryValue == criteriaValue;
    }
}