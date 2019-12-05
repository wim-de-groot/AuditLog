using System.Linq;
using System.Reflection;
using AuditLog.Abstractions;
using AuditLog.Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Miffy.MicroServices.Events;
using Moq;

namespace AuditLog.Test
{
    [TestClass]
    public class AuditLogEventListenerTest
    {
        [TestMethod]
        public void HasHandleMethod()
        {
            // Arrange
            var type = typeof(AuditLogEventListener);
            
            // Act
            var result = type.GetMethod("Handle");

            // Assert
            Assert.IsNotNull(result);
        }
        
        [TestMethod]
        public void HasEventListenerAttribute()
        {
            // Arrange
            var attributes = typeof(AuditLogEventListener)
                .GetMethod("Handle")
                ?.GetCustomAttributes(typeof(EventListenerAttribute));

            // Act
            var result = attributes?.FirstOrDefault();
            
            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void HandleMethodHasTopicAttribute()
        {
            // Arrange
            var attributes = typeof(AuditLogEventListener)
                .GetMethod("Handle")
                ?.GetCustomAttributes(typeof(TopicAttribute));

            // Act
            var result = attributes?.FirstOrDefault();
            
            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void TopicAttributeListensToAllTopics()
        {
            // Arrange
            var attributes = typeof(AuditLogEventListener)
                .GetMethod("Handle")
                ?.GetCustomAttributes(typeof(TopicAttribute));

            // Act
            var result = attributes?.FirstOrDefault() as TopicAttribute;
            
            // Assert
            Assert.AreEqual("#",result?.TopicPattern);
        }

        [TestMethod]
        public void HandleHasOneParameter()
        {
            // Arrange
            var method = typeof(AuditLogEventListener)
                .GetMethod("Handle");

            var result = method?.GetParameters();

            Assert.AreEqual(1,result?.Length);
        }

        [TestMethod]
        public void EventListenerAttributeHasQueueName()
        {
            // Arrange
            var attributes = typeof(AuditLogEventListener)
                    .GetMethod("Handle")
                    ?.GetCustomAttributes(typeof(EventListenerAttribute));

            // Act
            var result = attributes?.FirstOrDefault() as EventListenerAttribute;
            
            // Assert
            Assert.AreEqual("AuditLog", result?.QueueName);
        }

        [TestMethod]
        public void HandleCallsCreateOnRepository()
        {
            // Arrange
            const string message = "{'timestamp': 800000, 'routingKey': 'Miffy.*', 'eventType': 'DomainEvent', 'eventJson': ''}";
            
            var repositoryMock = new Mock<IAuditLogRepository<LogEntry, long>>();
            var repository = repositoryMock.Object;
            var eventListener = new AuditLogEventListener(repository);
            
            // Act
            eventListener.Handle(message);
            
            // Assert
            repositoryMock.Verify(mock => mock.Create(It.IsAny<LogEntry>()));
        }
    }
}