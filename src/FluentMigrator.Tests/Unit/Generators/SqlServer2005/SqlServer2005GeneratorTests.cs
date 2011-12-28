using FluentMigrator.Expressions;
using FluentMigrator.Runner.Generators.SqlServer;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.SqlServer2005
{
    [TestFixture]
    public class SqlServer2005GeneratorTests
    {
        [SetUp]
        public void Setup()
        {
            generator = new SqlServer2008Generator();
        }

        private IMigrationGenerator generator;

        [Test]
        public void CanGenerateNecessaryStatementsForADeleteDefaultExpression()
        {
            var expression = new DeleteDefaultConstraintExpression {ColumnName = "Name", SchemaName = "Personalia", TableName = "Person"};

            const string expected = "DECLARE @default sysname, @sql nvarchar(max);\r\n\r\n" +
                                    "-- get name of default constraint\r\n" +
                                    "SELECT @default = name\r\n" +
                                    "FROM sys.default_constraints\r\n" +
                                    "WHERE parent_object_id = object_id('[Personalia].[Person]')\r\n" + "" +
                                    "AND type = 'D'\r\n" + "" +
                                    "AND parent_column_id = (\r\n" + "" +
                                    "SELECT column_id\r\n" +
                                    "FROM sys.columns\r\n" +
                                    "WHERE object_id = object_id('[Personalia].[Person]')\r\n" +
                                    "AND name = 'Name'\r\n" +
                                    ");\r\n\r\n" +
                                    "-- create alter table command to drop contraint as string and run it\r\n" +
                                    "SET @sql = N'ALTER TABLE [Personalia].[Person] DROP CONSTRAINT ' + @default;\r\n" +
                                    "EXEC sp_executesql @sql;";

            generator.Generate(expression).ShouldBe(expected);
        }
    }
}