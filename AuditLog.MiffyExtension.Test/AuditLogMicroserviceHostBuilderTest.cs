using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Miffy.TestBus;

namespace AuditLog.MiffyExtension.Test
{
    [TestClass]
    public class AuditLogMicroserviceHostBuilderTest
    {
        [TestMethod]
        public void WithAuditLoggerShouldInstantiate___OnStart()
        {
            // Arrange
            using var context = new TestBusContext();

            // Act
            using var builder = new AuditLogMicroserviceHostBuilder()
                .WithReplayExchange("ReplayExchange")
                .WithReplayQueue("ReplayQueue")
                .WithBusContext(context);
            
            // Assert
            Assert.AreEqual("ReplayExchange", ((AuditLogMicroserviceHostBuilder)builder).ReplayExchange);
            Assert.AreEqual("ReplayQueue", ((AuditLogMicroserviceHostBuilder)builder).ReplayQueue);
        }
    }
}