using System;
using System.Collections.Generic;
using System.Text;
using AuditLog.Abstractions;
using AuditLog.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Framing;

namespace AuditLog.Test
{
    [TestClass]
    public class AuditLogCommandListenerTest
    {
        [TestInitialize]
        public void TestInitialize()
        {
            AuditLogLoggerFactory.LoggerFactory = NullLoggerFactory.Instance;
        }

        [TestMethod]
        public void ShouldImplementICommandListener()
        {
            // Arrange
            var eventReplayerMock = new Mock<IEventReplayer>();
            var repositoryMock = new Mock<IAuditLogRepository<LogEntry, long>>();
            var repository = repositoryMock.Object;
            var eventReplayer = eventReplayerMock.Object;
            var routingKeyMatcherMock = new Mock<IRoutingKeyMatcher>();
            var routingKeyMatcher = routingKeyMatcherMock.Object;
            var eventBusMock = new Mock<IEventBus>();
            var eventBus = eventBusMock.Object;
            var modelMock = new Mock<IModel>();
            var model = modelMock.Object;
            eventBusMock.Setup(mock => mock.Model).Returns(model);

            // Act
            var commandListener = new AuditLogCommandListener(repository, eventReplayer, routingKeyMatcher, eventBus);

            // Assert
            Assert.IsInstanceOfType(commandListener, typeof(ICommandListener));
        }

        [TestMethod]
        public void ReplayEventsReturnsReplayEventsResponse()
        {
            // Arrange
            var sender = new object();
            var basicDeliverEventArgs = new BasicDeliverEventArgs
            {
                BasicProperties = new BasicProperties
                {
                    Type = "SomeCommand",
                    Timestamp = new AmqpTimestamp(new DateTime(2019, 6, 4).Ticks),
                },
                Body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new ReplayEventsCommand
                {
                    EventType = "DomainEvent",
                    RoutingKey = "Test.*"
                })),
                RoutingKey = "TestQueue"
            };
            var eventReplayerMock = new Mock<IEventReplayer>();
            var repositoryMock = new Mock<IAuditLogRepository<LogEntry, long>>();
            var repository = repositoryMock.Object;
            var eventReplayer = eventReplayerMock.Object;
            var routingKeyMatcherMock = new Mock<IRoutingKeyMatcher>();
            var routingKeyMatcher = routingKeyMatcherMock.Object;
            var eventBusMock = new Mock<IEventBus>();
            var eventBus = eventBusMock.Object;
            var modelMock = new Mock<IModel>();
            var model = modelMock.Object;
            eventBusMock.Setup(mock => mock.Model).Returns(model);
            var commandListener = new AuditLogCommandListener(repository, eventReplayer, routingKeyMatcher, eventBus);

            // Act
            commandListener.Handle(sender, basicDeliverEventArgs);

