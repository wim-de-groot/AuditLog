using System;
using AuditLog.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RabbitMQ.Client;

namespace AuditLog.Test
{
    [TestClass]
    public class EventBusBuilderTest
    {
        [TestInitialize]
        public void TestInitialize()
        {
            Environment.SetEnvironmentVariable("HOSTNAME", "TestHostname");
            Environment.SetEnvironmentVariable("PORT", "5000");
            Environment.SetEnvironmentVariable("USERNAME", "TestUserName");
            Environment.SetEnvironmentVariable("PASSWORD", "TestPassword");
            Environment.SetEnvironmentVariable("EXCHANGE_NAME", "TestExchangeName");
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Environment.SetEnvironmentVariable("HOSTNAME", null);
            Environment.SetEnvironmentVariable("PORT", null);
            Environment.SetEnvironmentVariable("USERNAME", null);
            Environment.SetEnvironmentVariable("PASSWORD", null);
            Environment.SetEnvironmentVariable("EXCHANGE_NAME", null);
        }

        [TestMethod]
        public void FromEnvironmentShouldReturnInstanceOfTypeIEventBusBuilder()
        {
            // Arrange
            IEventBusBuilder builder = new EventBusBuilder();
            
            // Act
            var result = builder.FromEnvironment();
            
            // Assert
            Assert.IsInstanceOfType(result, typeof(IEventBusBuilder));
        }

        [TestMethod]
        public void FromEnvironmentShouldExtractHostName()
        {
            // Arrange
            IEventBusBuilder builder = new EventBusBuilder();
            
            // Act
            var result = builder.FromEnvironment();
            
            // Assert
            Assert.AreEqual("TestHostname", result.HostName);
        }
        
        [TestMethod]
        public void FromEnvironmentShouldExtractPort()
        {
            // Arrange
            IEventBusBuilder builder = new EventBusBuilder();
            
            // Act
            var result = builder.FromEnvironment();
            
            // Assert
            Assert.AreEqual(5000, result.Port);
        }
        
        [TestMethod]
        public void FromEnvironmentShouldExtractUserName()
        {
            // Arrange
            IEventBusBuilder builder = new EventBusBuilder();
            
            // Act
            var result = builder.FromEnvironment();
            
            // Assert
            Assert.AreEqual("TestUserName", result.UserName);
        }
        
        [TestMethod]
        public void FromEnvironmentShouldExtractPassword()
        {
            // Arrange
            IEventBusBuilder builder = new EventBusBuilder();
            
            // Act
            var result = builder.FromEnvironment();
            
            // Assert
            Assert.AreEqual("TestPassword", result.Password);
        }
        
        [TestMethod]
        public void FromEnvironmentShouldExtractExchangeName()
        {
            // Arrange
            IEventBusBuilder builder = new EventBusBuilder();
            
            // Act
            var result = builder.FromEnvironment();
            
            // Assert
            Assert.AreEqual("TestExchangeName", result.ExchangeName);
        }
        
        [TestMethod]
        public void FromEnvironmentShouldThrowExceptionIfHostNameEnvironmentVariableIsNull()
        {
            // Arrange
            Environment.SetEnvironmentVariable("HOSTNAME", null);
            IEventBusBuilder builder = new EventBusBuilder();
            
            // Act
            var result = Assert.ThrowsException<InvalidEnvironmentException>(() => builder.FromEnvironment());
            
            // Assert
            Assert.AreEqual("Environment variable [HOSTNAME] can not be null", result.Message);
        }
        
        [TestMethod]
        public void FromEnvironmentShouldThrowExceptionIfPortEnvironmentVariableIsNull()
        {
            // Arrange
            Environment.SetEnvironmentVariable("PORT", null);
            IEventBusBuilder builder = new EventBusBuilder();
            
            // Act
            var result = Assert.ThrowsException<InvalidEnvironmentException>(() => builder.FromEnvironment());
            
            // Assert
            Assert.AreEqual("Environment variable [PORT] can not be null", result.Message);
        }
        
