using System;
using System.Linq;
using System.Text;
using System.Threading;
using AuditLog.Abstractions;
using AuditLog.DAL;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AuditLog.IntegrationTest
{
    [TestClass]
    public class EventBusIntegrationTest
    {
        private SqliteConnection _connection;
        private DbContextOptions<AuditLogContext> _options;

        [TestInitialize]
        public void TestInitialize()
        {
            AuditLogLoggerFactory.LoggerFactory = NullLoggerFactory.Instance;
            Environment.SetEnvironmentVariable("HOSTNAME", "localhost");
            Environment.SetEnvironmentVariable("PORT", "5672");
            Environment.SetEnvironmentVariable("USERNAME", "Guest");
            Environment.SetEnvironmentVariable("PASSWORD", "Guest");
            Environment.SetEnvironmentVariable("EXCHANGE_NAME", "TestExchange");
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            _options = new DbContextOptionsBuilder<AuditLogContext>()
                .UseSqlite(_connection)
                .Options;

            using var context = new AuditLogContext(_options);
            context.Database.EnsureCreated();
        }

        [TestCleanup]
        public void TestCleanUp()
        {
            _connection.Close();
        }

        [TestMethod]
        public void HandleShouldBeCalledWhenMessageIsSend()
        {
            // Arrange
            var eventListenerMock = new Mock<IEventListener>();
            using var eventBus = new EventBusBuilder()
                .FromEnvironment()
                .CreateEventBus()
                .AddEventListener(eventListenerMock.Object, "#");
            var awaitHandle = new ManualResetEvent(false);

            // Act
            PublishMessage(eventBus);
            awaitHandle.WaitOne(1000);

            // Assert
            eventListenerMock.Verify(mock => mock.Handle(It.IsAny<object>(), It.IsAny<BasicDeliverEventArgs>()));
        }

        [TestMethod]
        public void WhenPublishingEventShouldResultInLogEntriesCountOfOne()
        {
            // Arrange
            using (var context = new AuditLogContext(_options))
            {
                var repository = new AuditLogRepository(context);
                var eventListener = new AuditLogEventListener(repository);
                using var eventBus = new EventBusBuilder()
                    .FromEnvironment()
                    .CreateEventBus()
                    .AddEventListener(eventListener, "#");

                var awaitHandle = new ManualResetEvent(false);

                // Act
                PublishMessage(eventBus);
                awaitHandle.WaitOne(1000);
            }

            // Assert
            using (var context = new AuditLogContext(_options))
            {
                Assert.AreEqual(1, context.LogEntries.Count());
            }
        }

        private void PublishMessage(IEventBus eventBus)
        {
            var message = new EventMessage {Text = "Hello world"};
            var json = JsonConvert.SerializeObject(message);
            var body = Encoding.UTF8.GetBytes(json);
            var basicProperties = eventBus.Model.CreateBasicProperties();
            basicProperties.Type = "EventMessage";
            basicProperties.Timestamp = new AmqpTimestamp(new DateTime(2019, 5, 3).Ticks);
            eventBus.Model.BasicPublish("TestExchange", "Test.Test.#", false, basicProperties, body);
        }
    }

    internal class EventMessage
    {
        public string Text { get; set; }
    }
}