            // Assert
            eventBusMock.Verify(mock => mock.PublishCommand(It.IsAny<ReplayEventsResponse>()));
        }

        [TestMethod]
        public void ReplayEventCallsFindByOnRepository()
        {
            // Arrange
            var sender = new object();
            var basicDeliverEventArgs = new BasicDeliverEventArgs
            {
                BasicProperties = new BasicProperties
                {
                    Type = "SomeCommand",
                    Timestamp = new AmqpTimestamp(new DateTime(2019, 6, 4).Ticks),
                },
                Body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new ReplayEventsCommand
                {
                    EventType = "DomainEvent",
                    RoutingKey = "Test.*"
                })),
                RoutingKey = "TestQueue"
            };
            var eventReplayerMock = new Mock<IEventReplayer>();
            var repositoryMock = new Mock<IAuditLogRepository<LogEntry, long>>();
            var repository = repositoryMock.Object;
            var eventReplayer = eventReplayerMock.Object;
            var routingKeyMatcherMock = new Mock<IRoutingKeyMatcher>();
            var routingKeyMatcher = routingKeyMatcherMock.Object;
            var eventBusMock = new Mock<IEventBus>();
            var eventBus = eventBusMock.Object;
            var modelMock = new Mock<IModel>();
            var model = modelMock.Object;
            eventBusMock.Setup(mock => mock.Model).Returns(model);
            var commandListener = new AuditLogCommandListener(repository, eventReplayer, routingKeyMatcher, eventBus);

            // Act
            commandListener.Handle(sender, basicDeliverEventArgs);

            // Assert
            repositoryMock.Verify(mock => mock.FindBy(It.IsAny<LogEntryCriteria>()));
        }

        [TestMethod]
        public void ReplayEventCallsFindByOnRepositoryWithRightCriteria()
        {
            // Arrange
            var logEntryCriteria = new LogEntryCriteria();
            var sender = new object();
            var basicDeliverEventArgs = new BasicDeliverEventArgs
            {
                BasicProperties = new BasicProperties
                {
                    Type = "SomeCommand",
                    Timestamp = new AmqpTimestamp(new DateTime(2019, 6, 4).Ticks),
                },
                Body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new ReplayEventsCommand
                {
                    EventType = "DomainEvent",
                    RoutingKey = "Test.*"
                })),
                RoutingKey = "TestQueue"
            };
            var eventReplayerMock = new Mock<IEventReplayer>();
            var repositoryMock = new Mock<IAuditLogRepository<LogEntry, long>>();
            var repository = repositoryMock.Object;
            var eventReplayer = eventReplayerMock.Object;
            var routingKeyMatcherMock = new Mock<IRoutingKeyMatcher>();
            var routingKeyMatcher = routingKeyMatcherMock.Object;
            var eventBusMock = new Mock<IEventBus>();
            var eventBus = eventBusMock.Object;
            var modelMock = new Mock<IModel>();
            var model = modelMock.Object;
            eventBusMock.Setup(mock => mock.Model).Returns(model);
            var commandListener = new AuditLogCommandListener(repository, eventReplayer, routingKeyMatcher, eventBus);
            repositoryMock.Setup(mock => mock.FindBy(It.IsAny<LogEntryCriteria>()))
                .Callback((LogEntryCriteria criteria) => logEntryCriteria = criteria);

            // Act
            commandListener.Handle(sender, basicDeliverEventArgs);

            //
            Assert.AreEqual("DomainEvent", logEntryCriteria.EventType);
            Assert.AreEqual("Test.*", logEntryCriteria.RoutingKey);
            Assert.AreEqual(null, logEntryCriteria.FromTimestamp);
            Assert.AreEqual(null, logEntryCriteria.ToTimestamp);
        }

        [TestMethod]
        public void ReplayEventsCallsReplayLogEntriesOnEventReplayer()
        {
            // Arrange
            var sender = new object();
            var basicDeliverEventArgs = new BasicDeliverEventArgs
            {
                BasicProperties = new BasicProperties
                {
                    Type = "SomeCommand",
                    Timestamp = new AmqpTimestamp(new DateTime(2019, 6, 4).Ticks),
                },
                Body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new ReplayEventsCommand
                {
                    EventType = "DomainEvent",
                    RoutingKey = "Test.*"
                })),
                RoutingKey = "TestQueue"
            };
            var eventReplayerMock = new Mock<IEventReplayer>();
            var repositoryMock = new Mock<IAuditLogRepository<LogEntry, long>>();
            var repository = repositoryMock.Object;
            var eventReplayer = eventReplayerMock.Object;
            var routingKeyMatcherMock = new Mock<IRoutingKeyMatcher>();
            var routingKeyMatcher = routingKeyMatcherMock.Object;
            var eventBusMock = new Mock<IEventBus>();
            var eventBus = eventBusMock.Object;
            var modelMock = new Mock<IModel>();
            var model = modelMock.Object;
            eventBusMock.Setup(mock => mock.Model).Returns(model);
            var commandListener = new AuditLogCommandListener(repository, eventReplayer, routingKeyMatcher, eventBus);

            // Act
            commandListener.Handle(sender, basicDeliverEventArgs);

            // Assert
            eventReplayerMock.Verify(mock => mock.ReplayLogEntries(It.IsAny<IEnumerable<LogEntry>>()));
        }

        [TestMethod]
        public void ReplayEventsReturnsReplayEventResponse200()
        {
            // Arrange
            var result = new ReplayEventsResponse();
            var sender = new object();
            var basicDeliverEventArgs = new BasicDeliverEventArgs
            {
                BasicProperties = new BasicProperties
                {
                    Type = "SomeCommand",
                    Timestamp = new AmqpTimestamp(new DateTime(2019, 6, 4).Ticks),
                },
                Body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new ReplayEventsCommand
                {
                    EventType = "DomainEvent",
                    RoutingKey = "Test.*"
                })),
                RoutingKey = "TestQueue"
            };
            var eventReplayerMock = new Mock<IEventReplayer>();
            var repositoryMock = new Mock<IAuditLogRepository<LogEntry, long>>();
            var repository = repositoryMock.Object;
            var eventReplayer = eventReplayerMock.Object;
            var routingKeyMatcherMock = new Mock<IRoutingKeyMatcher>();
            var routingKeyMatcher = routingKeyMatcherMock.Object;
            var eventBusMock = new Mock<IEventBus>();
            var eventBus = eventBusMock.Object;
            var modelMock = new Mock<IModel>();
            var model = modelMock.Object;
            eventBusMock.Setup(mock => mock.Model).Returns(model);
            eventBusMock.Setup(mock => mock.PublishCommand(It.IsAny<ReplayEventsResponse>()))
                .Callback((DomainCommand response) => result = response as ReplayEventsResponse);
            var commandListener = new AuditLogCommandListener(repository, eventReplayer, routingKeyMatcher, eventBus);

            // Act
            commandListener.Handle(sender, basicDeliverEventArgs);

            // Assert
            Assert.AreEqual(StatusCodes.Status200OK, result.Code);
            Assert.AreEqual("OK", result.Status);
        }

        [TestMethod]
        public void ReplayEventsReturnsReplayEventResponse500()
        {
            // Arrange
            var result = new ReplayEventsResponse();
            var sender = new object();
            var basicDeliverEventArgs = new BasicDeliverEventArgs
            {
                BasicProperties = new BasicProperties
                {
                    Type = "SomeCommand",
                    Timestamp = new AmqpTimestamp(new DateTime(2019, 6, 4).Ticks),
                },
                Body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new ReplayEventsCommand
                {
                    EventType = "DomainEvent",
                    RoutingKey = "Test.*"
                })),
                RoutingKey = "TestQueue"
            };
            var eventReplayerMock = new Mock<IEventReplayer>();
            var repositoryMock = new Mock<IAuditLogRepository<LogEntry, long>>();
            var repository = repositoryMock.Object;
            var eventReplayer = eventReplayerMock.Object;
            var routingKeyMatcherMock = new Mock<IRoutingKeyMatcher>();
            var routingKeyMatcher = routingKeyMatcherMock.Object;
            var eventBusMock = new Mock<IEventBus>();
            var eventBus = eventBusMock.Object;
            var modelMock = new Mock<IModel>();
            var model = modelMock.Object;
            eventBusMock.Setup(mock => mock.Model).Returns(model);
            eventBusMock.Setup(mock => mock.PublishCommand(It.IsAny<ReplayEventsResponse>()))
                .Callback((DomainCommand response) => result = response as ReplayEventsResponse);
            var commandListener = new AuditLogCommandListener(repository, eventReplayer, routingKeyMatcher, eventBus);
            repositoryMock.Setup(mock => mock.FindBy(It.IsAny<LogEntryCriteria>()))
                .Throws(new Exception());

            // Act
            commandListener.Handle(sender, basicDeliverEventArgs);

            // Assert
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.Code);
            Assert.AreEqual("Internal Error", result.Status);
        }

        [TestMethod]
        public void ReplayEventsCallsIsMatchOnRoutingKeyMatcher()
        {
            // Arrange
            var sender = new object();
            var basicDeliverEventArgs = new BasicDeliverEventArgs
            {
                BasicProperties = new BasicProperties
                {
                    Type = "SomeCommand",
                    Timestamp = new AmqpTimestamp(new DateTime(2019, 6, 4).Ticks),
                },
                Body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new ReplayEventsCommand
                {
                    EventType = "DomainEvent",
                    RoutingKey = "Test.*"
                })),
                RoutingKey = "TestQueue"
            };
            var eventReplayerMock = new Mock<IEventReplayer>();
            var repositoryMock = new Mock<IAuditLogRepository<LogEntry, long>>();
            var repository = repositoryMock.Object;
            var eventReplayer = eventReplayerMock.Object;
            var routingKeyMatcherMock = new Mock<IRoutingKeyMatcher>();
            var routingKeyMatcher = routingKeyMatcherMock.Object;
            var eventBusMock = new Mock<IEventBus>();
            var eventBus = eventBusMock.Object;
            var modelMock = new Mock<IModel>();
            var model = modelMock.Object;
            eventBusMock.Setup(mock => mock.Model).Returns(model);
            var commandListener = new AuditLogCommandListener(repository, eventReplayer, routingKeyMatcher, eventBus);
            repositoryMock.Setup(mock => mock.FindBy(It.IsAny<LogEntryCriteria>()))
                .Returns(new List<LogEntry>
                {
                    new LogEntry
                    {
                        Timestamp = new DateTime(2019, 5, 8).Ticks,
                        RoutingKey = "Test.*",
                        EventJson = "{'title': 'Some title'}",
                        EventType = "DomainEvent"
                    },
                    new LogEntry
                    {
                        Timestamp = new DateTime(2019, 7, 2).Ticks,
                        RoutingKey = "Test.#",
                        EventJson = "{'title': 'Some title'}",
                        EventType = "DomainEvent"
                    }
                });

            // Act
            commandListener.Handle(sender, basicDeliverEventArgs);

            // Assert
            routingKeyMatcherMock.Verify(mock => mock.IsMatch("Test.*", It.IsAny<string>()));
        }

        [TestMethod]
        public void ReplayEventsShouldCallRegisterReplayExchange()
        {
            // Arrange
            var sender = new object();
            var basicDeliverEventArgs = new BasicDeliverEventArgs
            {
                BasicProperties = new BasicProperties
                {
                    Type = "SomeCommand",
                    Timestamp = new AmqpTimestamp(new DateTime(2019, 6, 4).Ticks),
                },
                Body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new ReplayEventsCommand
                {
                    EventType = "DomainEvent",
                    RoutingKey = "Test.*",
                    ReplayExchangeName = "AuditLog.TestExchange"
                })),
                RoutingKey = "TestQueue",
            };
            var eventReplayerMock = new Mock<IEventReplayer>();
            var repositoryMock = new Mock<IAuditLogRepository<LogEntry, long>>();
            var repository = repositoryMock.Object;
            var eventReplayer = eventReplayerMock.Object;
            var routingKeyMatcherMock = new Mock<IRoutingKeyMatcher>();
            var routingKeyMatcher = routingKeyMatcherMock.Object;
            var eventBusMock = new Mock<IEventBus>();
            var eventBus = eventBusMock.Object;
            var modelMock = new Mock<IModel>();
            var model = modelMock.Object;
            eventBusMock.Setup(mock => mock.Model).Returns(model);
            var commandListener = new AuditLogCommandListener(repository, eventReplayer, routingKeyMatcher, eventBus);
            repositoryMock.Setup(mock => mock.FindBy(It.IsAny<LogEntryCriteria>()))
                .Returns(new List<LogEntry>
                {
                    new LogEntry
                    {
                        Timestamp = new DateTime(2019, 5, 8).Ticks,
                        RoutingKey = "Test.*",
                        EventJson = "{'title': 'Some title'}",
                        EventType = "DomainEvent"
                    },
                    new LogEntry
                    {
                        Timestamp = new DateTime(2019, 7, 2).Ticks,
                        RoutingKey = "Test.#",
                        EventJson = "{'title': 'Some title'}",
                        EventType = "DomainEvent"
                    }
                });

            // Act
            commandListener.Handle(sender, basicDeliverEventArgs);

            // Assert
            eventReplayerMock.Verify(mock => mock.RegisterReplayExchange("AuditLog.TestExchange"));
        }

        [TestMethod]
        public void HandleShouldCallBasicAck()
        {
            // Arrange
            var sender = new object();
            var basicDeliverEventArgs = new BasicDeliverEventArgs
            {
                BasicProperties = new BasicProperties
                {
                    Type = "SomeCommand",
                    Timestamp = new AmqpTimestamp(new DateTime(2019, 6, 4).Ticks),
                },
                Body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new ReplayEventsCommand
                {
                    EventType = "DomainEvent",
                    RoutingKey = "Test.*",
                    ReplayExchangeName = "AuditLog.TestExchange"
                })),
                RoutingKey = "TestQueue",
            };
            var eventReplayerMock = new Mock<IEventReplayer>();
            var repositoryMock = new Mock<IAuditLogRepository<LogEntry, long>>();
            var repository = repositoryMock.Object;
            var eventReplayer = eventReplayerMock.Object;
            var routingKeyMatcherMock = new Mock<IRoutingKeyMatcher>();
            var routingKeyMatcher = routingKeyMatcherMock.Object;
            var eventBusMock = new Mock<IEventBus>();
            var eventBus = eventBusMock.Object;
            var modelMock = new Mock<IModel>();
            var model = modelMock.Object;
            eventBusMock.Setup(mock => mock.Model).Returns(model);
            var commandListener = new AuditLogCommandListener(repository, eventReplayer, routingKeyMatcher, eventBus);
            repositoryMock.Setup(mock => mock.FindBy(It.IsAny<LogEntryCriteria>()))
                .Returns(new List<LogEntry>
                {
                    new LogEntry
                    {
                        Timestamp = new DateTime(2019, 5, 8).Ticks,
                        RoutingKey = "Test.*",
                        EventJson = "{'title': 'Some title'}",
                        EventType = "DomainEvent"
                    },
                    new LogEntry
                    {
                        Timestamp = new DateTime(2019, 7, 2).Ticks,
                        RoutingKey = "Test.#",
                        EventJson = "{'title': 'Some title'}",
                        EventType = "DomainEvent"
                    }
                });
            
            // Act
            commandListener.Handle(sender, basicDeliverEventArgs);
            
            // Assert
            modelMock.Verify(mock => mock.BasicAck(It.IsAny<ulong>(), false));
        }
    }
}