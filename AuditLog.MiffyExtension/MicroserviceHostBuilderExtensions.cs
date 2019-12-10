using Minor.Miffy.MicroServices.Host;

namespace AuditLog.MiffyExtension
{
    public static class MicroserviceHostBuilderExtensions
    {
        public static MicroserviceHostBuilder WithAuditLogger(this MicroserviceHostBuilder builder)
        {
            return builder;
        }
    }
}