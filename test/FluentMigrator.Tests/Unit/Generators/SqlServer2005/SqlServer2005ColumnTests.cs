#region License
//
// Copyright (c) 2018, Fluent Migrator Project
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

using System;
using System.Linq;

using FluentMigrator.Runner.Generators.SqlServer;
using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Generators.SqlServer2005
{
    [TestFixture]
    public class SqlServer2005ColumnTests : BaseColumnTests
    {
        protected SqlServer2005Generator Generator;

        [SetUp]
        public void Setup()
        {
            Generator = new SqlServer2005Generator();
        }

        [Test]
        public override void CanCreateNullableColumnWithCustomDomainTypeAndCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpressionWithNullableCustomType();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestSchema].[TestTable1] ADD [TestColumn1] MyDomainType NULL;");
        }

        [Test]
        public override void CanCreateNullableColumnWithCustomDomainTypeAndDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpressionWithNullableCustomType();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [dbo].[TestTable1] ADD [TestColumn1] MyDomainType NULL;");
        }

        [Test]
        public override void CanAlterColumnWithCustomSchema()
        {
            //TODO: This will fail if there are any keys attached
            var expression = GeneratorTestHelper.GetAlterColumnExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestSchema].[TestTable1] ALTER COLUMN [TestColumn1] NVARCHAR(20) NOT NULL;");
        }

        [Test]
        public override void CanAlterColumnWithDefaultSchema()
        {
            //TODO: This will fail if there are any keys attached
            var expression = GeneratorTestHelper.GetAlterColumnExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [dbo].[TestTable1] ALTER COLUMN [TestColumn1] NVARCHAR(20) NOT NULL;");
        }

        [Test]
        public override void CanCreateAutoIncrementColumnWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetAlterColumnAddAutoIncrementExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestSchema].[TestTable1] ALTER COLUMN [TestColumn1] INT NOT NULL IDENTITY(1,1);");
        }

        [Test]
        public override void CanCreateAutoIncrementColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetAlterColumnAddAutoIncrementExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [dbo].[TestTable1] ALTER COLUMN [TestColumn1] INT NOT NULL IDENTITY(1,1);");
        }

        [Test]
        public override void CanCreateColumnWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestSchema].[TestTable1] ADD [TestColumn1] NVARCHAR(5) NOT NULL;");
        }

        [Test]
        public override void CanCreateColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [dbo].[TestTable1] ADD [TestColumn1] NVARCHAR(5) NOT NULL;");
        }

        [Test]
        public override void CanCreateColumnWithSystemMethodAndCustomSchema()
        {
            var expressions = GeneratorTestHelper.GetCreateColumnWithSystemMethodExpression("TestSchema");
            var result = expressions.Select(x => (string)Generator.Generate((dynamic)x));
            result.ShouldBe([
                @"ALTER TABLE [TestSchema].[TestTable1] ADD [TestColumn1] DATETIME;",
                @"UPDATE [TestSchema].[TestTable1] SET [TestColumn1] = GETDATE() WHERE 1 = 1;",
            ]);
        }

        [Test]
        public override void CanCreateColumnWithSystemMethodAndDefaultSchema()
        {
            var expressions = GeneratorTestHelper.GetCreateColumnWithSystemMethodExpression();
            var result = expressions.Select(x => (string)Generator.Generate((dynamic)x));
            result.ShouldBe([
                @"ALTER TABLE [dbo].[TestTable1] ADD [TestColumn1] DATETIME;",
                @"UPDATE [dbo].[TestTable1] SET [TestColumn1] = GETDATE() WHERE 1 = 1;",
            ]);
        }

        [Test]
        public override void CanCreateDecimalColumnWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateDecimalColumnExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestSchema].[TestTable1] ADD [TestColumn1] DECIMAL(19,2) NOT NULL;");
        }

        [Test]
        public override void CanCreateDecimalColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateDecimalColumnExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [dbo].[TestTable1] ADD [TestColumn1] DECIMAL(19,2) NOT NULL;");
        }

        [Test]
        public override void CanDropColumnWithCustomSchema()
        {
            //This does not work if it is a primary key
            var expression = GeneratorTestHelper.GetDeleteColumnExpression();
            expression.SchemaName = "TestSchema";

            var expected = "DECLARE @default sysname, @sql nvarchar(max);" + Environment.NewLine + Environment.NewLine +
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
                         "SET @sql = N'ALTER TABLE [TestSchema].[TestTable1] DROP CONSTRAINT ' + QUOTENAME(@default);" + Environment.NewLine +
                         "EXEC sp_executesql @sql;" + Environment.NewLine + Environment.NewLine +
                         "-- now we can finally drop column" + Environment.NewLine +
                         "ALTER TABLE [TestSchema].[TestTable1] DROP COLUMN [TestColumn1];" + Environment.NewLine;

            var result = Generator.Generate(expression);
            result.ShouldBe(expected);
        }

        [Test]
        public override void CanDropColumnWithDefaultSchema()
        {
            //This does not work if it is a primary key
            var expression = GeneratorTestHelper.GetDeleteColumnExpression();

            var expected = "DECLARE @default sysname, @sql nvarchar(max);" + Environment.NewLine + Environment.NewLine +
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
                        "SET @sql = N'ALTER TABLE [dbo].[TestTable1] DROP CONSTRAINT ' + QUOTENAME(@default);" + Environment.NewLine +
                        "EXEC sp_executesql @sql;" + Environment.NewLine + Environment.NewLine +
                        "-- now we can finally drop column" + Environment.NewLine +
                        "ALTER TABLE [dbo].[TestTable1] DROP COLUMN [TestColumn1];" + Environment.NewLine;

            var result = Generator.Generate(expression);
            result.ShouldBe(expected);
        }

        [Test]
        public override void CanDropMultipleColumnsWithCustomSchema()
        {
            //This does not work if it is a primary key
            var expression = GeneratorTestHelper.GetDeleteColumnExpression(new [] { "TestColumn1", "TestColumn2" });
            expression.SchemaName = "TestSchema";

            var expected = "DECLARE @default sysname, @sql nvarchar(max);" + Environment.NewLine + Environment.NewLine +
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
                         "SET @sql = N'ALTER TABLE [TestSchema].[TestTable1] DROP CONSTRAINT ' + QUOTENAME(@default);" + Environment.NewLine +
                         "EXEC sp_executesql @sql;" + Environment.NewLine + Environment.NewLine +
                         "-- now we can finally drop column" + Environment.NewLine +
                         "ALTER TABLE [TestSchema].[TestTable1] DROP COLUMN [TestColumn1];" + Environment.NewLine +
                         "GO" + Environment.NewLine +
                         "DECLARE @default sysname, @sql nvarchar(max);" + Environment.NewLine + Environment.NewLine +
                         "-- get name of default constraint" + Environment.NewLine +
                         "SELECT @default = name" + Environment.NewLine +
                         "FROM sys.default_constraints" + Environment.NewLine +
                         "WHERE parent_object_id = object_id('[TestSchema].[TestTable1]')" + Environment.NewLine +
                         "AND type = 'D'" + Environment.NewLine +
                         "AND parent_column_id = (" + Environment.NewLine +
                         "SELECT column_id" + Environment.NewLine +
                         "FROM sys.columns" + Environment.NewLine +
                         "WHERE object_id = object_id('[TestSchema].[TestTable1]')" + Environment.NewLine +
                         "AND name = 'TestColumn2'" + Environment.NewLine +
                         ");" + Environment.NewLine + Environment.NewLine +
                         "-- create alter table command to drop constraint as string and run it" + Environment.NewLine +
                         "SET @sql = N'ALTER TABLE [TestSchema].[TestTable1] DROP CONSTRAINT ' + QUOTENAME(@default);" + Environment.NewLine +
                         "EXEC sp_executesql @sql;" + Environment.NewLine + Environment.NewLine +
                         "-- now we can finally drop column" + Environment.NewLine +
                         "ALTER TABLE [TestSchema].[TestTable1] DROP COLUMN [TestColumn2];" + Environment.NewLine;

            var result = Generator.Generate(expression);
            result.ShouldBe(expected);
        }

        [Test]
        public override void CanDropMultipleColumnsWithDefaultSchema()
        {
            //This does not work if it is a primary key
            var expression = GeneratorTestHelper.GetDeleteColumnExpression(new [] { "TestColumn1", "TestColumn2" });

            var expected = "DECLARE @default sysname, @sql nvarchar(max);" + Environment.NewLine + Environment.NewLine +
                        "-- get name of default constraint" + Environment.NewLine +
                        "SELECT @default = name" + Environment.NewLine +
                        "FROM sys.default_constraints" + Environment.NewLine +
                        "WHERE parent_object_id = object_id('[dbo].[TestTable1]')" + Environment.NewLine + "" +
                        "AND type = 'D'" + Environment.NewLine +
                        "AND parent_column_id = (" + Environment.NewLine +
                        "SELECT column_id" + Environment.NewLine +
                        "FROM sys.columns" + Environment.NewLine +
                        "WHERE object_id = object_id('[dbo].[TestTable1]')" + Environment.NewLine +
                        "AND name = 'TestColumn1'" + Environment.NewLine +
                        ");" + Environment.NewLine + Environment.NewLine +
                        "-- create alter table command to drop constraint as string and run it" + Environment.NewLine +
                        "SET @sql = N'ALTER TABLE [dbo].[TestTable1] DROP CONSTRAINT ' + QUOTENAME(@default);" + Environment.NewLine +
                        "EXEC sp_executesql @sql;" + Environment.NewLine + Environment.NewLine +
                        "-- now we can finally drop column" + Environment.NewLine +
                        "ALTER TABLE [dbo].[TestTable1] DROP COLUMN [TestColumn1];" + Environment.NewLine +
                        "GO" + Environment.NewLine +
                        "DECLARE @default sysname, @sql nvarchar(max);" + Environment.NewLine + Environment.NewLine +
                        "-- get name of default constraint" + Environment.NewLine +
                        "SELECT @default = name" + Environment.NewLine +
                        "FROM sys.default_constraints" + Environment.NewLine +
                        "WHERE parent_object_id = object_id('[dbo].[TestTable1]')" + Environment.NewLine + "" +
                        "AND type = 'D'" + Environment.NewLine +
                        "AND parent_column_id = (" + Environment.NewLine +
                        "SELECT column_id" + Environment.NewLine +
                        "FROM sys.columns" + Environment.NewLine +
                        "WHERE object_id = object_id('[dbo].[TestTable1]')" + Environment.NewLine +
                        "AND name = 'TestColumn2'" + Environment.NewLine +
                        ");" + Environment.NewLine + Environment.NewLine +
                        "-- create alter table command to drop constraint as string and run it" + Environment.NewLine +
                        "SET @sql = N'ALTER TABLE [dbo].[TestTable1] DROP CONSTRAINT ' + QUOTENAME(@default);" + Environment.NewLine +
                        "EXEC sp_executesql @sql;" + Environment.NewLine + Environment.NewLine +
                        "-- now we can finally drop column" + Environment.NewLine +
                        "ALTER TABLE [dbo].[TestTable1] DROP COLUMN [TestColumn2];" + Environment.NewLine;

            var result = Generator.Generate(expression);
            result.ShouldBe(expected);
        }

        [Test]
        public override void CanRenameColumnWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetRenameColumnExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("sp_rename N'[TestSchema].[TestTable1].[TestColumn1]', N'TestColumn2';");
        }

        [Test]
        public override void CanRenameColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetRenameColumnExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("sp_rename N'[dbo].[TestTable1].[TestColumn1]', N'TestColumn2';");
        }
    }
}
