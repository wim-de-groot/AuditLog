using System;
using System.Collections.Generic;
using AuditLog.Abstractions;
using AuditLog.Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Framing;

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
        public void DisposingTwiceShouldNotCallDisposeTwiceOnConnectionOrModel()
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
            eventBus.Dispose();

            // Assert
            connectionMock.Verify(mock => mock.Dispose(), Times.AtMost(1));
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
        public void AddEventListenerShouldHaveBasicConsumer()
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

        [TestMethod]
        public void AddCommandListenerShouldCallQueueDeclare()
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
            eventBus.AddCommandListener(new Mock<ICommandListener>().Object, "TestQueue");

            // Assert
            modelMock.Verify(mock => mock.QueueDeclare("TestQueue", true, false, false, null));
        }

        [TestMethod]
        public void AddCommandListenerShouldCallBasicConsume()
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
            eventBus.AddCommandListener(new Mock<ICommandListener>().Object, "TestQueue");

            // Assert
            modelMock.Verify(mock =>
                mock.BasicConsume("TestQueue", false, It.IsAny<string>(), false, false, null,
                    It.IsAny<IBasicConsumer>()));
        }

        [TestMethod]
        public void AddCommandListenerShouldCallBasicConsumeWithConsumerTagOfTypeGuid()
        {
            // Arrange
            var consumerTagAsGuid = Guid.Empty;
            var modelMock = new Mock<IModel>();
            var model = modelMock.Object;
            var connectionMock = new Mock<IConnection>();
            var connection = connectionMock.Object;
            connectionMock.Setup(mock => mock.CreateModel()).Returns(model);
            modelMock.Setup(mock => mock.QueueDeclare(string.Empty, false, true, true, null))
                .Returns(new QueueDeclareOk("TestQueue", 0, 1));
            modelMock.Setup(mock => mock.BasicConsume("TestQueue", false, It.IsAny<string>(), false, false, null,
                    It.IsAny<IBasicConsumer>()))
                .Callback((
                    string queue,
                    bool autoAck,
                    string consumerTag,
                    bool noLocal,
                    bool exclusive,
                    IDictionary<string, object> arguments,
                    IBasicConsumer consumer) =>
                {
                    consumerTagAsGuid = new Guid(consumerTag);
                });
            using var eventBus = new EventBus(connection, "TestExchange");

            // Act
            eventBus.AddCommandListener(new Mock<ICommandListener>().Object, "TestQueue");

            // Assert
            Assert.AreNotEqual(Guid.Empty, consumerTagAsGuid);
        }

        [TestMethod]
        public void AddCommandListenerShouldHaveBasicConsumer()
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
                    mock.BasicConsume("TestQueue", false, It.IsAny<string>(), false, false, null,
                        It.IsAny<IBasicConsumer>()))
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

            // Act
            eventBus.AddCommandListener(new Mock<ICommandListener>().Object, "TestQueue");

            // Assert
            Assert.IsNotNull(basicConsumer);
        }

        [TestMethod]
        public void PublishCommandShouldCallBasicPublish()
        {
            // Arrange
            var modelMock = new Mock<IModel>();
            var model = modelMock.Object;
            var connectionMock = new Mock<IConnection>();
            var connection = connectionMock.Object;
            connectionMock.Setup(mock => mock.CreateModel()).Returns(model);
            modelMock.Setup(mock => mock.CreateBasicProperties()).Returns(new BasicProperties());
            using var eventBus = new EventBus(connection, "TestExchange");

            // Act
            eventBus.PublishCommand(new ReplayEventsCommand());

            // Assert
            modelMock.Verify(mock =>
                mock.BasicPublish(string.Empty, "AuditLog", false, It.IsAny<IBasicProperties>(), It.IsAny<byte[]>()));
        }

        [TestMethod]
        public void PublishCommandShouldCreateBasicPropertiesWithTypeOfReplayEventsCommand()
        {
            // Arrange
            IBasicProperties properties = null;
            var modelMock = new Mock<IModel>();
            var model = modelMock.Object;
            var connectionMock = new Mock<IConnection>();
            var connection = connectionMock.Object;
            connectionMock.Setup(mock => mock.CreateModel()).Returns(model);
            modelMock.Setup(mock => mock.CreateBasicProperties()).Returns(new BasicProperties());
            modelMock.Setup(mock =>
                    mock.BasicPublish(string.Empty, "AuditLog", false, It.IsAny<IBasicProperties>(),
                        It.IsAny<byte[]>()))
                .Callback((string ExchangeType, string routingKey, bool mandatory, IBasicProperties basicProperties,
                    byte[] body) =>
                {
                    properties = basicProperties;
                });
            using var eventBus = new EventBus(connection, "TestExchange");

            // Act
            eventBus.PublishCommand(new ReplayEventsCommand());

            // Assert
            Assert.AreEqual("ReplayEventsCommand", properties.Type);
        }

        [TestMethod]
        public void AddCommandListenerShouldAddHandleToConsumerReceived()
        {
            // Arrange
            IBasicConsumer basicConsumer = null;
            var modelMock = new Mock<IModel>();
            var model = modelMock.Object;
            var connectionMock = new Mock<IConnection>();
            var connection = connectionMock.Object;
            var commandListenerMock = new Mock<ICommandListener>();
            connectionMock.Setup(mock => mock.CreateModel()).Returns(model);
            modelMock.Setup(mock => mock.QueueDeclare(string.Empty, false, true, true, null))
                .Returns(new QueueDeclareOk("TestQueue", 0, 1));
            modelMock.Setup(mock =>
                    mock.BasicConsume("TestQueue", false, It.IsAny<string>(), false, false, null,
                        It.IsAny<IBasicConsumer>()))
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

            // Act
            eventBus.AddCommandListener(commandListenerMock.Object, "TestQueue");
            basicConsumer.HandleBasicDeliver("", ulong.MaxValue, false, "TestExchange", "Test.Test.*",
                new BasicProperties(), It.IsAny<byte[]>());


            // Assert
            commandListenerMock.Verify(mock => mock.Handle(It.IsAny<object>(), It.IsAny<BasicDeliverEventArgs>()));
        }
        
        [TestMethod]
        public void AddEventListenerShouldAddHandleToConsumerReceived()
        {
            // Arrange
            IBasicConsumer basicConsumer = null;
            var modelMock = new Mock<IModel>();
            var model = modelMock.Object;
            var connectionMock = new Mock<IConnection>();
            var connection = connectionMock.Object;
            var eventListenerMock = new Mock<IEventListener>();
            connectionMock.Setup(mock => mock.CreateModel()).Returns(model);
            modelMock.Setup(mock => mock.QueueDeclare(string.Empty, false, true, true, null))
                .Returns(new QueueDeclareOk("TestQueue", 0, 1));
            modelMock.Setup(mock =>
                    mock.BasicConsume("TestQueue", false, It.IsAny<string>(), false, false, null,
                        It.IsAny<IBasicConsumer>()))
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

            // Act
            eventBus.AddEventListener(eventListenerMock.Object, "#");
            basicConsumer.HandleBasicDeliver("", ulong.MaxValue, false, "TestExchange", "#",
                new BasicProperties(), It.IsAny<byte[]>());


            // Assert
            eventListenerMock.Verify(mock => mock.Handle(It.IsAny<object>(), It.IsAny<BasicDeliverEventArgs>()));
        }
    }
}