using AuditLog.Domain;
using Microsoft.EntityFrameworkCore;

namespace AuditLog.DAL
{
    public class AuditLogContext : DbContext
    {
        public AuditLogContext(DbContextOptions options) : base(options) { }
        public DbSet<LogEntry> LogEntries { get; set; }
    }
}