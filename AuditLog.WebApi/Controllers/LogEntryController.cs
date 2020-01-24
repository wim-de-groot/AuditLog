using System.Collections.Generic;
using System.Linq;
using AuditLog.Abstractions;
using AuditLog.Domain;
using Microsoft.AspNetCore.Mvc;

namespace AuditLog.WebApi.Controllers
{
    [ApiController]
    [Route("logEntries")]
    public class LogEntryController : ControllerBase
    {
        private readonly IAuditLogRepository<LogEntry, long> _repository;

        public LogEntryController(IAuditLogRepository<LogEntry,long> repository) => _repository = repository;

        [HttpGet]
        public ActionResult<IEnumerable<LogEntry>> GetAll() => _repository.FindAll().ToList();
    }
}