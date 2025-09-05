using System.Data;

using FluentMigrator.Exceptions;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators.Snowflake;
using FluentMigrator.Runner.Processors.Snowflake;
using FluentMigrator.Snowflake;

using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.Generators.Snowflake
{
    [TestFixture(true)]
    [TestFixture(false)]
    [Category("Generator")]
    [Category("Snowflake")]
    public class SnowflakeGeneratorTests
    {
        protected SnowflakeGenerator Generator;
        private readonly bool _quotingEnabled;

        public SnowflakeGeneratorTests(bool quotingEnabled)
        {
            _quotingEnabled = quotingEnabled;
        }

        [SetUp]
        public void Setup()
        {
            var sfOptions = _quotingEnabled ? SnowflakeOptions.QuotingEnabled() : SnowflakeOptions.QuotingDisabled();
            Generator = new SnowflakeGenerator(sfOptions);
        }

        [Test]
        public void CanAlterColumnWithDefaultValue()
        {
            //TODO: This will fail if there are any keys attached
            var expression = GeneratorTestHelper.GetAlterColumnExpression();
            expression.Column.DefaultValue = "Foo";
            expression.SchemaName = "TestSchema";

            Assert.Throws<DatabaseOperationNotSupportedException>(() => Generator.Generate(expression));
        }

        [Test]
        public void CanAlterDefaultConstraint()
        {
            var expression = GeneratorTestHelper.GetAlterDefaultConstraintExpression();
            expression.SchemaName = "TestSchema";
            Assert.Throws<DatabaseOperationNotSupportedException>(() => Generator.Generate(expression));
        }

        [Test]
        public void CanAddColumnWithGetDateDefault()
        {
            var column = new ColumnDefinition
            {
                Name = "TestColumn1",
                Type = DbType.String,
                Size = 5,
                DefaultValue = SystemMethods.CurrentDateTime
            };
            var expression = new CreateColumnExpression { TableName = "TestTable1", Column = column, SchemaName = "TestSchema" };

            var result = Generator.Generate(expression);
            result.ShouldBe(@"ALTER TABLE ""TestSchema"".""TestTable1"" ADD COLUMN ""TestColumn1"" VARCHAR(5) NOT NULL DEFAULT SYSDATE()::TIMESTAMP_NTZ;", _quotingEnabled);
        }

        [Test]
        public void CanCreateTableWithGetDateDefault()
        {
            var column = new ColumnDefinition
            {
                Name = "TestColumn1",
                Type = DbType.String,
                Size = 5,
                DefaultValue = SystemMethods.CurrentDateTime
            };
            var expression = new CreateTableExpression { TableName = "TestTable1", SchemaName = "TestSchema" };
            expression.Columns.Add(column);

            var result = Generator.Generate(expression);
            result.ShouldBe(@"CREATE TABLE ""TestSchema"".""TestTable1"" (""TestColumn1"" VARCHAR(5) NOT NULL DEFAULT SYSDATE()::TIMESTAMP_NTZ);", _quotingEnabled);
        }

        [Test]
        public void CanCreateTableWithSeededIdentity()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithAutoIncrementExpression();
            expression.SchemaName = "TestSchema";
            expression.Columns[0].AdditionalFeatures.Add(SnowflakeExtensions.IdentitySeed, 45);
            expression.Columns[0].AdditionalFeatures.Add(SnowflakeExtensions.IdentityIncrement, 23);

            var result = Generator.Generate(expression);
            result.ShouldBe(@"CREATE TABLE ""TestSchema"".""TestTable1"" (""TestColumn1"" NUMBER NOT NULL IDENTITY(45,23), ""TestColumn2"" NUMBER NOT NULL);", _quotingEnabled);
        }

        [Test]
        public void CanGenerateNecessaryStatementsForADeleteDefaultExpression()
        {
            var expression = new DeleteDefaultConstraintExpression {ColumnName = "Name", SchemaName = "Personalia", TableName = "Person" };

            var result = Generator.Generate(expression);
            result.ShouldBe(@"ALTER TABLE ""Personalia"".""Person"" ALTER COLUMN ""Name"" DROP DEFAULT;", _quotingEnabled);
        }
    }
}
