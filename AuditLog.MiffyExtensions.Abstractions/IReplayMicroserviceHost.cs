using System.Collections.Generic;
using Minor.Miffy.MicroServices.Host;

namespace AuditLog.MiffyExtensions.Abstractions
{
    public interface IReplayMicroserviceHost : IMicroserviceHost
    {
        public IEnumerable<IReplayMicroserviceListener> ReplayMicroserviceListeners { get; }
        void StartReplaying();
        void StopReplaying();
    }
}