
namespace FluentMigrator.Tests.Unit.Generators.GenericGeneratorTests
{
    using FluentMigrator.Expressions;
    using FluentMigrator.Model;
    using FluentMigrator.Runner.Generators;
    using NUnit.Framework;
    using NUnit.Should;
    using System.Data;
    using FluentMigrator.Runner.Generators.Generic;

    [TestFixture]
    public class CreateTableTests
    {
       
       
        protected CreateTableExpression GetCreateTableExpressionHelper(string tableName)
        {
            string columnName1 = "ColumnName1";

           var column1 = new ColumnDefinition { Name = columnName1, TableName = tableName, Type = DbType.String };
            var expression = new CreateTableExpression { TableName = tableName };
            expression.Columns.Add(column1);
            return expression;
        }

        GenericGenerator SUT = default(GenericGenerator);

        [SetUp]
        public void Setup()
        {
            SUT = new GenericGeneratorImplementor();
        }

        [Test]
        public void CanCreateTable()
        {
            string tableName = "NewTable";

            var expression = GetCreateTableExpressionHelper(tableName);
            var sql = SUT.Generate(expression);
            sql.ShouldBe("create table \"NewTable\" (\"ColumnName1\" NVARCHAR(255) NULL)");
        }



        
    }
}
