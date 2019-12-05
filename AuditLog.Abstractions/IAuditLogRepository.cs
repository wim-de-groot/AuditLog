using System.Collections.Generic;
using AuditLog.Domain;

namespace AuditLog.Abstractions
{
    public interface IAuditLogRepository<TEntity, in TKey>
    {
        IEnumerable<TEntity> FindAll();
        IEnumerable<TEntity> FindBy(LogEntryCriteria criteria);
        void Create(TEntity entity);
    }
}