using System;
using Minor.Miffy.MicroServices.Host;

namespace AuditLog.MiffyExtension
{
    public static class MicroserviceHostBuilderExtension
    {
        public static MicroserviceHostBuilder WithAuditLogger(this MicroserviceHostBuilder builder)
        {
            return builder;
        }
    }
}