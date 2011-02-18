
namespace FluentMigrator.Tests.Unit.Generators
{
    using FluentMigrator.Expressions;
    using FluentMigrator.Model;
    using FluentMigrator.Runner.Generators;
    using NUnit.Framework;
    using NUnit.Should;
    using System.Data;
    using FluentMigrator.Runner.Generators.Generic;
    public class GenericGeneratorCreateTableTests : GenericGeneratorTestBase
    {
        [Test]
        public void CanCreateTable()
        {
            string tableName = "NewTable";

            var expression = GetCreateTableExpression(); ;
            var sql = SUT.Generate(expression);
            sql.ShouldBe("create table \"NewTable\" (\"ColumnName1\" NVARCHAR(255) NULL)");
        }



        
    }
}
