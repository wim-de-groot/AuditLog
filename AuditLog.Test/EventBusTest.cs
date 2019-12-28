using System.Collections.Generic;
using AuditLog.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AuditLog.Test
{
    [TestClass]
    public class EventBusTest
    {
        [TestMethod]
        public void InstanceShouldBeCreatingModel()
        {
            // Arrange
            var modelMock = new Mock<IModel>();
            var model = modelMock.Object;
            var connectionMock = new Mock<IConnection>();
            var connection = connectionMock.Object;
            connectionMock.Setup(mock => mock.CreateModel()).Returns(model);

            // Act
            using var eventBus = new EventBus(connection, "TestExchange");

            // Assert
            connectionMock.Verify(mock => mock.CreateModel());
        }

        [TestMethod]
        public void DisposingShouldCallDisposeOnModel()
        {
            // Arrange
            var modelMock = new Mock<IModel>();
            var model = modelMock.Object;
            var connectionMock = new Mock<IConnection>();
            var connection = connectionMock.Object;
            connectionMock.Setup(mock => mock.CreateModel()).Returns(model);
            using var eventBus = new EventBus(connection, "TestExchange");

            // Act
            eventBus.Dispose();

            // Assert
            modelMock.Verify(mock => mock.Dispose());
        }

        [TestMethod]
        public void DisposingShouldCallDisposeOnConnection()
        {
            // Arrange
            var modelMock = new Mock<IModel>();
            var model = modelMock.Object;
            var connectionMock = new Mock<IConnection>();
            var connection = connectionMock.Object;
            connectionMock.Setup(mock => mock.CreateModel()).Returns(model);
            using var eventBus = new EventBus(connection, "TestExchange");

            // Act
            eventBus.Dispose();

            // Assert
            connectionMock.Verify(mock => mock.Dispose());
        }

        [TestMethod]
        public void AddEventListenerShouldCallQueueDeclare()
        {
            // Arrange
            var modelMock = new Mock<IModel>();
            var model = modelMock.Object;
            var connectionMock = new Mock<IConnection>();
            var connection = connectionMock.Object;
            connectionMock.Setup(mock => mock.CreateModel()).Returns(model);
            modelMock.Setup(mock => mock.QueueDeclare(string.Empty, false, true, true, null))
                .Returns(new QueueDeclareOk("TestQueue", 0, 1));
            using var eventBus = new EventBus(connection, "TestExchange");

            // Act
            eventBus.AddEventListener(new Mock<IEventListener>().Object, "#");

            // Assert
            modelMock.Verify(mock => mock.QueueDeclare(string.Empty, false, true, true, null));
        }

        [TestMethod]
        public void AddEventListenerShouldCallQueueBind()
        {
            // Arrange
            var modelMock = new Mock<IModel>();
            var model = modelMock.Object;
            var connectionMock = new Mock<IConnection>();
            var connection = connectionMock.Object;
            connectionMock.Setup(mock => mock.CreateModel()).Returns(model);
            modelMock.Setup(mock => mock.QueueDeclare(string.Empty, false, true, true, null))
                .Returns(new QueueDeclareOk("TestQueue", 0, 1));
            using var eventBus = new EventBus(connection, "TestExchange");

            // Act
            eventBus.AddEventListener(new Mock<IEventListener>().Object, "#");

            // Assert
            modelMock.Verify(mock => mock.QueueBind("TestQueue", "TestExchange", "#", null));
        }

        [TestMethod]
        public void AddEventListenerShouldCallBasicConsume()
        {
            // Arrange
            var modelMock = new Mock<IModel>();
            var model = modelMock.Object;
            var connectionMock = new Mock<IConnection>();
            var connection = connectionMock.Object;
            connectionMock.Setup(mock => mock.CreateModel()).Returns(model);
            modelMock.Setup(mock => mock.QueueDeclare(string.Empty, false, true, true, null))
                .Returns(new QueueDeclareOk("TestQueue", 0, 1));
            using var eventBus = new EventBus(connection, "TestExchange");

            // Act
            eventBus.AddEventListener(new Mock<IEventListener>().Object, "#");

            // Assert
            modelMock.Verify(mock =>
                mock.BasicConsume("TestQueue", false, string.Empty, false, false, null, It.IsAny<IBasicConsumer>()));
        }

        [TestMethod]
        public void AddEventListenerShould()
        {
            // Arrange
            IBasicConsumer basicConsumer = null;
            var modelMock = new Mock<IModel>();
            var model = modelMock.Object;
            var connectionMock = new Mock<IConnection>();
            var connection = connectionMock.Object;
            connectionMock.Setup(mock => mock.CreateModel()).Returns(model);
            modelMock.Setup(mock => mock.QueueDeclare(string.Empty, false, true, true, null))
                .Returns(new QueueDeclareOk("TestQueue", 0, 1));
            modelMock.Setup(mock =>
                    mock.BasicConsume("TestQueue", false, string.Empty, false, false, null, It.IsAny<IBasicConsumer>()))
                .Callback((
                    string queue,
                    bool autoAck,
                    string consumerTag,
                    bool noLocal,
                    bool exclusive,
                    IDictionary<string, object> arguments,
                    IBasicConsumer consumer) =>
                {
                    basicConsumer = consumer;
                });
            using var eventBus = new EventBus(connection, "TestExchange");
            var eventListener = new Mock<IEventListener>().Object;
            
            // Act
            eventBus.AddEventListener(eventListener, "#");
            
            // Assert
            Assert.IsNotNull(basicConsumer);
        }
    }
}