using FluentMigrator.Expressions;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Exceptions;

using Moq;
using NUnit.Framework;

namespace FluentMigrator.Tests
{
    [TestFixture]
    public class MigrationValidatorTests
    {
        [SetUp]
        public void Setup()
        {
            _migration = Mock.Of<IMigration>();
            _migrationValidator = new MigrationValidator(Mock.Of<IAnnouncer>(), new DefaultConventionSet());
        }

        private MigrationValidator _migrationValidator;
        private IMigration _migration;

        private IMigrationExpression BuildInvalidExpression()
        {
            return new CreateTableExpression();
        }

        private IMigrationExpression BuildValidExpression()
        {
            var expression = new CreateTableExpression();
            expression.TableName = "Foo";
            return expression;
        }

        [Test]
        public void ItDoesNotThrowIfExpressionsAreValid()
        {
            Assert.DoesNotThrow(
                () => _migrationValidator.ApplyConventionsToAndValidateExpressions(_migration
                                                                                  , new[] {BuildValidExpression()}));
        }

        [Test]
        public void ItThrowsInvalidMigrationExceptionIfExpressionsAreInvalid()
        {
            Assert.Throws<InvalidMigrationException>(
                () => _migrationValidator.ApplyConventionsToAndValidateExpressions(_migration
                                                                                  , new[] {BuildInvalidExpression()}));
        }

        [Test]
        public void ItThrowsInvalidMigrationExceptionIfExpressionsContainsMultipleInvalidOfSameType()
        {
            Assert.Throws<InvalidMigrationException>(
                () => _migrationValidator.ApplyConventionsToAndValidateExpressions(_migration
                                                                                  , new[] {BuildInvalidExpression(), BuildInvalidExpression()}));
        }
    }
}
