using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Miffy;
using Moq;
using RabbitMQ.Client;

namespace AuditLog.MiffyExtension.Test
{
    [TestClass]
    public class RabbitMqReplayContextBuilderTest
    {
        [TestMethod]
        public void WithReplayExchangePrefixShouldAddReplayExchangePrefix()
        {
            // Arrange
            var builder = new RabbitMqReplayContextBuilder();
            
            // Act
            builder.WithReplayExchangePrefix("TestExchange");
            
            // Assert
            Assert.AreEqual("TestExchange", builder.ReplayExchangePrefix);
        }

        [TestMethod]
        public void ReadFromEnvironmentVariablesShouldAddReplayExchangePrefix()
        {
            // Arrange
            Environment.SetEnvironmentVariable("REPLAY_EXCHANGE_NAME", "TestExchangeFromEnvironment");
            Environment.SetEnvironmentVariable("BROKER_CONNECTION_STRING", "amqp://guest:guest@localhost");
            Environment.SetEnvironmentVariable("BROKER_EXCHANGE_NAME", "exchange");
            
            // Act
            var builder = new RabbitMqReplayContextBuilder()
                .ReadFromEnvironmentVariables();
            
            // Assert
            Assert.AreEqual("TestExchangeFromEnvironment", ((RabbitMqReplayContextBuilder)builder).ReplayExchangePrefix);
        }

        [TestMethod]
        public void ReadFromEnvironmentVariablesWithNotSetEnvironmentShouldThrowException()
        {
            // Arrange
            Environment.SetEnvironmentVariable("REPLAY_EXCHANGE_NAME", null);
            Environment.SetEnvironmentVariable("BROKER_CONNECTION_STRING", "amqp://guest:guest@localhost");
            Environment.SetEnvironmentVariable("BROKER_EXCHANGE_NAME", "exchange");

            // Act
            var exception = Assert.ThrowsException<BusConfigurationException>(() => new RabbitMqReplayContextBuilder()
                .ReadFromEnvironmentVariables());
            
            // Assert
            Assert.AreEqual("REPLAY_EXCHANGE_NAME variable not set", exception.Message);
        }

        [TestMethod]
        public void CreateContextCallsCreateConnection()
        {
            // Arrange
            var connectionFactoryMock = new Mock<IConnectionFactory>();
            var connectionMock = new Mock<IConnection>();
            var modelMock = new Mock<IModel>();
            var connectionFactory = connectionFactoryMock.Object;
            var connection = connectionMock.Object;
            var model = modelMock.Object;
            connectionFactoryMock.Setup(mock => mock.CreateConnection()).Returns(connection);
            connectionMock.Setup(mock => mock.CreateModel()).Returns(model);

            // Act
            using (new RabbitMqReplayContextBuilder()
                    .WithReplayExchangePrefix("TestExchange")
                    .CreateContext(connectionFactory))
            
            // Assert
            connectionFactoryMock.Verify(mock => mock.CreateConnection());
        }

        [TestMethod]
        public void CreateContextCallsCreateModel()
        {
            // Arrange
            var connectionFactoryMock = new Mock<IConnectionFactory>();
            var connectionMock = new Mock<IConnection>();
            var modelMock = new Mock<IModel>();
            var connectionFactory = connectionFactoryMock.Object;
            var connection = connectionMock.Object;
            var model = modelMock.Object;
            connectionFactoryMock.Setup(mock => mock.CreateConnection()).Returns(connection);
            connectionMock.Setup(mock => mock.CreateModel()).Returns(model);

            // Act
            using (new RabbitMqReplayContextBuilder()
                    .WithReplayExchangePrefix("TestExchange")
                    .CreateContext(connectionFactory))
            
            // Assert
            connectionMock.Verify(mock => mock.CreateModel());
        }

        [TestMethod]
        public void CreateContextCallsExchangeDeclare()
        {
            // Arrange
            var connectionFactoryMock = new Mock<IConnectionFactory>();
            var connectionMock = new Mock<IConnection>();
            var modelMock = new Mock<IModel>();
            var connectionFactory = connectionFactoryMock.Object;
            var connection = connectionMock.Object;
            var model = modelMock.Object;
            connectionFactoryMock.Setup(mock => mock.CreateConnection()).Returns(connection);
            connectionMock.Setup(mock => mock.CreateModel()).Returns(model);

            // Act
            using (new RabbitMqReplayContextBuilder()
                .WithReplayExchangePrefix("TestExchange")
                .CreateContext(connectionFactory))
            
            // Assert
            modelMock.Verify(mock => mock.ExchangeDeclare("TestExchange", ExchangeType.Direct,false, false, null));
        }

        [TestMethod]
        public void CreateContextReturnsTypeOfRabbitMqBusReplayContext()
        {
            // Arrange
            var connectionFactoryMock = new Mock<IConnectionFactory>();
            var connectionMock = new Mock<IConnection>();
            var modelMock = new Mock<IModel>();
            var connectionFactory = connectionFactoryMock.Object;
            var connection = connectionMock.Object;
            var model = modelMock.Object;
            connectionFactoryMock.Setup(mock => mock.CreateConnection()).Returns(connection);
            connectionMock.Setup(mock => mock.CreateModel()).Returns(model);

            // Act
            using var result = new RabbitMqReplayContextBuilder()
                .WithReplayExchangePrefix("TestExchange")
                .CreateContext(connectionFactory);
            
            // Assert
            Assert.IsInstanceOfType(result, typeof(RabbitMqBusReplayContext));
        }

        [TestMethod]
        public void CreateContextReturnsRabbitMqBusReplayContextWithTheRightReplayExchangePrefix()
        {
            // Arrange
            var connectionFactoryMock = new Mock<IConnectionFactory>();
            var connectionMock = new Mock<IConnection>();
            var modelMock = new Mock<IModel>();
            var connectionFactory = connectionFactoryMock.Object;
            var connection = connectionMock.Object;
            var model = modelMock.Object;
            connectionFactoryMock.Setup(mock => mock.CreateConnection()).Returns(connection);
            connectionMock.Setup(mock => mock.CreateModel()).Returns(model);

            // Act
            using var result = new RabbitMqReplayContextBuilder()
                .WithReplayExchangePrefix("TestExchange")
                .CreateContext(connectionFactory) as RabbitMqBusReplayContext;
            
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("TestExchange", result.ReplayExchangePrefix);
        }
    }
}