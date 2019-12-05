using System;
using System.Collections.Generic;
using System.Linq;
using AuditLog.Abstractions;
using AuditLog.Domain;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Miffy.MicroServices.Events;
using Moq;

namespace AuditLog.Test
{
    [TestClass]
    public class AuditLogCommandListenerTest
    {
        [TestMethod]
        public void HasReplayEventsMethod()
        {
            // Arrange
            var type = typeof(AuditLogCommandListener);
            
            // Act
            var result = type.GetMethod("ReplayEvents");

            // Assert
            Assert.IsNotNull(result);
        }
        
        [TestMethod]
        public void ReplayEventsMethodHasCommandListenerAttribute()
        {
            // Arrange
            var attributes = typeof(AuditLogCommandListener)
                .GetMethod("ReplayEvents")
                ?.GetCustomAttributes(typeof(CommandListenerAttribute));

            // Act
            var result = attributes?.FirstOrDefault();
            
            // Assert
            Assert.IsNotNull(result);
        }
        
        [TestMethod]
        public void CommandListenerAttributeHasQueueName()
        {
            // Arrange
            var attributes = typeof(AuditLogCommandListener)
                .GetMethod("ReplayEvents")
                ?.GetCustomAttributes(typeof(CommandListenerAttribute));

            // Act
            var result = attributes?.FirstOrDefault() as CommandListenerAttribute;
            
            // Assert
            Assert.AreEqual("AuditLog",result?.QueueName);
        }
        
        [TestMethod]
        public void ReplayEventsHasOneParameter()
        {
            // Arrange
            var method = typeof(AuditLogCommandListener)
                .GetMethod("ReplayEvents");

            var result = method?.GetParameters();

            Assert.AreEqual(1,result?.Length);
        }
        
        [TestMethod]
        public void ReplayEventsReturnsReplayEventsResponse()
        {
            // Arrange
            var eventReplayerMock = new Mock<IEventReplayer>();
            var repositoryMock = new Mock<IAuditLogRepository<LogEntry, long>>();
            var repository = repositoryMock.Object;
            var eventReplayer = eventReplayerMock.Object;
            var routingKeyMatcherMock = new Mock<IRoutingKeyMatcher>();
            var routingKeyMatcher = routingKeyMatcherMock.Object;
            var commandListener = new AuditLogCommandListener(repository, eventReplayer, routingKeyMatcher);
            
            // Act
            var result = commandListener.ReplayEvents(new ReplayEventsCommand
            {
                EventType = "DomainEvent",
                RoutingKey = "Test.*"
            });

            // Assert
            Assert.IsInstanceOfType(result, typeof(ReplayEventsResponse));
        }

        [TestMethod]
        public void ReplayEventCallsFindByOnRepository()
        {
            // Arrange
            var eventReplayerMock = new Mock<IEventReplayer>();
            var repositoryMock = new Mock<IAuditLogRepository<LogEntry, long>>();
            var repository = repositoryMock.Object;
            var eventReplayer = eventReplayerMock.Object;
            var routingKeyMatcherMock = new Mock<IRoutingKeyMatcher>();
            var routingKeyMatcher = routingKeyMatcherMock.Object;
            var commandListener = new AuditLogCommandListener(repository, eventReplayer, routingKeyMatcher);
            
            // Act
            commandListener.ReplayEvents(new ReplayEventsCommand
            {
                EventType = "DomainEvent",
                RoutingKey = "Test.*"
            });
            
            //
            repositoryMock.Verify(mock => mock.FindBy(It.IsAny<LogEntryCriteria>()));
        }
        
