using System;

using FluentMigrator.Helpers;

using NUnit.Framework;

namespace FluentMigrator.Tests.Helpers
{
    [TestFixture]
    public class IncrementorTests
    {
        [Test]
        public void CanCreateAnIncrementor()
        {
            // Arrange
            Action action = () => new Incrementor(1, 1);

            // Act and Assert
            Assert.DoesNotThrow(action.Invoke);
        }

        [TestCase(1, 1)]
        [TestCase(2, 2)]
        [TestCase(10, 20)]
        public void InitialValuesShouldBeAsExpected(int firstValue, int increment)
        {
            // Arrange
            var incrementor = new Incrementor(firstValue, increment);

            // Act
            var currentValue = incrementor.NextValue;
            var previousValue = incrementor.PreviousValue;

            // Assert
            Assert.AreEqual(currentValue, firstValue);
            Assert.AreEqual(previousValue, firstValue - increment);
        }

        [TestCase(1, 1)]
        [TestCase(2, 2)]
        [TestCase(10, 20)]
        public void AfterIncrementingValuesShouldBeAsExpected(int firstValue, int increment)
        {
            // Arrange
            var incrementor = new Incrementor(firstValue, increment);

            // Act
            var incremented = incrementor.Increment();
            var currentValue = incrementor.NextValue;
            var previousValue = incrementor.PreviousValue;

            // Assert
            Assert.AreEqual(incremented, firstValue);
            Assert.AreEqual(currentValue, firstValue + increment);
            Assert.AreEqual(previousValue, firstValue);
        }
    }
}
