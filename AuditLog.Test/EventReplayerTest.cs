using System;
using System.Collections.Generic;
using System.Text;
using AuditLog.Domain;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Miffy;
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
            var busContextMock = new Mock<IBusContext<IConnection>>();
            var busContext = busContextMock.Object;
            var channelMock = new Mock<IModel>();
            var channel = channelMock.Object;
            var eventReplayer = new EventReplayer(busContext);
            busContextMock.Setup(mock => mock.ExchangeName).Returns("TestExchange");
            busContextMock.Setup(mock => mock.Connection.CreateModel()).Returns(channel);
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
            var busContextMock = new Mock<IBusContext<IConnection>>();
            var busContext = busContextMock.Object;
            var channelMock = new Mock<IModel>();
            var channel = channelMock.Object;
            var eventReplayer = new EventReplayer(busContext);
            busContextMock.Setup(mock => mock.ExchangeName).Returns("TestExchange");
            busContextMock.Setup(mock => mock.Connection.CreateModel()).Returns(channel);
            channelMock.Setup(mock => mock.CreateBasicProperties()).Returns(properties);
            
            // Act
            eventReplayer.ReplayLogEntry(new LogEntry
            {
                Id = 1,
                EventJson = "{'title': 'iets'}",
                EventType = "DomainEvent",
                RoutingKey = "Test.*",
                Timestamp = new DateTime(2019, 7, 6).Ticks
            });
            
            // Assert
            channelMock.Verify(mock => mock
                .BasicPublish(
                    "TestExchange", 
                    "Test.*", 
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
            var key = "";
            var type = "";
            byte[] buffer = null;
            
            var properties = new BasicProperties();
            var busContextMock = new Mock<IBusContext<IConnection>>();
            var busContext = busContextMock.Object;
            var channelMock = new Mock<IModel>();
            var channel = channelMock.Object;
            var eventReplayer = new EventReplayer(busContext);
            busContextMock.Setup(mock => mock.ExchangeName).Returns("TestExchange");
            busContextMock.Setup(mock => mock.Connection.CreateModel()).Returns(channel);
            channelMock.Setup(mock => mock.CreateBasicProperties()).Returns(properties);
            channelMock.Setup(mock => mock
                    .BasicPublish(
                        "TestExchange",
                        "Test.*",
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
                    key = routingKey;
                    type = basicProperties.Type;
                    buffer = body;
                });
            
            // Act
            eventReplayer.ReplayLogEntry(new LogEntry
            {
                Id = 1,
                EventJson = "{'title': 'iets'}",
                EventType = "DomainEvent",
                RoutingKey = "Test.*",
                Timestamp = new DateTime(2019, 7, 6).Ticks
            });
            
            // Assert
            Assert.AreEqual("TestExchange", exchangeName);
            Assert.AreEqual("DomainEvent", type);
            Assert.AreEqual("Test.*", key);
            Assert.AreEqual(new DateTime(2019, 7, 6).Ticks, timestamp);
            Assert.AreEqual("{'title': 'iets'}", Encoding.Unicode.GetString(buffer));
        }

        [TestMethod]
        public void ReplayLogEntriesCallsReplayLogEntryTwiceForListOfTwoEntries()
        {
            // Arrange
            var properties = new BasicProperties();
            var busContextMock = new Mock<IBusContext<IConnection>>();
            var busContext = busContextMock.Object;
            var channelMock = new Mock<IModel>();
            var channel = channelMock.Object;
            var eventReplayer = new EventReplayer(busContext);
            busContextMock.Setup(mock => mock.ExchangeName).Returns("TestExchange");
            busContextMock.Setup(mock => mock.Connection.CreateModel()).Returns(channel);
            channelMock.Setup(mock => mock.CreateBasicProperties()).Returns(properties);
            
            // Act
            eventReplayer.ReplayLogEntries(new List<LogEntry>
            {
                new LogEntry
                {
                    Id = 1,
                    EventJson = "{'title': 'iets'}",
                    EventType = "DomainEvent",
                    RoutingKey = "Test.*",
                    Timestamp = new DateTime(2019, 7, 6).Ticks
                },
                new LogEntry
                {
                    Id = 2,
                    EventJson = "{'title': 'nog iets'}",
                    EventType = "DomainEvent",
                    RoutingKey = "Test.*",
                    Timestamp = new DateTime(2019, 7, 6).Ticks
                }
            });
            
            // Assert
            channelMock.Verify(mock => mock
                .BasicPublish(
                    "TestExchange",
                    "Test.*",
                    false,
                    properties,
                    It.IsAny<byte[]>()), Times.Exactly(2));

        }
    }
}