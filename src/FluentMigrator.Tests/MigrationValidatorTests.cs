using FluentMigrator.Expressions;
using FluentMigrator.Runner;
using Moq;
using Xunit;

namespace FluentMigrator.Tests
{
    public class MigrationValidatorTests
    {
        public MigrationValidatorTests()
        {
            migration = Mock.Of<IMigration>();
            migrationValidator = new MigrationValidator(Mock.Of<IAnnouncer>(), new MigrationConventions());
        }

        private MigrationValidator migrationValidator;
        private IMigration migration;

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

        [Fact]
        public void it_does_not_throw_if_expressions_are_valid()
        {
            Assert.DoesNotThrow(
                () => migrationValidator.ApplyConventionsToAndValidateExpressions(migration
                                                                                  , new[] {BuildValidExpression()}));
        }

        [Fact]
        public void it_throws_invalid_migration_exception_if_expressions_are_invalid()
        {
            Assert.Throws<InvalidMigrationException>(
                () => migrationValidator.ApplyConventionsToAndValidateExpressions(migration
                                                                                  , new[] {BuildInvalidExpression()}));
        }

        [Fact]
        public void it_throws_invalid_migration_exception_if_expressions_contains_multiple_invalid_of_same_type()
        {
            Assert.Throws<InvalidMigrationException>(
                () => migrationValidator.ApplyConventionsToAndValidateExpressions(migration
                                                                                  , new[] {BuildInvalidExpression(), BuildInvalidExpression()}));
        }
    }
}