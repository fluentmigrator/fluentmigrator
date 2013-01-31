using FluentMigrator.Exceptions;
using FluentMigrator.Expressions;
using FluentMigrator.Runner.Generators.SqlServer;
using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.Generators.SqlServerCe
{
    [TestFixture]
    public class SqlServerCeGeneratorTests
    {
        [SetUp]
        public void Setup()
        {
            generator = new SqlServerCeGenerator();
        }

        private SqlServerCeGenerator generator;

        [Test]
        [ExpectedException(typeof(DatabaseOperationNotSupportedException))]
        public void GenerateNecessaryStatementsForADeleteDefaultExpressionIsThrowsException()
        {
            var expression = new DeleteDefaultConstraintExpression {ColumnName = "Name", SchemaName = "Personalia", TableName = "Person"};
            generator.Generate(expression);
        }
    }
}