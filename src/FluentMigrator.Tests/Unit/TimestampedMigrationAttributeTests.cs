using System;

using Xunit;

namespace FluentMigrator.Tests.Unit
{
    public class TimestampedMigrationAttributeTests
    {
        private const int DayOfMonth = 15;

        private const string Description = "Description";

        private const int Hour = 12;

        private const string InvalidDateExceptionMessage = "Year, Month, and Day parameters describe an un-representable DateTime.";

        private const string InvalidTimeExceptionMessage = "Hour, Minute, and Second parameters describe an un-representable DateTime.";

        private const int Minute = 30;

        private const int Month = 06;

        private const int Second = 30;

        private const int Year = 2000;

        [Fact]
        public void CanCreateOneAccurateToTheMinute()
        {
            // Arrange
            Action action = () => new TimestampedMigrationAttribute(Year, Month, DayOfMonth, Hour, Minute);

            // Act and Assert
            Assert.That((TestDelegate)action.Invoke, Throws.Nothing);
        }

        [Fact]
        public void CanCreateOneAccurateToTheMinuteWithDescription()
        {
            // Arrange
            Action action = () => new TimestampedMigrationAttribute(Year, Month, DayOfMonth, Hour, Minute, Description);

            // Act and Assert
            Assert.That((TestDelegate)action.Invoke, Throws.Nothing);
        }

        [Fact]
        public void CanCreateOneAccurateToTheMinuteWithTransactionBehavior()
        {
            // Arrange
            Action action = () => new TimestampedMigrationAttribute(Year, Month, DayOfMonth, Hour, Minute, TransactionBehavior.Default);

            // Act and Assert
            var code = (TestDelegate)action.Invoke;
            Assert.That(code, Throws.Nothing);
        }

        [Fact]
        public void CanCreateOneAccurateToTheMinuteWithTransactionBehaviorAndDescription()
        {
            // Arrange
            Action action = () => new TimestampedMigrationAttribute(Year, Month, DayOfMonth, Hour, Minute, TransactionBehavior.Default, Description);

            // Act and Assert
            var code = (TestDelegate)action.Invoke;
            Assert.That(code, Throws.Nothing);
        }

        [Fact]
        public void CanCreateOneAccurateToTheSecond()
        {
            // Arrange
            Action action = () => new TimestampedMigrationAttribute(Year, Month, DayOfMonth, Hour, Minute, Second);

            // Act and Assert
            var code = (TestDelegate)action.Invoke;
            Assert.That(code, Throws.Nothing);
        }

        [Fact]
        public void CanCreateOneAccurateToTheSecondWithDescription()
        {
            // Arrange
            Action action = () => new TimestampedMigrationAttribute(Year, Month, DayOfMonth, Hour, Minute, Second, Description);

            // Act and Assert
            var code = (TestDelegate)action.Invoke;
            Assert.That(code, Throws.Nothing);
        }

        [Fact]
        public void CanCreateOneAccurateToTheSecondWithTransactionBehavior()
        {
            // Arrange
            Action action = () => new TimestampedMigrationAttribute(Year, Month, DayOfMonth, Hour, Minute, Second, TransactionBehavior.Default);

            // Act and Assert
            var code = (TestDelegate)action.Invoke;
            Assert.That(code, Throws.Nothing);
        }

        [Fact]
        public void CanCreateOneAccurateToTheSecondWithTransactionBehaviorAndDescription()
        {
            // Arrange
            Action action = () => new TimestampedMigrationAttribute(Year, Month, DayOfMonth, Hour, Minute, Second, TransactionBehavior.Default, Description);

            // Act and Assert
            var code = (TestDelegate)action.Invoke;
            Assert.That(code, Throws.Nothing);
        }

        [Fact]
        public void CreatingOneSetsUnderlyingValues()
        {
            // Arrange and Act
            var attribute = new TimestampedMigrationAttribute(Year, Month, DayOfMonth, Hour, Minute, Second, TransactionBehavior.None, Description);

            // Assert
            Assert.That(attribute.Description, Is.EqualTo(Description));
            Assert.That(attribute.TransactionBehavior, Is.EqualTo(TransactionBehavior.None));
            Assert.That(attribute.Version, Is.EqualTo(20000615123030));
        }

        [Fact]
        public void ExtendsMigrationAttribute()
        {
            // Arrange and Act
            var timestampedMigrationAttribute = new TimestampedMigrationAttribute(Year, Month, DayOfMonth, Hour, Minute);

            // Assert
            Assert.That(timestampedMigrationAttribute, Is.InstanceOf<MigrationAttribute>());
        }

        [Fact]
        public void TryingToCreateWithInvalidDayOfMonthResultsInArgumentOutOfRangeException()
        {
            // Arrange
            Action action = () => new TimestampedMigrationAttribute(Year, Month, 99, Hour, Minute);

            // Act and Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(action.Invoke);
            Assert.That(exception.Message, Is.EqualTo(InvalidDateExceptionMessage));
        }

        [Fact]
        public void TryingToCreateWithInvalidHourResultsInArgumentOutOfRangeException()
        {
            // Arrange
            Action action = () => new TimestampedMigrationAttribute(Year, Month, DayOfMonth, 99, Minute);

            // Act and Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(action.Invoke);
            Assert.That(exception.Message, Is.EqualTo(InvalidTimeExceptionMessage));
        }

        [Fact]
        public void TryingToCreateWithInvalidMinuteResultsInArgumentOutOfRangeException()
        {
            // Arrange
            Action action = () => new TimestampedMigrationAttribute(Year, Month, DayOfMonth, Hour, 99);

            // Act and Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(action.Invoke);
            Assert.That(exception.Message, Is.EqualTo(InvalidTimeExceptionMessage));
        }

        [Fact]
        public void TryingToCreateWithInvalidMonthResultsInArgumentOutOfRangeException()
        {
            // Arrange
            Action action = () => new TimestampedMigrationAttribute(Year, 99, DayOfMonth, Hour, Minute);

            // Act and Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(action.Invoke);
            Assert.That(exception.Message, Is.EqualTo(InvalidDateExceptionMessage));
        }

        [Fact]
        public void TryingToCreateWithInvalidSecondResultsInArgumentOutOfRangeException()
        {
            // Arrange
            Action action = () => new TimestampedMigrationAttribute(Year, Month, DayOfMonth, Hour, Minute, 99);

            // Act and Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(action.Invoke);
            Assert.That(exception.Message, Is.EqualTo(InvalidTimeExceptionMessage));
        }

        [Fact]
        public void TryingToCreateWithInvalidYearResultsInArgumentOutOfRangeException()
        {
            // Arrange
            Action action = () => new TimestampedMigrationAttribute(0000, Month, DayOfMonth, Hour, Minute);

            // Act and Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(action.Invoke);
            Assert.That(exception.Message, Is.EqualTo(InvalidDateExceptionMessage));
        }
    }
}
