using System;
using System.Collections.Generic;
using System.Text;
using AuditLog.Abstractions;
using AuditLog.Domain;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RabbitMQ.Client;
using RabbitMQ.Client.Framing;

namespace AuditLog.Test
{
    [TestClass]
    public class EventReplayerTest
    {
        [TestInitialize]
        public void TestInitialize()
        {
            AuditLogLoggerFactory.LoggerFactory = NullLoggerFactory.Instance;
        }
        
        [TestMethod]
        public void ReplayLogEntryCallsCreateBasicProperties()
        {
            // Arrange
            var properties = new BasicProperties();
            var channelMock = new Mock<IModel>();
            var channel = channelMock.Object;
            var eventBusMock = new Mock<IEventBus>();
            var eventBus = eventBusMock.Object;
            var eventReplayer = new EventReplayer(eventBus);
            eventBusMock.Setup(mock => mock.ExchangeName).Returns("TestExchange");
            eventBusMock.Setup(mock => mock.Connection.CreateModel()).Returns(channel);
            channelMock.Setup(mock => mock.CreateBasicProperties()).Returns(properties);

            // Act
            eventReplayer.ReplayLogEntry(new LogEntry
            {
                Id = 1,
                EventJson = "",
                EventType = "DomainEvent",
                RoutingKey = "Test.*",
                Timestamp = new DateTime(2019, 7, 6).Ticks
            });
            
            // Assert
            channelMock.Verify(mock => mock.CreateBasicProperties());
        }

        [TestMethod]
        public void ReplayLogEntryCallsBasicPublish()
        {
            // Arrange
            var properties = new BasicProperties();
            var channelMock = new Mock<IModel>();
            var channel = channelMock.Object;
            var eventBusMock = new Mock<IEventBus>();
            var eventBus = eventBusMock.Object;
            var eventReplayer = new EventReplayer(eventBus);
            eventBusMock.Setup(mock => mock.ExchangeName).Returns("TestExchange");
            eventBusMock.Setup(mock => mock.Connection.CreateModel()).Returns(channel);
            channelMock.Setup(mock => mock.CreateBasicProperties()).Returns(properties);
            
            // Act
            eventReplayer.ReplayLogEntry(new LogEntry
            {
                Id = 1,
                EventJson = "{'title': 'Something'}",
                EventType = "DomainEvent",
                RoutingKey = "Test.*",
                Timestamp = new DateTime(2019, 7, 6).Ticks
            });
            
            // Assert
            channelMock.Verify(mock => mock
                .BasicPublish(
                    string.Empty, 
                    "Replay.Test.*", 
                    false,
                    properties, 
                    It.IsAny<byte[]>())
            );
        }

        [TestMethod]
        public void ReplayLogEntryBasicPublishesTheRightValues()
        {
            // Arrange
            var exchangeName = "";
            long timestamp = 0;
            var type = "";
            var queueName = "";
            byte[] buffer = null;
            
            var properties = new BasicProperties();
            var channelMock = new Mock<IModel>();
            var channel = channelMock.Object;
            var eventBusMock = new Mock<IEventBus>();
            var eventBus = eventBusMock.Object;
            var eventReplayer = new EventReplayer(eventBus);
            eventBusMock.Setup(mock => mock.ExchangeName).Returns("TestExchange");
            eventBusMock.Setup(mock => mock.Connection.CreateModel()).Returns(channel);
            channelMock.Setup(mock => mock.CreateBasicProperties()).Returns(properties);
            channelMock.Setup(mock => mock
                    .BasicPublish(
                        string.Empty, 
                        "Replay.Test.*",
                        false,
                        properties,
                        It.IsAny<byte[]>()))
                .Callback((
                    string exchange,
                    string routingKey,
                    bool mandatory,
                    IBasicProperties basicProperties,
                    byte[] body
                ) =>
                {
                    exchangeName = exchange;
                    timestamp = basicProperties.Timestamp.UnixTime;
                    queueName = routingKey;
                    type = basicProperties.Type;
                    buffer = body;
                });
            
            // Act
            eventReplayer.ReplayLogEntry(new LogEntry
            {
                Id = 1,
                EventJson = "{'title': 'Something'}",
                EventType = "DomainEvent",
                RoutingKey = "Test.*",
                Timestamp = new DateTime(2019, 7, 6).Ticks
            });
            
            // Assert
            Assert.AreEqual(string.Empty, exchangeName);
            Assert.AreEqual("DomainEvent", type);
            Assert.AreEqual("Replay.Test.*", queueName);
            Assert.AreEqual(new DateTime(2019, 7, 6).Ticks, timestamp);
            Assert.AreEqual("{'title': 'Something'}", Encoding.Unicode.GetString(buffer));
        }