        [TestMethod]
        public void ReplayEventCallsFindByOnRepositoryWithRightCriteria()
        {
            // Arrange
            LogEntryCriteria logEntryCriteria = null;
            var eventReplayerMock = new Mock<IEventReplayer>();
            var repositoryMock = new Mock<IAuditLogRepository<LogEntry, long>>();
            var repository = repositoryMock.Object;
            var eventReplayer = eventReplayerMock.Object;
            var routingKeyMatcherMock = new Mock<IRoutingKeyMatcher>();
            var routingKeyMatcher = routingKeyMatcherMock.Object;
            var commandListener = new AuditLogCommandListener(repository, eventReplayer, routingKeyMatcher);
            repositoryMock.Setup(mock => mock.FindBy(It.IsAny<LogEntryCriteria>()))
                .Callback((LogEntryCriteria criteria) => logEntryCriteria = criteria);
            
            // Act
            commandListener.ReplayEvents(new ReplayEventsCommand
            {
                EventType = "DomainEvent",
                RoutingKey = "Test.*"
            });
            
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
            var eventReplayerMock = new Mock<IEventReplayer>();
            var repositoryMock = new Mock<IAuditLogRepository<LogEntry, long>>();
            var repository = repositoryMock.Object;
            var eventReplayer = eventReplayerMock.Object;
            var routingKeyMatcherMock = new Mock<IRoutingKeyMatcher>();
            var routingKeyMatcher = routingKeyMatcherMock.Object;
            var commandListener = new AuditLogCommandListener(repository, eventReplayer, routingKeyMatcher);
            
            // Act
            commandListener.ReplayEvents(new ReplayEventsCommand
            {
                EventType = "DomainEvent",
                RoutingKey = "Test.*"
            });
            
            // Assert
            eventReplayerMock.Verify(mock => mock.ReplayLogEntries(It.IsAny<IEnumerable<LogEntry>>()));
        }

        [TestMethod]
        public void ReplayEventsReturnsReplayEventResponse200()
        {
            // Arrange
            var eventReplayerMock = new Mock<IEventReplayer>();
            var repositoryMock = new Mock<IAuditLogRepository<LogEntry, long>>();
            var repository = repositoryMock.Object;
            var eventReplayer = eventReplayerMock.Object;
            var routingKeyMatcherMock = new Mock<IRoutingKeyMatcher>();
            var routingKeyMatcher = routingKeyMatcherMock.Object;
            var commandListener = new AuditLogCommandListener(repository, eventReplayer, routingKeyMatcher);
            
            // Act
            var result = commandListener.ReplayEvents(new ReplayEventsCommand
            {
                EventType = "DomainEvent",
                RoutingKey = "Test.*"
            });

            // Assert
            Assert.AreEqual(200, result.Code);
            Assert.AreEqual("OK", result.Status);
        }

        [TestMethod]
        public void ReplayEventsReturnsReplayEventResponse500()
        {
            // Arrange
            var eventReplayerMock = new Mock<IEventReplayer>();
            var repositoryMock = new Mock<IAuditLogRepository<LogEntry, long>>();
            var repository = repositoryMock.Object;
            var eventReplayer = eventReplayerMock.Object;
            var routingKeyMatcherMock = new Mock<RoutingKeyMatcher>();
            var routingKeyMatcher = routingKeyMatcherMock.Object;
            var commandListener = new AuditLogCommandListener(repository, eventReplayer, routingKeyMatcher);
            repositoryMock.Setup(mock => mock.FindBy(It.IsAny<LogEntryCriteria>()))
                .Throws(new Exception());
            // Act
            var result = commandListener.ReplayEvents(new ReplayEventsCommand
            {
                EventType = "DomainEvent",
                RoutingKey = "Test.*"
            });

            // Assert
            Assert.AreEqual(500, result.Code);
            Assert.AreEqual("Internal Error", result.Status);
        }

        [TestMethod]
        public void ReplayEventsCallsIsMatchOnRoutingKeyMatcher()
        {
            // Arrange
            var eventReplayerMock = new Mock<IEventReplayer>();
            var repositoryMock = new Mock<IAuditLogRepository<LogEntry, long>>();
            var repository = repositoryMock.Object;
            var eventReplayer = eventReplayerMock.Object;
            var routingKeyMatcherMock = new Mock<IRoutingKeyMatcher>();
            var routingKeyMatcher = routingKeyMatcherMock.Object;
            var commandListener = new AuditLogCommandListener(repository, eventReplayer, routingKeyMatcher);
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
            commandListener.ReplayEvents(new ReplayEventsCommand
            {
                EventType = "DomainEvent",
                RoutingKey = "Test.*"
            });
            
            // Assert
            routingKeyMatcherMock.Verify(mock => mock.IsMatch("Test.*", It.IsAny<string>()));
        }
    }
}