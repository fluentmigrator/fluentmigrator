using FluentMigrator.Exceptions;
using FluentMigrator.Expressions;
using FluentMigrator.Runner.Generators.SqlServer;
using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.Generators.SqlServerCe
{
    [TestFixture]
    public class SqlServerCeSchemaTests
    {
        protected SqlServerCeGenerator Generator;

        [SetUp]
        public void Setup()
        {
            Generator = new SqlServerCeGenerator();
        }

        [Test]
        public void CannotAlterASchema()
        {
            Assert.Throws<DatabaseOperationNotSupportedException>(() => Generator.Generate(new AlterSchemaExpression()));
        }

        [Test]
        public void CannotCreateASchema()
        {
            Assert.Throws<DatabaseOperationNotSupportedException>(() => Generator.Generate(new CreateSchemaExpression()));
        }

        [Test]
        public void CannotDeleteASchema()
        {
            Assert.Throws<DatabaseOperationNotSupportedException>(() => Generator.Generate(new DeleteSchemaExpression()));
        }
    }
}
