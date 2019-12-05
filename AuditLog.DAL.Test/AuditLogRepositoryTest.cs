using System;
using System.Collections.Generic;
using System.Linq;
using AuditLog.Domain;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AuditLog.DAL.Test
{
    [TestClass]
    public class AuditLogRepositoryTest
    {
        private SqliteConnection _connection;
        private DbContextOptions<AuditLogContext> _options;

        [TestInitialize]
        public void TestInitialize()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            _options = new DbContextOptionsBuilder<AuditLogContext>()
                .UseSqlite(_connection)
                .Options;

            using var context = new AuditLogContext(_options);
            context.Database.EnsureCreated();
            SeedData();
        }

        [TestCleanup]
        public void TestCleanUp()
        {
            _connection.Close();
        }

        [TestMethod]
        public void FindAllIsInstanceOfIEnumerableOfLogEntry()
        {
            // Arrange
            using var context = new AuditLogContext(_options);
            var repository = new AuditLogRepository(context);

            // Act
            var result = repository.FindAll();

            // Assert
            Assert.IsInstanceOfType(result, typeof(IEnumerable<LogEntry>));
        }

        [TestMethod]
        public void FindAllResultCountIs4()
        {
            // Arrange
            using var context = new AuditLogContext(_options);
            var repository = new AuditLogRepository(context);
                
            // Act
            var result = repository.FindAll();
                
            // Assert
            Assert.AreEqual(4, result.Count());
        }

        [TestMethod]
        public void FindByIsInstanceOfIEnumerableOfLogEntry()
        {
            // Arrange
            using var context = new AuditLogContext(_options);
            var repository = new AuditLogRepository(context);
            var criteria = new LogEntryCriteria
            {
                EventType = "DomainEvent",
                FromTimestamp = new DateTime(2019, 7, 1).Ticks,
                ToTimestamp = new DateTime(2019, 7, 3).Ticks,
            };

            // Act
            var result = repository.FindBy(criteria);

            // Assert
            Assert.IsInstanceOfType(result, typeof(IEnumerable<LogEntry>));
        }

        [TestMethod]
        public void FindByWithTimestampsOutOfReachShouldBeCountOf0()
        {
            // Arrange
            using var context = new AuditLogContext(_options);
            var repository = new AuditLogRepository(context);
            var criteria = new LogEntryCriteria
            {
                EventType = "DomainEvent",
                FromTimestamp = new DateTime(2019, 6, 7).Ticks,
                ToTimestamp = new DateTime(2019, 6, 10).Ticks,
            };

            // Act
            var result = repository.FindBy(criteria);
                
            // Assert
            Assert.AreEqual(0, result.Count());
        }
        
        [TestMethod]
        public void FindByWithTimestampsEqualToFromTimestampsShouldBeCountOf2()
        {
            // Arrange
            using var context = new AuditLogContext(_options);
            var repository = new AuditLogRepository(context);
            var criteria = new LogEntryCriteria
            {
                EventType = "DomainEvent",
                FromTimestamp = new DateTime(2019, 7, 2).Ticks,
                ToTimestamp = new DateTime(2019, 7, 10).Ticks,
            };

            // Act
            var result = repository.FindBy(criteria);
                
            // Assert
            Assert.AreEqual(2, result.Count());
        }
        
        [TestMethod]
        public void FindByWithTimestampsEqualToFromAndToTimestampsShouldBeCountOf3()
        {
            // Arrange
            using var context = new AuditLogContext(_options);
            var repository = new AuditLogRepository(context);
            var criteria = new LogEntryCriteria
            {
                EventType = "DomainEvent",
                FromTimestamp = new DateTime(2019, 7, 2).Ticks,
                ToTimestamp = new DateTime(2019, 8, 15).Ticks,
            };

            // Act
            var result = repository.FindBy(criteria);
                
            // Assert
            Assert.AreEqual(3, result.Count());
        }
        
        [TestMethod]
        public void FindByWithNoDomainEventShouldBeCountOf4()
        {
            // Arrange
            using var context = new AuditLogContext(_options);
            var repository = new AuditLogRepository(context);
            var criteria = new LogEntryCriteria
            {
                FromTimestamp = new DateTime(2019, 2, 2).Ticks,
                ToTimestamp = new DateTime(2019, 9, 10).Ticks,
            };

            // Act
            var result = repository.FindBy(criteria);
                
            // Assert
            Assert.AreEqual(4, result.Count());
        }

        [TestMethod]
        public void CreateOneLogEntryResultsInCountOf5()
        {
            // Arrange
            using (var context = new AuditLogContext(_options))
            {
                var repository = new AuditLogRepository(context);
                var logEntry = new LogEntry
                {
                    Timestamp = new DateTime(2019,7,2).Ticks,
                    EventJson = "{'title': 'Something'}",
                    EventType = "DomainEvent",
                    RoutingKey = "Test.TestQueue.TestCreated"
                };

                // Act
                repository.Create(logEntry);
            }
            
            // Assert
            using (var context = new AuditLogContext(_options))
            {
                var result = context.LogEntries.Count();
                Assert.AreEqual(5, result);
            }
        }

        private void SeedData()
        {
            using var context = new AuditLogContext(_options);
            var logEntries = new List<LogEntry>
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
                },
                new LogEntry
                {
                    Timestamp = new DateTime(2019, 7, 2).Ticks,
                    RoutingKey = "Test.#",
                    EventJson = "{'title': 'Some title'}",
                    EventType = "DomainEvent"
                },
                new LogEntry
                {
                    Timestamp = new DateTime(2019, 8, 15).Ticks,
                    RoutingKey = "Test2.#",
                    EventJson = "{'title': 'Some title'}",
                    EventType = "DomainEvent"
                }
            };
                
            context.LogEntries.AddRange(logEntries);
            
            context.SaveChanges();
        }
    }
}