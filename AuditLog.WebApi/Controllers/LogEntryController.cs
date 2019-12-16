using System.Collections.Generic;
using System.Linq;
using AuditLog.Abstractions;
using AuditLog.Domain;
using Microsoft.AspNetCore.Mvc;

namespace AuditLog.WebApi.Controllers
{
    public class LogEntryController
    {
        private readonly IAuditLogRepository<LogEntry, long> _repository;

        public LogEntryController(IAuditLogRepository<LogEntry,long> repository) => _repository = repository;

        public ActionResult<IEnumerable<LogEntry>> GetAll() => _repository.FindAll().ToList();
    }
}