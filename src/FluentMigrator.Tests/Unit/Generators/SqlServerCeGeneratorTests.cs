using System;
using System.Data;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators
{
    [TestFixture]
    public class SqlServerCeGeneratorTests
    {
        private const string tableName = "NewTable";
        SqlServerCeGenerator generator;

        [SetUp]
        public void SetUp()
        {
            generator = new SqlServerCeGenerator();
        }

        [Test]
        public void DoesNotImplementASchema()
        {
            var expression = GetCreateTableExpression(tableName);
            expression.Columns[0].Type = DbType.String;
            expression.Columns[0].Size = 100;
            var sql = generator.Generate(expression);
            sql.ShouldBe("CREATE TABLE [NewTable] (ColumnName1 NVARCHAR(100) NOT NULL)");
        }

        [Test]
        [ExpectedException(typeof(NotImplementedException))]
        public void CannotCreateASchema()
        {
            var sql = generator.Generate(new CreateSchemaExpression());
        }

        [Test]
        [ExpectedException(typeof(NotImplementedException))]
        public void CannotAlterASchema()
        {
            var sql = generator.Generate(new AlterSchemaExpression());
        }

        [Test]
        [ExpectedException(typeof(NotImplementedException))]
        public void CannotDeleteASchema()
        {
            var sql = generator.Generate(new DeleteSchemaExpression());
        }

        private static CreateTableExpression GetCreateTableExpression(string tableName)
        {
            var columnName1 = "ColumnName1";

            var column1 = new ColumnDefinition { Name = columnName1, Type = DbType.String };

            var expression = new CreateTableExpression { TableName = tableName };
            expression.Columns.Add(column1);
            return expression;
        }

    }
}