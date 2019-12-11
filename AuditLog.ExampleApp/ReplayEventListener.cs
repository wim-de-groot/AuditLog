using Minor.Miffy.MicroServices.Events;

namespace AuditLog.ExampleApp
{
    public class ReplayEventListener
    {
        [EventListener("ReplayEventQueue")]
        [Topic("ReplayEventQueue")]
        public void Handles()
    }
}