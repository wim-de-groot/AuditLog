using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AuditLog.Abstractions;
using AuditLog.Domain;
using AuditLog.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace AuditLog.WebApi.Test
{
    [TestClass]
    public class LogEntryControllerTest
    {
        [TestMethod]
        public void GetAllShouldBeTypeOfActionResult()
        {
            // Arrange
            var repositoryMock = new Mock<IAuditLogRepository<LogEntry, long>>();
            var repository = repositoryMock.Object;
            var controller = new LogEntryController(repository);

            // Act
            var result = controller.GetAll();

            // Assert
            Assert.IsInstanceOfType(result, typeof(ActionResult<IEnumerable<LogEntry>>));
        }

        [TestMethod]
        public void GetAllShouldHaveAEnumerableTypeOfLogEntries()
        {
            // Arrange
            var repositoryMock = new Mock<IAuditLogRepository<LogEntry, long>>();
            var repository = repositoryMock.Object;
            var controller = new LogEntryController(repository);

            // Act
            var result = controller.GetAll().Value;

            // Assert
            Assert.IsInstanceOfType(result, typeof(IEnumerable<LogEntry>));
        }

        [TestMethod]
        public void GetAllShouldCallFindAllOnRepository()
        {
            // Arrange
            var repositoryMock = new Mock<IAuditLogRepository<LogEntry, long>>();
            var repository = repositoryMock.Object;
            var controller = new LogEntryController(repository);

            // Act
            var result = controller.GetAll().Value;

            // Assert
            repositoryMock.Verify(mock => mock.FindAll());
        }

        [TestMethod]
        public void GetAllShouldReturnLogEntriesFromDatabase()
        {
            // Arrange
            var repositoryMock = new Mock<IAuditLogRepository<LogEntry, long>>();
            var repository = repositoryMock.Object;
            var controller = new LogEntryController(repository);
            repositoryMock.Setup(mock => mock.FindAll())
                .Returns(new List<LogEntry>
                {
                    new LogEntry
                    {
                        Id = 1,
                        EventType = "PolisCreatedEvent",
                        RoutingKey = "Polis.*",
                        Timestamp = new DateTime(2019, 4, 2).Ticks,
                        EventJson = "{'title': 'Something',}",
                    }
                });

            // Act
            var result = controller.GetAll().Value;

            // Assert
            Assert.IsTrue(result.Any(entry => entry.EventType.Equals("PolisCreatedEvent")));
        }

        [TestMethod]
        public void ShouldHaveApiControllerAttribute()
        {
            // Arrange
            var repositoryMock = new Mock<IAuditLogRepository<LogEntry, long>>();
            var repository = repositoryMock.Object;
            var controller = new LogEntryController(repository);

            // Act
            var result = controller.GetType().GetCustomAttribute<ApiControllerAttribute>();
            
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void ShouldHaveRouteAttribute()
        {
            // Arrange
            var repositoryMock = new Mock<IAuditLogRepository<LogEntry, long>>();
            var repository = repositoryMock.Object;
            var controller = new LogEntryController(repository);

            // Act
            var result = controller.GetType().GetCustomAttribute<RouteAttribute>();
            
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void RouteAttributeShouldHaveTemplateOfLogEntries()
        {
            // Arrange
            var repositoryMock = new Mock<IAuditLogRepository<LogEntry, long>>();
            var repository = repositoryMock.Object;
            var controller = new LogEntryController(repository);

            // Act
            var result = controller.GetType().GetCustomAttribute<RouteAttribute>().Template;
            
            // Assert
            Assert.AreEqual("logEntries", result);
        }

        [TestMethod]
        public void GetAllShouldHaveAHttpGetAttribute()
        {
            // Arrange
            var repositoryMock = new Mock<IAuditLogRepository<LogEntry, long>>();
            var repository = repositoryMock.Object;
            var controller = new LogEntryController(repository);

            // Act
            var result = controller.GetType().GetMethod("GetAll")?.GetCustomAttribute<HttpGetAttribute>();
            
            // Assert
            Assert.IsNotNull(result);
        }
    }
}