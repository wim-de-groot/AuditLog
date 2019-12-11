using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RabbitMQ.Client;

namespace AuditLog.MiffyExtension.Test
{
    [TestClass]
    public class RabbitMqBusReplayContextTest
    {
        [TestMethod]
        public void InstanceHasReplayQueuePrefix()
        {
            // Arrange
            var connectionMock = new Mock<IConnection>();
            var connection = connectionMock.Object;
            
            // Act
            using var result = new RabbitMqBusReplayContext(connection, "NormalExchange", "ReplayExchange");
            
            // Assert
            Assert.AreEqual("ReplayExchange",result.ReplayExchangePrefix);
        }

        [TestMethod]
        public void CreateReplayMessageReceiverReturnsInstanceOfRabbitMqReplayMessageReceiver()
        {
            // Arrange
            var connectionMock = new Mock<IConnection>();
            var connection = connectionMock.Object;
            using var context = new RabbitMqBusReplayContext(connection, "NormalExchange", "ReplayExchange");
            
            // Act
            var result = context.CreateReplayMessageReceiver("TestQueue");
            
            // Assert
            Assert.IsInstanceOfType(result, typeof(RabbitMqReplayMessageReceiver));
        }

        [TestMethod]
        public void ResultOfCreateReplayMessageReceiverHasQueueName()
        {
            // Arrange
            var connectionMock = new Mock<IConnection>();
            var connection = connectionMock.Object;
            using var context = new RabbitMqBusReplayContext(connection, "NormalExchange", "ReplayExchange");
            
            // Act
            var result = context.CreateReplayMessageReceiver("TestQueue");
            
            // Assert
            Assert.AreEqual("TestQueue", result.QueueName);
        }
    }
}