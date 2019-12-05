using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AuditLog.Test
{
    [TestClass]
    public class RoutingKeyMatcherTest
    {
        [TestMethod]
        public void IsMatchIsTypeOfBool()
        {
            // Arrange
            var routingKeyMatcher = new RoutingKeyMatcher();
            
            // Act
            var result = routingKeyMatcher.IsMatch("Test.#","Test.TestQueue.TestCreated");

            // Assert
            Assert.IsInstanceOfType(result, typeof(bool));
        }

        [TestMethod]
        public void IsMatchWithCriteriaRoutingKeyNullShouldBeTrue()
        {
            // Arrange
            var routingKeyMatcher = new RoutingKeyMatcher();
            
            // Act
            var result = routingKeyMatcher.IsMatch(null,"Test.TestQueue.TestCreated");

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsMatchWithCriteriaContainingHashTagThatMatchesEntryRoutingKeyShouldBeTrue()
        {
            // Arrange
            var routingKeyMatcher = new RoutingKeyMatcher();
            
            // Act
            var result = routingKeyMatcher.IsMatch("Test.#","Test.TestQueue.TestCreated");

            // Assert
            Assert.IsTrue(result);
        }
        
        [TestMethod]
        public void IsMatchWithCriteriaThatDoesNotMatchEntryRoutingKeyShouldBeFalse()
        {
            // Arrange
            var routingKeyMatcher = new RoutingKeyMatcher();
            
            // Act
            var result = routingKeyMatcher.IsMatch("Test","Test.TestQueue.TestCreated");

            // Assert
            Assert.IsFalse(result);
        }
        
        [TestMethod]
        public void IsMatchWithCriteriaWithOnlyAHashTagShouldBeTrue()
        {
            // Arrange
            var routingKeyMatcher = new RoutingKeyMatcher();
            
            // Act
            var result = routingKeyMatcher.IsMatch("#","Test.TestQueue.TestCreated");

            // Assert
            Assert.IsTrue(result);
        }
    }
}