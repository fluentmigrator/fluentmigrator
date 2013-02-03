using System;
using System.Data;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
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

            string expected = "DECLARE @default sysname, @sql nvarchar(max);" + Environment.NewLine + Environment.NewLine +
                                    "-- get name of default constraint" + Environment.NewLine +
                                    "SELECT @default = name" + Environment.NewLine +
                                    "FROM sys.default_constraints" + Environment.NewLine +
                                    "WHERE parent_object_id = object_id('[Personalia].[Person]')" + Environment.NewLine +
                                    "AND type = 'D'" + Environment.NewLine +
                                    "AND parent_column_id = (" + Environment.NewLine +
                                    "SELECT column_id" + Environment.NewLine +
                                    "FROM sys.columns" + Environment.NewLine +
                                    "WHERE object_id = object_id('[Personalia].[Person]')" + Environment.NewLine +
                                    "AND name = 'Name'" + Environment.NewLine +
                                    ");" + Environment.NewLine + Environment.NewLine +
                                    "-- create alter table command to drop constraint as string and run it" + Environment.NewLine +
                                    "SET @sql = N'ALTER TABLE [Personalia].[Person] DROP CONSTRAINT ' + @default;" + Environment.NewLine +
                                    "EXEC sp_executesql @sql;";

            generator.Generate(expression).ShouldBe(expected);
        }

        [Test]
        public void CanUseSystemMethodCurrentUserAsADefaultValueForAColumn()
        {
            const string tableName = "NewTable";

            var columnDefinition = new ColumnDefinition { Name = "NewColumn", Size = 15, Type = DbType.String, DefaultValue = SystemMethods.CurrentUser };

            var expression = new CreateColumnExpression { Column = columnDefinition, TableName = tableName };

            string sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE [dbo].[NewTable] ADD [NewColumn] NVARCHAR(15) NOT NULL CONSTRAINT [DF__NewColumn] DEFAULT CURRENT_USER");
        }

        [Test]
        public void CanUseSystemMethodNewGuidAsADefaultValueForAColumn()
        {
            const string tableName = "NewTable";

            var columnDefinition = new ColumnDefinition { Name = "NewColumn", Type = DbType.Guid, DefaultValue = SystemMethods.NewGuid };

            var expression = new CreateColumnExpression { Column = columnDefinition, TableName = tableName };

            string sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE [dbo].[NewTable] ADD [NewColumn] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF__NewColumn] DEFAULT NEWID()");
        }

        [Test]
        public void CanUseSystemMethodNewSequentialIdAsADefaultValueForAColumn()
        {
            const string tableName = "NewTable";

            var columnDefinition = new ColumnDefinition { Name = "NewColumn", Type = DbType.Guid, DefaultValue = SystemMethods.NewSequentialId };

            var expression = new CreateColumnExpression { Column = columnDefinition, TableName = tableName };

            string sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE [dbo].[NewTable] ADD [NewColumn] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF__NewColumn] DEFAULT NEWSEQUENTIALID()");
        }

        [Test]
        public void CanUseSystemMethodCurrentDateTimeAsADefaultValueForAColumn()
        {
            const string tableName = "NewTable";

            var columnDefinition = new ColumnDefinition { Name = "NewColumn", Type = DbType.DateTime, DefaultValue = SystemMethods.CurrentDateTime };

            var expression = new CreateColumnExpression { Column = columnDefinition, TableName = tableName };

            string sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE [dbo].[NewTable] ADD [NewColumn] DATETIME NOT NULL CONSTRAINT [DF__NewColumn] DEFAULT GETDATE()");
        }

        [Test]
        public void CanUseSystemMethodCurrentUTCDateTimeAsADefaultValueForAColumn()
        {
            const string tableName = "NewTable";

            var columnDefinition = new ColumnDefinition { Name = "NewColumn", Type = DbType.DateTime, DefaultValue = SystemMethods.CurrentUTCDateTime };

            var expression = new CreateColumnExpression { Column = columnDefinition, TableName = tableName };

            string sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE [dbo].[NewTable] ADD [NewColumn] DATETIME NOT NULL CONSTRAINT [DF__NewColumn] DEFAULT GETUTCDATE()");
        }
    }
}