using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RabbitMQ.Client;

namespace AuditLog.MiffyExtension.Test
{
    [TestClass]
    public class RabbitMqReplayMessageReceiverTest
    {
        [TestMethod]
        public void QueueNameShouldBeInjected()
        {
            // Arrange
            var connectionMock = new Mock<IConnection>();
            var modelMock = new Mock<IModel>();
            var connection = connectionMock.Object;
            var model = modelMock.Object;
            using var context = new RabbitMqBusReplayContext(connection, "NormalExchange", "ReplayExchange");
            connectionMock.Setup(mock => mock.CreateModel()).Returns(model);

            // Act
            using var messageReceiver = new RabbitMqReplayMessageReceiver(context, "TestQueueName");

            // Assert
            Assert.AreEqual("TestQueueName", messageReceiver.QueueName);
        }

        [TestMethod]
        public void ContextShouldBeInjected()
        {
            // Arrange
            var connectionMock = new Mock<IConnection>();
            var modelMock = new Mock<IModel>();
            var connection = connectionMock.Object;
            var model = modelMock.Object;
            using var context = new RabbitMqBusReplayContext(connection, "NormalExchange", "ReplayExchange");
            connectionMock.Setup(mock => mock.CreateModel()).Returns(model);

            // Act
            using var messageReceiver = new RabbitMqReplayMessageReceiver(context, "TestQueueName");

            // Assert
            Assert.AreEqual(context, messageReceiver.Context);
        }

        [TestMethod]
        public void ChannelShouldBeInjected()
        {
            // Arrange
            var connectionMock = new Mock<IConnection>();
            var modelMock = new Mock<IModel>();
            var connection = connectionMock.Object;
            var model = modelMock.Object;
            using var context = new RabbitMqBusReplayContext(connection, "NormalExchange", "ReplayExchange");
            connectionMock.Setup(mock => mock.CreateModel()).Returns(model);

            // Act
            using var messageReceiver = new RabbitMqReplayMessageReceiver(context, "TestQueueName");

            // Assert
            Assert.AreEqual(model, messageReceiver.Channel);
        }

        [TestMethod]
        public void InitialIsListeningShouldBeFalse()
        {
            // Arrange
            var connectionMock = new Mock<IConnection>();
            var modelMock = new Mock<IModel>();
            var connection = connectionMock.Object;
            var model = modelMock.Object;
            using var context = new RabbitMqBusReplayContext(connection, "NormalExchange", "ReplayExchange");
            connectionMock.Setup(mock => mock.CreateModel()).Returns(model);

            // Act
            using var messageReceiver = new RabbitMqReplayMessageReceiver(context, "TestQueueName");
            
            // Assert
            Assert.IsFalse(messageReceiver.IsListening);
        }

        [TestMethod]
        public void DisposingShouldCallDisposeOnChannel()
        {
            // Arrange
            var connectionMock = new Mock<IConnection>();
            var modelMock = new Mock<IModel>();
            var connection = connectionMock.Object;
            var model = modelMock.Object;
            using var context = new RabbitMqBusReplayContext(connection, "NormalExchange", "ReplayExchange");
            connectionMock.Setup(mock => mock.CreateModel()).Returns(model);

            // Act
            using (new RabbitMqReplayMessageReceiver(context, "TestQueueName")) { }

            // Assert
            modelMock.Verify(mock => mock.Dispose());
        }

        [TestMethod]
        public void METHOD()
        {
            
        }
    }
}