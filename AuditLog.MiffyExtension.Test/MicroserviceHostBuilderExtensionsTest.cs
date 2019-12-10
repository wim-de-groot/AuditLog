using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Miffy.MicroServices.Host;
using Minor.Miffy.TestBus;

namespace AuditLog.MiffyExtension.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            // Arrange
            using var context = new TestBusContext();
            
            using var builder = new MicroserviceHostBuilder()
                .WithBusContext(context)
                .WithAuditLogger();

            using var host = builder.CreateHost();
            host.Start();
        }
    }
}