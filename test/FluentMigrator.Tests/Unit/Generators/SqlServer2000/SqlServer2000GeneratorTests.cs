using System;
using System.Data;
using FluentMigrator.Exceptions;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Extensions;
using FluentMigrator.Runner.Generators.SqlServer;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.SqlServer2000
{
    [TestFixture]
    public class SqlServer2000GeneratorTests
    {
        protected SqlServer2000Generator Generator;

        [SetUp]
        public void Setup()
        {
            Generator = new SqlServer2000Generator();
        }

        [Test]
        public void CanAlterColumnWithDefaultValue()
        {
            //TODO: This will fail if there are any keys attached 
            var expression = GeneratorTestHelper.GetAlterColumnExpression();
            expression.Column.DefaultValue = "Foo";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestTable1] ALTER COLUMN [TestColumn1] NVARCHAR(20) NOT NULL");
        }

        [Test]
        public void CanAlterDefaultConstraint()
        {
            var expression = GeneratorTestHelper.GetAlterDefaultConstraintExpression();

            string expected = "DECLARE @default sysname, @sql nvarchar(4000);" + Environment.NewLine + Environment.NewLine +
            "-- get name of default constraint" + Environment.NewLine +
            "SELECT @default = name" + Environment.NewLine +
            "FROM sys.default_constraints" + Environment.NewLine +
            "WHERE parent_object_id = object_id('[TestTable1]')" + Environment.NewLine + "" +
            "AND type = 'D'" + Environment.NewLine + "" +
            "AND parent_column_id = (" + Environment.NewLine +
            "SELECT column_id" + Environment.NewLine +
            "FROM sys.columns" + Environment.NewLine +
            "WHERE object_id = object_id('[TestTable1]')" + Environment.NewLine +
            "AND name = 'TestColumn1'" + Environment.NewLine +
            ");" + Environment.NewLine + Environment.NewLine +
            "-- create alter table command to drop constraint as string and run it" + Environment.NewLine +
            "SET @sql = N'ALTER TABLE [TestTable1] DROP CONSTRAINT ' + @default;" + Environment.NewLine +
            "EXEC sp_executesql @sql;" + Environment.NewLine + Environment.NewLine +
            "-- create alter table command to create new default constraint as string and run it" + Environment.NewLine +
            "ALTER TABLE [TestTable1] WITH NOCHECK ADD CONSTRAINT [DF_TestTable1_TestColumn1] DEFAULT(1) FOR [TestColumn1];";

            var result = Generator.Generate(expression);
            result.ShouldBe(expected);
        }

        [Test]
        public void CanAlterSchemaInStrictMode()
        {
            Generator.compatabilityMode = Runner.CompatabilityMode.STRICT;

            Assert.Throws<DatabaseOperationNotSupportedException>(() => Generator.Generate(new CreateSchemaExpression()));
        }

        [Test]
        public void CanAddColumnWithGetDateDefault()
        {
            var column = new ColumnDefinition
            {
                Name = "TestColumn1",
                Type = DbType.String,
                Size = 5,
                DefaultValue = "GetDate()"
            };
            var expression = new CreateColumnExpression { TableName = "TestTable1", Column = column };

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestTable1] ADD [TestColumn1] NVARCHAR(5) NOT NULL DEFAULT GetDate()");
        }

        [Test]
        public void CanCreateSchemaInStrictMode()
        {
            Generator.compatabilityMode = Runner.CompatabilityMode.STRICT;

            Assert.Throws<DatabaseOperationNotSupportedException>(() => Generator.Generate(new CreateSchemaExpression()));
        }

        [Test]
        public void CanCreateTableWithGetDateDefault()
        {
            var column = new ColumnDefinition
            {
                Name = "TestColumn1",
                Type = DbType.String,
                Size = 5,
                DefaultValue = "GetDate()"
            };
            var expression = new CreateTableExpression { TableName = "TestTable1" };
            expression.Columns.Add(column);

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE [TestTable1] ([TestColumn1] NVARCHAR(5) NOT NULL DEFAULT GetDate())");
        }

        [Test]
        public void CanCreateTableWithSeededIdentity()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithAutoIncrementExpression();
            expression.Columns[0].AdditionalFeatures.Add(SqlServerExtensions.IdentitySeed, 45);
            expression.Columns[0].AdditionalFeatures.Add(SqlServerExtensions.IdentityIncrement, 23);

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE [TestTable1] ([TestColumn1] INT NOT NULL IDENTITY(45,23), [TestColumn2] INT NOT NULL)");
        }

        [Test]
        public void CanDropSchemaInStrictMode()
        {
            Generator.compatabilityMode = Runner.CompatabilityMode.STRICT;

            Assert.Throws<DatabaseOperationNotSupportedException>(() => Generator.Generate(new DeleteSchemaExpression()));
        }

        [Test]
        public void CanGenerateNecessaryStatementsForADeleteDefaultExpression()
        {
            var expression = new DeleteDefaultConstraintExpression {ColumnName = "Name", SchemaName = "Personalia", TableName = "Person"};

            var expected = "DECLARE @default sysname, @sql nvarchar(4000);" + Environment.NewLine + Environment.NewLine +
                                    "-- get name of default constraint" + Environment.NewLine +
                                    "SELECT @default = name" + Environment.NewLine +
                                    "FROM sys.default_constraints" + Environment.NewLine +
                                    "WHERE parent_object_id = object_id('[Person]')" + Environment.NewLine +
                                    "AND type = 'D'" + Environment.NewLine +
                                    "AND parent_column_id = (" + Environment.NewLine +
                                    "SELECT column_id" + Environment.NewLine +
                                    "FROM sys.columns" + Environment.NewLine +
                                    "WHERE object_id = object_id('[Person]')" + Environment.NewLine +
                                    "AND name = 'Name'" + Environment.NewLine +
                                    ");" + Environment.NewLine + Environment.NewLine +
                                    "-- create alter table command to drop constraint as string and run it" + Environment.NewLine +
                                    "SET @sql = N'ALTER TABLE [Person] DROP CONSTRAINT ' + @default;" + Environment.NewLine +
                                    "EXEC sp_executesql @sql;";

            var result = Generator.Generate(expression);
            result.ShouldBe(expected);
        }
    }
}