        [TestMethod]
        public void ReplayLogEntriesCallsReplayLogEntryTwiceForListOfTwoEntries()
        {
            // Arrange
            var properties = new BasicProperties();
            var channelMock = new Mock<IModel>();
            var channel = channelMock.Object;
            var eventBusMock = new Mock<IEventBus>();
            var eventBus = eventBusMock.Object;
            var eventReplayer = new EventReplayer(eventBus);
            eventBusMock.Setup(mock => mock.ExchangeName).Returns("TestExchange");
            eventBusMock.Setup(mock => mock.Connection.CreateModel()).Returns(channel);
            channelMock.Setup(mock => mock.CreateBasicProperties()).Returns(properties);
            
            // Act
            eventReplayer.ReplayLogEntries(new List<LogEntry>
            {
                new LogEntry
                {
                    Id = 1,
                    EventJson = "{'title': 'Something'}",
                    EventType = "DomainEvent",
                    RoutingKey = "Test.*",
                    Timestamp = new DateTime(2019, 7, 6).Ticks
                },
                new LogEntry
                {
                    Id = 2,
                    EventJson = "{'title': 'nog Something'}",
                    EventType = "DomainEvent",
                    RoutingKey = "Test.*",
                    Timestamp = new DateTime(2019, 7, 6).Ticks
                }
            });
            
            // Assert
            channelMock.Verify(mock => mock
                .BasicPublish(
                    string.Empty,
                    "Replay.Test.*",
                    false,
                    properties,
                    It.IsAny<byte[]>()), Times.Exactly(2));
        }

        [TestMethod]
        public void RegisterReplayExchangeShouldSetTheWhereTheEventsAreReplayedTo()
        {
            // Arrange
            var properties = new BasicProperties();
            var channelMock = new Mock<IModel>();
            var channel = channelMock.Object;
            var eventBusMock = new Mock<IEventBus>();
            var eventBus = eventBusMock.Object;
            var eventReplayer = new EventReplayer(eventBus);
            eventBusMock.Setup(mock => mock.ExchangeName).Returns("TestExchange");
            eventBusMock.Setup(mock => mock.Connection.CreateModel()).Returns(channel);
            channelMock.Setup(mock => mock.CreateBasicProperties()).Returns(properties);
            
            // Act
            eventReplayer.RegisterReplayExchange("AuditLog.TestExchange");
            eventReplayer.ReplayLogEntries(new List<LogEntry>
            {
                new LogEntry
                {
                    Id = 1,
                    EventJson = "{'title': 'Something'}",
                    EventType = "DomainEvent",
                    RoutingKey = "Test.*",
                    Timestamp = new DateTime(2019, 7, 6).Ticks
                },
                new LogEntry
                {
                    Id = 2,
                    EventJson = "{'title': 'nog Something'}",
                    EventType = "DomainEvent",
                    RoutingKey = "Test.*",
                    Timestamp = new DateTime(2019, 7, 6).Ticks
                }
            });
            
            // Assert
            channelMock.Verify(mock => mock
                .BasicPublish(
                    "AuditLog.TestExchange",
                    "Replay.Test.*",
                    false,
                    properties,
                    It.IsAny<byte[]>()), Times.Exactly(2));
        }
    }
}