        [TestMethod]
        public void FromEnvironmentShouldThrowExceptionIfUserNameEnvironmentVariableIsNull()
        {
            // Arrange
            Environment.SetEnvironmentVariable("USERNAME", null);
            IEventBusBuilder builder = new EventBusBuilder();
            
            // Act
            var result = Assert.ThrowsException<InvalidEnvironmentException>(() => builder.FromEnvironment());
            
            // Assert
            Assert.AreEqual("Environment variable [USERNAME] can not be null", result.Message);
        }
        
        [TestMethod]
        public void FromEnvironmentShouldThrowExceptionIfPasswordEnvironmentVariableIsNull()
        {
            // Arrange
            Environment.SetEnvironmentVariable("PASSWORD", null);
            IEventBusBuilder builder = new EventBusBuilder();
            
            // Act
            var result = Assert.ThrowsException<InvalidEnvironmentException>(() => builder.FromEnvironment());
            
            // Assert
            Assert.AreEqual("Environment variable [PASSWORD] can not be null", result.Message);
        }
        
        [TestMethod]
        public void FromEnvironmentShouldThrowExceptionIfExchangeNameEnvironmentVariableIsNull()
        {
            // Arrange
            Environment.SetEnvironmentVariable("EXCHANGE_NAME", null);
            IEventBusBuilder builder = new EventBusBuilder();
            
            // Act
            var result = Assert.ThrowsException<InvalidEnvironmentException>(() => builder.FromEnvironment());
            
            // Assert
            Assert.AreEqual("Environment variable [EXCHANGE_NAME] can not be null", result.Message);
        }

        [TestMethod]
        public void CreateEventBusShouldCreateEventBusWithConnection()
        {
            // Arrange
            var modelMock = new Mock<IModel>();
            var model = modelMock.Object;
            var connectionMock = new Mock<IConnection>();
            var connection = connectionMock.Object;
            var factoryMock = new Mock<IConnectionFactory>();
            var factory = factoryMock.Object;
            connectionMock.Setup(mock => mock.CreateModel()).Returns(model);
            factoryMock.Setup(mock => mock.CreateConnection()).Returns(connection);
            IEventBusBuilder builder = new EventBusBuilder();

            // Act
            var result = builder.FromEnvironment().CreateEventBus(factory);
            
            // Assert
            Assert.AreEqual(connection, result.Connection);
        }
        
        [TestMethod]
        public void CreateEventBusShouldCreateEventBusWithExchangeName()
        {
            // Arrange
            var modelMock = new Mock<IModel>();
            var model = modelMock.Object;
            var connectionMock = new Mock<IConnection>();
            var connection = connectionMock.Object;
            var factoryMock = new Mock<IConnectionFactory>();
            var factory = factoryMock.Object;
            connectionMock.Setup(mock => mock.CreateModel()).Returns(model);
            factoryMock.Setup(mock => mock.CreateConnection()).Returns(connection);
            IEventBusBuilder builder = new EventBusBuilder();

            // Act
            var result = builder.FromEnvironment().CreateEventBus(factory);
            
            // Assert
            Assert.AreEqual("TestExchangeName", result.ExchangeName);
        }
        
        [TestMethod]
        public void CreateEventBusShouldDeclareExchange()
        {
            // Arrange
            var modelMock = new Mock<IModel>();
            var model = modelMock.Object;
            var connectionMock = new Mock<IConnection>();
            var connection = connectionMock.Object;
            var factoryMock = new Mock<IConnectionFactory>();
            var factory = factoryMock.Object;
            connectionMock.Setup(mock => mock.CreateModel()).Returns(model);
            factoryMock.Setup(mock => mock.CreateConnection()).Returns(connection);
            IEventBusBuilder builder = new EventBusBuilder();

            // Act
            builder.FromEnvironment().CreateEventBus(factory);
            
            // Assert
            connectionMock.Verify(mock => mock.CreateModel());
            modelMock.Verify(mock => mock.ExchangeDeclare("TestExchangeName", ExchangeType.Topic, false, false, null));
        }
    }
}