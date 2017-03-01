using System;
using System.Data;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Extensions;
using FluentMigrator.Runner.Generators.SqlServer;
using Xunit;

namespace FluentMigrator.Tests.Unit.Generators.SqlServer2005
{
    public class SqlServer2005GeneratorTests
    {
        protected IMigrationGenerator Generator;

        public SqlServer2005GeneratorTests()
        {
            Generator = new SqlServer2008Generator();
        }

        [Fact]
        public void CanAlterDefaultConstraintWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetAlterDefaultConstraintExpression();
            expression.SchemaName = "TestSchema";

            string expected = "DECLARE @default sysname, @sql nvarchar(max);" + Environment.NewLine + Environment.NewLine +
            "-- get name of default constraint" + Environment.NewLine +
            "SELECT @default = name" + Environment.NewLine +
            "FROM sys.default_constraints" + Environment.NewLine +
            "WHERE parent_object_id = object_id('[TestSchema].[TestTable1]')" + Environment.NewLine +
            "AND type = 'D'" + Environment.NewLine +
            "AND parent_column_id = (" + Environment.NewLine +
            "SELECT column_id" + Environment.NewLine +
            "FROM sys.columns" + Environment.NewLine +
            "WHERE object_id = object_id('[TestSchema].[TestTable1]')" + Environment.NewLine +
            "AND name = 'TestColumn1'" + Environment.NewLine +
            ");" + Environment.NewLine + Environment.NewLine +
            "-- create alter table command to drop constraint as string and run it" + Environment.NewLine +
            "SET @sql = N'ALTER TABLE [TestSchema].[TestTable1] DROP CONSTRAINT ' + @default;" + Environment.NewLine +
            "EXEC sp_executesql @sql;" + Environment.NewLine + Environment.NewLine +
            "-- create alter table command to create new default constraint as string and run it" + Environment.NewLine +
            "ALTER TABLE [TestSchema].[TestTable1] WITH NOCHECK ADD CONSTRAINT [DF_TestTable1_TestColumn1] DEFAULT(1) FOR [TestColumn1];";

            var result = Generator.Generate(expression);
            result.ShouldBe(expected);
        }

        [Fact]
        public void CanAlterDefaultConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetAlterDefaultConstraintExpression();

            string expected = "DECLARE @default sysname, @sql nvarchar(max);" + Environment.NewLine + Environment.NewLine +
            "-- get name of default constraint" + Environment.NewLine +
            "SELECT @default = name" + Environment.NewLine +
            "FROM sys.default_constraints" + Environment.NewLine +
            "WHERE parent_object_id = object_id('[dbo].[TestTable1]')" + Environment.NewLine +
            "AND type = 'D'" + Environment.NewLine +
            "AND parent_column_id = (" + Environment.NewLine +
            "SELECT column_id" + Environment.NewLine +
            "FROM sys.columns" + Environment.NewLine +
            "WHERE object_id = object_id('[dbo].[TestTable1]')" + Environment.NewLine +
            "AND name = 'TestColumn1'" + Environment.NewLine +
            ");" + Environment.NewLine + Environment.NewLine +
            "-- create alter table command to drop constraint as string and run it" + Environment.NewLine +
            "SET @sql = N'ALTER TABLE [dbo].[TestTable1] DROP CONSTRAINT ' + @default;" + Environment.NewLine +
            "EXEC sp_executesql @sql;" + Environment.NewLine + Environment.NewLine +
            "-- create alter table command to create new default constraint as string and run it" + Environment.NewLine +
            "ALTER TABLE [dbo].[TestTable1] WITH NOCHECK ADD CONSTRAINT [DF_TestTable1_TestColumn1] DEFAULT(1) FOR [TestColumn1];";

