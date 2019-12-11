using Minor.Miffy.MicroServices.Host;

namespace AuditLog.MiffyExtension
{
    public class AuditLogMicroserviceHostBuilder : MicroserviceHostBuilder
    {
        public string ReplayExchange { get; set; }
        public string ReplayQueue { get; set; }
        public AuditLogMicroserviceHostBuilder WithReplayExchange(string replayExchange)
        {
            ReplayExchange = replayExchange;
            return this;
        }

        public AuditLogMicroserviceHostBuilder WithReplayQueue(string replayQueue)
        {
            ReplayQueue = replayQueue;
            return this;
        }

        public override MicroserviceHost CreateHost()
        {
            var host = base.CreateHost();

            var messageReceiver = new AuditLogRabbitMqMessageReceiver(Context, ReplayExchange, ReplayQueue);

            return host;
        }
    }
}