            var result = Generator.Generate(expression);
            result.ShouldBe(expected);
        }

        [Fact]
        public void CanCreateIncludeIndexWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateIncludeIndexExpression();
            expression.Index.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE INDEX [TestIndex] ON [TestSchema].[TestTable1] ([TestColumn1] ASC) INCLUDE ([TestColumn2])");
        }

        [Fact]
        public void CanCreateIncludeIndexWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateIncludeIndexExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE INDEX [TestIndex] ON [dbo].[TestTable1] ([TestColumn1] ASC) INCLUDE ([TestColumn2])");
        }

        [Fact]
        public void CanCreateMultiColumnIncludeIndexWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateMultiIncludeIndexExpression();
            expression.Index.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE INDEX [TestIndex] ON [TestSchema].[TestTable1] ([TestColumn1] ASC) INCLUDE ([TestColumn2], [TestColumn3])");
        }

        [Fact]
        public void CanCreateMultiColumnIncludeIndexWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateMultiIncludeIndexExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE INDEX [TestIndex] ON [dbo].[TestTable1] ([TestColumn1] ASC) INCLUDE ([TestColumn2], [TestColumn3])");
        }

        [Fact]
        public void CanCreateTableWithNvarcharMaxWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            expression.SchemaName = "TestSchema";
            expression.Columns[0].Type = DbType.String;
            expression.Columns[0].Size = Int32.MaxValue;

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE [TestSchema].[TestTable1] ([TestColumn1] NVARCHAR(MAX) NOT NULL, [TestColumn2] INT NOT NULL)");
        }

        [Fact]
        public void CanCreateTableWithNvarcharMaxWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            expression.Columns[0].Type = DbType.String;
            expression.Columns[0].Size = Int32.MaxValue;

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE [dbo].[TestTable1] ([TestColumn1] NVARCHAR(MAX) NOT NULL, [TestColumn2] INT NOT NULL)");
        }

        [Fact]
        public void CanCreateTableWithSeededIdentityWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithAutoIncrementExpression();
            expression.Columns[0].AdditionalFeatures.Add(SqlServerExtensions.IdentitySeed, 45);
            expression.Columns[0].AdditionalFeatures.Add(SqlServerExtensions.IdentityIncrement, 23);

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE [dbo].[TestTable1] ([TestColumn1] INT NOT NULL IDENTITY(45,23), [TestColumn2] INT NOT NULL)");
        }

        [Fact]
        public void CanCreateXmlColumnWithCustomSchema()
        {
            var expression = new CreateColumnExpression();
            expression.TableName = "TestTable1";

            expression.Column = new ColumnDefinition();
            expression.Column.Name = "TestColumn1";
            expression.Column.Type = DbType.Xml;
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestSchema].[TestTable1] ADD [TestColumn1] XML NOT NULL");
        }

        [Fact]
        public void CanCreateXmlColumnWithDefaultSchema()
        {
            var expression = new CreateColumnExpression();
            expression.TableName = "TestTable1";

            expression.Column = new ColumnDefinition();
            expression.Column.Name = "TestColumn1";
            expression.Column.Type = DbType.Xml;

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [dbo].[TestTable1] ADD [TestColumn1] XML NOT NULL");
        }

        [Fact]
        public void CanDropDefaultExpression()
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

            var result = Generator.Generate(expression);
            result.ShouldBe(expected);
        }

        [Fact]
        public void CanUseSystemMethodCurrentDateTimeAsADefaultValueForAColumn()
        {
            var expression = new CreateColumnExpression
                {
                    Column = new ColumnDefinition
                        {
                            Name = "NewColumn", Type = DbType.DateTime, 
                            DefaultValue = SystemMethods.CurrentDateTime
                        }, 
                    TableName = "NewTable"
                };

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [dbo].[NewTable] ADD [NewColumn] DATETIME NOT NULL CONSTRAINT [DF__NewColumn] DEFAULT GETDATE()");
        }

        [Fact]
        public void CanUseSystemMethodCurrentUserAsADefaultValueForAColumn()
        {
            var expression = new CreateColumnExpression
                {
                    Column = new ColumnDefinition
                        {
                            Name = "NewColumn", 
                            Size = 15, 
                            Type = DbType.String, 
                            DefaultValue = SystemMethods.CurrentUser
                        }, 
                    TableName = "NewTable"
                };

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [dbo].[NewTable] ADD [NewColumn] NVARCHAR(15) NOT NULL CONSTRAINT [DF__NewColumn] DEFAULT CURRENT_USER");
        }

        [Fact]
        public void CanUseSystemMethodCurrentUTCDateTimeAsADefaultValueForAColumn()
        {
            var expression = new CreateColumnExpression
                {
                    Column = new ColumnDefinition
                        {
                            Name = "NewColumn", 
                            Type = DbType.DateTime,
                            DefaultValue = SystemMethods.CurrentUTCDateTime
                        }, TableName = "NewTable"
                };

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [dbo].[NewTable] ADD [NewColumn] DATETIME NOT NULL CONSTRAINT [DF__NewColumn] DEFAULT GETUTCDATE()");
        }

        [Fact]
        public void CanUseSystemMethodNewGuidAsADefaultValueForAColumn()
        {
            var expression = new CreateColumnExpression
                {
                    Column = new ColumnDefinition
                        {
                            Name = "NewColumn", 
                            Type = DbType.Guid, 
                            DefaultValue = SystemMethods.NewGuid
                        }, TableName = "NewTable"
                };

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [dbo].[NewTable] ADD [NewColumn] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF__NewColumn] DEFAULT NEWID()");
        }

        [Fact]
        public void CanUseSystemMethodNewSequentialIdAsADefaultValueForAColumn()
        {
            var expression = new CreateColumnExpression
                {
                    Column = new ColumnDefinition
                        {
                            Name = "NewColumn", 
                            Type = DbType.Guid, 
                            DefaultValue = SystemMethods.NewSequentialId
                        }, TableName = "NewTable"
                };

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [dbo].[NewTable] ADD [NewColumn] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF__NewColumn] DEFAULT NEWSEQUENTIALID()");
        }

        [Fact]
        public void CanCreateTableWithDescriptionAndColumnDescription()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithTableDescriptionAndColumnDescriptions();

            var result = Generator.Generate(expression);

            result.ShouldBe(@"CREATE TABLE [dbo].[TestTable1] ([TestColumn1] NVARCHAR(255), [TestColumn2] INT NOT NULL)" + Environment.NewLine +
                            "GO" + Environment.NewLine +
                            "EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'TestDescription', @level0type=N'SCHEMA', @level0name='dbo', @level1type=N'TABLE', @level1name='TestTable1';EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'TestColumn1Description', @level0type = N'SCHEMA', @level0name = 'dbo', @level1type = N'Table', @level1name = 'TestTable1', @level2type = N'Column',  @level2name = 'TestColumn1';EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'TestColumn2Description', @level0type = N'SCHEMA', @level0name = 'dbo', @level1type = N'Table', @level1name = 'TestTable1', @level2type = N'Column',  @level2name = 'TestColumn2'" + Environment.NewLine);
        }

        [Fact]
        public void CanAlterTableWithDescription()
        {
            var expression = GeneratorTestHelper.GetAlterTableWithDescriptionExpression();

            var result = Generator.Generate(expression);

            result.ShouldBe(@"IF EXISTS ( SELECT * FROM fn_listextendedproperty(N'MS_Description', N'SCHEMA', N'dbo', N'TABLE', N'TestTable1', NULL, NULL)) EXEC sys.sp_dropextendedproperty @name=N'MS_Description', @level0type=N'SCHEMA', @level0name='dbo', @level1type=N'TABLE', @level1name='TestTable1';EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'TestDescription', @level0type=N'SCHEMA', @level0name='dbo', @level1type=N'TABLE', @level1name='TestTable1'");
        }

        [Fact]
        public void CanCreateColumnWithDescription()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpressionWithDescription();

            var result = Generator.Generate(expression);

            result.ShouldBe(@"ALTER TABLE [dbo].[TestTable1] ADD [TestColumn1] NVARCHAR(5) NOT NULL" + Environment.NewLine +
                            "GO" + Environment.NewLine +
                            "EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'TestColumn1Description', @level0type = N'SCHEMA', @level0name = 'dbo', @level1type = N'Table', @level1name = 'TestTable1', @level2type = N'Column',  @level2name = 'TestColumn1'" + Environment.NewLine);
        }

        [Fact]
        public void CanAlterColumnWithDescription()
        {
            var expression = GeneratorTestHelper.GetAlterColumnExpressionWithDescription();

            var result = Generator.Generate(expression);

            result.ShouldBe(@"ALTER TABLE [dbo].[TestTable1] ALTER COLUMN [TestColumn1] NVARCHAR(20) NOT NULL" + Environment.NewLine +
                            "GO" + Environment.NewLine +
                            "IF EXISTS (SELECT * FROM fn_listextendedproperty(N'MS_Description', N'SCHEMA', N'dbo', N'TABLE', N'TestTable1', N'Column', N'TestColumn1' )) EXEC sys.sp_dropextendedproperty @name=N'MS_Description', @level0type = N'SCHEMA', @level0name = 'dbo', @level1type = N'Table', @level1name = 'TestTable1', @level2type = N'Column',  @level2name = 'TestColumn1';EXEC sys.sp_addextendedproperty @name = N'MS_Description', @value = N'TestColumn1Description', @level0type = N'SCHEMA', @level0name = 'dbo', @level1type = N'Table', @level1name = 'TestTable1', @level2type = N'Column',  @level2name = 'TestColumn1'" + Environment.NewLine);
        }

        [Fact]
        public void CanCreateColumnWithCollation()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpressionWithCollation();

            var result = Generator.Generate(expression);

            result.ShouldBe(@"ALTER TABLE [dbo].[TestTable1] ADD [TestColumn1] NVARCHAR(5) COLLATE " + GeneratorTestHelper.TestColumnCollationName + " NOT NULL");
        }

        [Fact]
        public void CanAlterColumnWithCollation()
        {
            var expression = GeneratorTestHelper.GetAlterColumnExpressionWithCollation();

            var result = Generator.Generate(expression);

            result.ShouldBe(@"ALTER TABLE [dbo].[TestTable1] ALTER COLUMN [TestColumn1] NVARCHAR(20) COLLATE " + GeneratorTestHelper.TestColumnCollationName + " NOT NULL");
        }
    }
}
