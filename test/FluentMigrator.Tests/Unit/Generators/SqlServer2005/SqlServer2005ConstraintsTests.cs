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
using System.Collections.Generic;
using System.Data;
using System.Linq;

using FluentMigrator.Builders.Create.Constraint;
using FluentMigrator.Builders.Delete.Constraint;
using FluentMigrator.Model;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Generators.SqlServer;
using FluentMigrator.SqlServer;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Generators.SqlServer2005
{
    [TestFixture]
    public class SqlServer2005ConstraintsTests : BaseConstraintsTests
    {
        protected SqlServer2005Generator Generator;

        [SetUp]
        public void Setup()
        {
            Generator = new SqlServer2005Generator();
        }

        [Test]
        public void CanAlterDefaultConstraintWithCurrentUserAsDefault()
        {
            var expression = GeneratorTestHelper.GetAlterDefaultConstraintExpression();
            expression.DefaultValue = SystemMethods.CurrentUser;

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
            "SET @sql = N'ALTER TABLE [dbo].[TestTable1] DROP CONSTRAINT ' + QUOTENAME(@default);" + Environment.NewLine +
            "EXEC sp_executesql @sql;" + Environment.NewLine + Environment.NewLine +
            "-- create alter table command to create new default constraint as string and run it" + Environment.NewLine +
            "ALTER TABLE [dbo].[TestTable1] WITH NOCHECK ADD CONSTRAINT [DF_TestTable1_TestColumn1] DEFAULT(CURRENT_USER) FOR [TestColumn1];";

            var result = Generator.Generate(expression);
            result.ShouldBe(expected);
        }

        [Test]
        public void CanAlterDefaultConstraintWithCurrentDateAsDefault()
        {
            var expression = GeneratorTestHelper.GetAlterDefaultConstraintExpression();
            expression.DefaultValue = SystemMethods.CurrentDateTime;

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
            "SET @sql = N'ALTER TABLE [dbo].[TestTable1] DROP CONSTRAINT ' + QUOTENAME(@default);" + Environment.NewLine +
            "EXEC sp_executesql @sql;" + Environment.NewLine + Environment.NewLine +
            "-- create alter table command to create new default constraint as string and run it" + Environment.NewLine +
            "ALTER TABLE [dbo].[TestTable1] WITH NOCHECK ADD CONSTRAINT [DF_TestTable1_TestColumn1] DEFAULT(GETDATE()) FOR [TestColumn1];";

            var result = Generator.Generate(expression);
            result.ShouldBe(expected);
        }

        [Test]
        public void CanAlterDefaultConstraintWithCurrentUtcDateAsDefault()
        {
            var expression = GeneratorTestHelper.GetAlterDefaultConstraintExpression();
            expression.DefaultValue = SystemMethods.CurrentUTCDateTime;

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
            "SET @sql = N'ALTER TABLE [dbo].[TestTable1] DROP CONSTRAINT ' + QUOTENAME(@default);" + Environment.NewLine +
            "EXEC sp_executesql @sql;" + Environment.NewLine + Environment.NewLine +
            "-- create alter table command to create new default constraint as string and run it" + Environment.NewLine +
            "ALTER TABLE [dbo].[TestTable1] WITH NOCHECK ADD CONSTRAINT [DF_TestTable1_TestColumn1] DEFAULT(GETUTCDATE()) FOR [TestColumn1];";

            var result = Generator.Generate(expression);
            result.ShouldBe(expected);
        }

        [Test]
        public void CanAlterDefaultConstraintWithCurrentDateTimeOffsetUsingGetUtcDateAsDefault()
        {
            var expression = GeneratorTestHelper.GetAlterDefaultConstraintExpression();
            expression.DefaultValue = SystemMethods.CurrentDateTimeOffset;

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
                "SET @sql = N'ALTER TABLE [dbo].[TestTable1] DROP CONSTRAINT ' + QUOTENAME(@default);" + Environment.NewLine +
                "EXEC sp_executesql @sql;" + Environment.NewLine + Environment.NewLine +
                "-- create alter table command to create new default constraint as string and run it" + Environment.NewLine +
                "ALTER TABLE [dbo].[TestTable1] WITH NOCHECK ADD CONSTRAINT [DF_TestTable1_TestColumn1] DEFAULT(GETUTCDATE()) FOR [TestColumn1];";

            var result = Generator.Generate(expression);
            result.ShouldBe(expected);
        }

        [Test]
        public void CanAlterDefaultConstraintWithNewGuidAsDefault()
        {
            var expression = GeneratorTestHelper.GetAlterDefaultConstraintExpression();
            expression.DefaultValue = SystemMethods.NewGuid;

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
            "SET @sql = N'ALTER TABLE [dbo].[TestTable1] DROP CONSTRAINT ' + QUOTENAME(@default);" + Environment.NewLine +
            "EXEC sp_executesql @sql;" + Environment.NewLine + Environment.NewLine +
            "-- create alter table command to create new default constraint as string and run it" + Environment.NewLine +
            "ALTER TABLE [dbo].[TestTable1] WITH NOCHECK ADD CONSTRAINT [DF_TestTable1_TestColumn1] DEFAULT(NEWID()) FOR [TestColumn1];";

            var result = Generator.Generate(expression);
            result.ShouldBe(expected);
        }

        [Test]
        public void CanAlterDefaultConstraintWithStringAsDefault()
        {
            var expression = GeneratorTestHelper.GetAlterDefaultConstraintExpression();
            expression.DefaultValue = "TestString";

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
            "SET @sql = N'ALTER TABLE [dbo].[TestTable1] DROP CONSTRAINT ' + QUOTENAME(@default);" + Environment.NewLine +
            "EXEC sp_executesql @sql;" + Environment.NewLine + Environment.NewLine +
            "-- create alter table command to create new default constraint as string and run it" + Environment.NewLine +
            "ALTER TABLE [dbo].[TestTable1] WITH NOCHECK ADD CONSTRAINT [DF_TestTable1_TestColumn1] DEFAULT(N'TestString') FOR [TestColumn1];";

            var result = Generator.Generate(expression);
            result.ShouldBe(expected);
        }

        [Test]
        public void CanAlterDefaultConstraintWithSqlFunctionAsDefault()
        {
            var expression = GeneratorTestHelper.GetAlterDefaultConstraintExpression();
            expression.DefaultValue = "MyTestFunction()";

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
            "SET @sql = N'ALTER TABLE [dbo].[TestTable1] DROP CONSTRAINT ' + QUOTENAME(@default);" + Environment.NewLine +
            "EXEC sp_executesql @sql;" + Environment.NewLine + Environment.NewLine +
            "-- create alter table command to create new default constraint as string and run it" + Environment.NewLine +
            "ALTER TABLE [dbo].[TestTable1] WITH NOCHECK ADD CONSTRAINT [DF_TestTable1_TestColumn1] DEFAULT(MyTestFunction()) FOR [TestColumn1];";

            var result = Generator.Generate(expression);
            result.ShouldBe(expected);
        }

        [Test]
        public override void CanCreateForeignKeyWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateForeignKeyExpression();
            expression.ForeignKey.ForeignTableSchema = "TestSchema";
            expression.ForeignKey.PrimaryTableSchema = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestSchema].[TestTable1] ADD CONSTRAINT [FK_TestTable1_TestColumn1_TestTable2_TestColumn2] FOREIGN KEY ([TestColumn1]) REFERENCES [TestSchema].[TestTable2] ([TestColumn2]);");
        }

        [Test]
        public override void CanCreateForeignKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateForeignKeyExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [dbo].[TestTable1] ADD CONSTRAINT [FK_TestTable1_TestColumn1_TestTable2_TestColumn2] FOREIGN KEY ([TestColumn1]) REFERENCES [dbo].[TestTable2] ([TestColumn2]);");
        }

        [Test]
        public override void CanCreateForeignKeyWithDifferentSchemas()
        {
            var expression = GeneratorTestHelper.GetCreateForeignKeyExpression();
            expression.ForeignKey.ForeignTableSchema = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestSchema].[TestTable1] ADD CONSTRAINT [FK_TestTable1_TestColumn1_TestTable2_TestColumn2] FOREIGN KEY ([TestColumn1]) REFERENCES [dbo].[TestTable2] ([TestColumn2]);");
        }

        [Test]
        public override void CanCreateMultiColumnForeignKeyWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnForeignKeyExpression();
            expression.ForeignKey.ForeignTableSchema = "TestSchema";
            expression.ForeignKey.PrimaryTableSchema = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestSchema].[TestTable1] ADD CONSTRAINT [FK_TestTable1_TestColumn1_TestColumn3_TestTable2_TestColumn2_TestColumn4] FOREIGN KEY ([TestColumn1], [TestColumn3]) REFERENCES [TestSchema].[TestTable2] ([TestColumn2], [TestColumn4]);");
        }

        [Test]
        public override void CanCreateMultiColumnForeignKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnForeignKeyExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [dbo].[TestTable1] ADD CONSTRAINT [FK_TestTable1_TestColumn1_TestColumn3_TestTable2_TestColumn2_TestColumn4] FOREIGN KEY ([TestColumn1], [TestColumn3]) REFERENCES [dbo].[TestTable2] ([TestColumn2], [TestColumn4]);");
        }

        [Test]
        public override void CanCreateMultiColumnForeignKeyWithDifferentSchemas()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnForeignKeyExpression();
            expression.ForeignKey.ForeignTableSchema = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestSchema].[TestTable1] ADD CONSTRAINT [FK_TestTable1_TestColumn1_TestColumn3_TestTable2_TestColumn2_TestColumn4] FOREIGN KEY ([TestColumn1], [TestColumn3]) REFERENCES [dbo].[TestTable2] ([TestColumn2], [TestColumn4]);");
        }

        public override void CanCreateMultiColumnPrimaryKeyConstraintWithCustomSchema([Values] CompatibilityMode compatibilityMode)
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnPrimaryKeyExpression();
            expression.Constraint.SchemaName = "TestSchema";

            Generator.CompatibilityMode = compatibilityMode;
            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestSchema].[TestTable1] ADD CONSTRAINT [PK_TestTable1_TestColumn1_TestColumn2] PRIMARY KEY ([TestColumn1], [TestColumn2]);");
        }

        [Test]
        public override void CanCreateMultiColumnPrimaryKeyConstraintWithDefaultSchema([Values] CompatibilityMode compatibilityMode)
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnPrimaryKeyExpression();

            Generator.CompatibilityMode = compatibilityMode;
            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [dbo].[TestTable1] ADD CONSTRAINT [PK_TestTable1_TestColumn1_TestColumn2] PRIMARY KEY ([TestColumn1], [TestColumn2]);");
        }

        [Test]
        public override void CanCreateMultiColumnUniqueConstraintWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnUniqueConstraintExpression();
            expression.Constraint.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestSchema].[TestTable1] ADD CONSTRAINT [UC_TestTable1_TestColumn1_TestColumn2] UNIQUE ([TestColumn1], [TestColumn2]);");
        }

        [Test]
        public virtual void CanCreateMultiColumnUniqueConstraintWithCustomSchemaAndFilter()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnUniqueConstraintExpression();
            expression.Constraint.SchemaName = "TestSchema";
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.UniqueConstraintFilter, expression.Constraint.Columns.First() + " IS NOT NULL");

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE UNIQUE INDEX [UC_TestTable1_TestColumn1_TestColumn2] ON [TestSchema].[TestTable1] ([TestColumn1], [TestColumn2]) WHERE TestColumn1 IS NOT NULL;");
        }

        [Test]
        public virtual void CanCreateMultiColumnUniqueConstraintWithCustomSchemaAndFilterWithNonClusteredIndex()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnUniqueConstraintExpression();
            expression.Constraint.SchemaName = "TestSchema";
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.ConstraintType, SqlServerConstraintType.NonClustered);
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.UniqueConstraintFilter, expression.Constraint.Columns.First() + " IS NOT NULL");

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE UNIQUE INDEX [UC_TestTable1_TestColumn1_TestColumn2] ON [TestSchema].[TestTable1] ([TestColumn1], [TestColumn2]) WHERE TestColumn1 IS NOT NULL;");
        }

        [Test]
        public virtual void CannotCreateMultiColumnUniqueConstraintWithCustomSchemaAndFilterWithClusteredIndex()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueConstraintExpression();
            expression.Constraint.SchemaName = "TestSchema";
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.ConstraintType, SqlServerConstraintType.Clustered);
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.UniqueConstraintFilter, expression.Constraint.Columns.First() + " IS NOT NULL");

            var ex = Assert.Throws<Exception>(() => Generator.Generate(expression));
            Assert.That(ex, Is.Not.Null);
            Assert.That(ex.Message, Is.EqualTo("Filtered indexes are non-clustered indexes that have the addition of a WHERE clause. SQL Server does not support clustered filtered indexes. Create a non-clustered index with include columns instead to create a non-clustered covering index."));
        }

        [Test]
        public virtual void CanCreateMultiColumnUniqueConstraintWithCustomSchemaAndInclude()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnUniqueConstraintExpression();
            expression.Constraint.SchemaName = "TestSchema";
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.UniqueConstraintIncludesList, new List<IndexIncludeDefinition> { new IndexIncludeDefinition { Name = "TestColumn3" } });

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE UNIQUE INDEX [UC_TestTable1_TestColumn1_TestColumn2] ON [TestSchema].[TestTable1] ([TestColumn1], [TestColumn2]) INCLUDE ([TestColumn3]);");
        }

        [Test]
        public virtual void CanCreateMultiColumnUniqueConstraintWithCustomSchemaAndFilterAndInclude()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnUniqueConstraintExpression();
            expression.Constraint.SchemaName = "TestSchema";
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.UniqueConstraintFilter, expression.Constraint.Columns.First() + " IS NOT NULL");
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.UniqueConstraintIncludesList, new List<IndexIncludeDefinition> { new IndexIncludeDefinition { Name = "TestColumn3" } });

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE UNIQUE INDEX [UC_TestTable1_TestColumn1_TestColumn2] ON [TestSchema].[TestTable1] ([TestColumn1], [TestColumn2]) INCLUDE ([TestColumn3]) WHERE TestColumn1 IS NOT NULL;");
        }

        [Test]
        public virtual void CanCreateMultiColumnUniqueConstraintWithCustomSchemaAndFilterAndIncludeWithNonClusteredIndex()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnUniqueConstraintExpression();
            expression.Constraint.SchemaName = "TestSchema";
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.ConstraintType, SqlServerConstraintType.NonClustered);
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.UniqueConstraintFilter, expression.Constraint.Columns.First() + " IS NOT NULL");
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.UniqueConstraintIncludesList, new List<IndexIncludeDefinition> { new IndexIncludeDefinition { Name = "TestColumn3" } });

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE UNIQUE INDEX [UC_TestTable1_TestColumn1_TestColumn2] ON [TestSchema].[TestTable1] ([TestColumn1], [TestColumn2]) INCLUDE ([TestColumn3]) WHERE TestColumn1 IS NOT NULL;");
        }

        [Test]
        public virtual void CannotCreateMultiColumnUniqueConstraintWithCustomSchemaAndFilterAndIncludeWithClusteredIndex()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueConstraintExpression();
            expression.Constraint.SchemaName = "TestSchema";
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.ConstraintType, SqlServerConstraintType.Clustered);
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.UniqueConstraintFilter, expression.Constraint.Columns.First() + " IS NOT NULL");
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.UniqueConstraintIncludesList, new List<IndexIncludeDefinition> { new IndexIncludeDefinition { Name = "TestColumn3" } });

            var ex = Assert.Throws<Exception>(() => Generator.Generate(expression));
            Assert.That(ex, Is.Not.Null);
            Assert.That(ex.Message, Is.EqualTo("Filtered indexes are non-clustered indexes that have the addition of a WHERE clause. SQL Server does not support clustered filtered indexes. Create a non-clustered index with include columns instead to create a non-clustered covering index."));
        }

        [Test]
        public override void CanCreateMultiColumnUniqueConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnUniqueConstraintExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [dbo].[TestTable1] ADD CONSTRAINT [UC_TestTable1_TestColumn1_TestColumn2] UNIQUE ([TestColumn1], [TestColumn2]);");
        }

        [Test]
        public virtual void CanCreateMultiColumnUniqueConstraintWithDefaultSchemaAndFilter()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnUniqueConstraintExpression();
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.UniqueConstraintFilter, expression.Constraint.Columns.First() + " IS NOT NULL");

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE UNIQUE INDEX [UC_TestTable1_TestColumn1_TestColumn2] ON [dbo].[TestTable1] ([TestColumn1], [TestColumn2]) WHERE TestColumn1 IS NOT NULL;");
        }

        [Test]
        public virtual void CanCreateMultiColumnUniqueConstraintWithDefaultSchemaAndFilterWithNonClusteredIndex()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnUniqueConstraintExpression();
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.ConstraintType, SqlServerConstraintType.NonClustered);
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.UniqueConstraintFilter, expression.Constraint.Columns.First() + " IS NOT NULL");

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE UNIQUE INDEX [UC_TestTable1_TestColumn1_TestColumn2] ON [dbo].[TestTable1] ([TestColumn1], [TestColumn2]) WHERE TestColumn1 IS NOT NULL;");
        }

        [Test]
        public virtual void CannotCreateMultiColumnUniqueConstraintWithDefaultSchemaAndFilterWithClusteredIndex()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueConstraintExpression();
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.ConstraintType, SqlServerConstraintType.Clustered);
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.UniqueConstraintFilter, expression.Constraint.Columns.First() + " IS NOT NULL");

            var ex = Assert.Throws<Exception>(() => Generator.Generate(expression));
            Assert.That(ex, Is.Not.Null);
            Assert.That(ex.Message, Is.EqualTo("Filtered indexes are non-clustered indexes that have the addition of a WHERE clause. SQL Server does not support clustered filtered indexes. Create a non-clustered index with include columns instead to create a non-clustered covering index."));
        }

        [Test]
        public virtual void CanCreateMultiColumnUniqueConstraintWithDefaultSchemaAndInclude()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnUniqueConstraintExpression();
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.UniqueConstraintIncludesList, new List<IndexIncludeDefinition> { new IndexIncludeDefinition { Name = "TestColumn3" } });

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE UNIQUE INDEX [UC_TestTable1_TestColumn1_TestColumn2] ON [dbo].[TestTable1] ([TestColumn1], [TestColumn2]) INCLUDE ([TestColumn3]);");
        }

        [Test]
        public virtual void CanCreateMultiColumnUniqueConstraintWithDefaultSchemaAndFilterAndInclude()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnUniqueConstraintExpression();
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.UniqueConstraintFilter, expression.Constraint.Columns.First() + " IS NOT NULL");
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.UniqueConstraintIncludesList, new List<IndexIncludeDefinition> { new IndexIncludeDefinition { Name = "TestColumn3" } });

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE UNIQUE INDEX [UC_TestTable1_TestColumn1_TestColumn2] ON [dbo].[TestTable1] ([TestColumn1], [TestColumn2]) INCLUDE ([TestColumn3]) WHERE TestColumn1 IS NOT NULL;");
        }

        [Test]
        public virtual void CanCreateMultiColumnUniqueConstraintWithDefaultSchemaAndFilterAndIncludeWithNonClusteredIndex()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnUniqueConstraintExpression();
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.ConstraintType, SqlServerConstraintType.NonClustered);
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.UniqueConstraintFilter, expression.Constraint.Columns.First() + " IS NOT NULL");
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.UniqueConstraintIncludesList, new List<IndexIncludeDefinition> { new IndexIncludeDefinition { Name = "TestColumn3" } });

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE UNIQUE INDEX [UC_TestTable1_TestColumn1_TestColumn2] ON [dbo].[TestTable1] ([TestColumn1], [TestColumn2]) INCLUDE ([TestColumn3]) WHERE TestColumn1 IS NOT NULL;");
        }

        [Test]
        public virtual void CannotCreateMultiColumnUniqueConstraintWithDefaultSchemaAndFilterAndIncludeWithClusteredIndex()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueConstraintExpression();
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.ConstraintType, SqlServerConstraintType.Clustered);
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.UniqueConstraintFilter, expression.Constraint.Columns.First() + " IS NOT NULL");
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.UniqueConstraintIncludesList, new List<IndexIncludeDefinition> { new IndexIncludeDefinition { Name = "TestColumn3" } });

            var ex = Assert.Throws<Exception>(() => Generator.Generate(expression));
            Assert.That(ex, Is.Not.Null);
            Assert.That(ex.Message, Is.EqualTo("Filtered indexes are non-clustered indexes that have the addition of a WHERE clause. SQL Server does not support clustered filtered indexes. Create a non-clustered index with include columns instead to create a non-clustered covering index."));
        }

        [Test]
        public override void CanCreateNamedForeignKeyWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedForeignKeyExpression();
            expression.ForeignKey.ForeignTableSchema = "TestSchema";
            expression.ForeignKey.PrimaryTableSchema = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestSchema].[TestTable1] ADD CONSTRAINT [FK_Test] FOREIGN KEY ([TestColumn1]) REFERENCES [TestSchema].[TestTable2] ([TestColumn2]);");
        }

        [Test]
        public override void CanCreateNamedForeignKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedForeignKeyExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [dbo].[TestTable1] ADD CONSTRAINT [FK_Test] FOREIGN KEY ([TestColumn1]) REFERENCES [dbo].[TestTable2] ([TestColumn2]);");
        }

        [Test]
        public override void CanCreateNamedForeignKeyWithDifferentSchemas()
        {
            var expression = GeneratorTestHelper.GetCreateNamedForeignKeyExpression();
            expression.ForeignKey.ForeignTableSchema = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestSchema].[TestTable1] ADD CONSTRAINT [FK_Test] FOREIGN KEY ([TestColumn1]) REFERENCES [dbo].[TestTable2] ([TestColumn2]);");
        }

        [Test]
        public override void CanCreateNamedForeignKeyWithOnDeleteAndOnUpdateOptions()
        {
            var expression = GeneratorTestHelper.GetCreateNamedForeignKeyExpression();
            expression.ForeignKey.OnDelete = Rule.Cascade;
            expression.ForeignKey.OnUpdate = Rule.SetDefault;

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [dbo].[TestTable1] ADD CONSTRAINT [FK_Test] FOREIGN KEY ([TestColumn1]) REFERENCES [dbo].[TestTable2] ([TestColumn2]) ON DELETE CASCADE ON UPDATE SET DEFAULT;");
        }

        [TestCase(Rule.SetDefault, "SET DEFAULT"), TestCase(Rule.SetNull, "SET NULL"), TestCase(Rule.Cascade, "CASCADE")]
        public override void CanCreateNamedForeignKeyWithOnDeleteOptions(Rule rule, string output)
        {
            var expression = GeneratorTestHelper.GetCreateNamedForeignKeyExpression();
            expression.ForeignKey.OnDelete = rule;

            var result = Generator.Generate(expression);
            result.ShouldBe(string.Format("ALTER TABLE [dbo].[TestTable1] ADD CONSTRAINT [FK_Test] FOREIGN KEY ([TestColumn1]) REFERENCES [dbo].[TestTable2] ([TestColumn2]) ON DELETE {0};", output));
        }

        [TestCase(Rule.SetDefault, "SET DEFAULT"), TestCase(Rule.SetNull, "SET NULL"), TestCase(Rule.Cascade, "CASCADE")]
        public override void CanCreateNamedForeignKeyWithOnUpdateOptions(Rule rule, string output)
        {
            var expression = GeneratorTestHelper.GetCreateNamedForeignKeyExpression();
            expression.ForeignKey.OnUpdate = rule;

            var result = Generator.Generate(expression);
            result.ShouldBe(string.Format("ALTER TABLE [dbo].[TestTable1] ADD CONSTRAINT [FK_Test] FOREIGN KEY ([TestColumn1]) REFERENCES [dbo].[TestTable2] ([TestColumn2]) ON UPDATE {0};", output));
        }

        [Test]
        public override void CanCreateNamedMultiColumnForeignKeyWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedMultiColumnForeignKeyExpression();
            expression.ForeignKey.ForeignTableSchema = "TestSchema";
            expression.ForeignKey.PrimaryTableSchema = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestSchema].[TestTable1] ADD CONSTRAINT [FK_Test] FOREIGN KEY ([TestColumn1], [TestColumn3]) REFERENCES [TestSchema].[TestTable2] ([TestColumn2], [TestColumn4]);");
        }

        [Test]
        public override void CanCreateNamedMultiColumnForeignKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedMultiColumnForeignKeyExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [dbo].[TestTable1] ADD CONSTRAINT [FK_Test] FOREIGN KEY ([TestColumn1], [TestColumn3]) REFERENCES [dbo].[TestTable2] ([TestColumn2], [TestColumn4]);");
        }

        [Test]
        public override void CanCreateNamedMultiColumnForeignKeyWithDifferentSchemas()
        {
            var expression = GeneratorTestHelper.GetCreateNamedMultiColumnForeignKeyExpression();
            expression.ForeignKey.ForeignTableSchema = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestSchema].[TestTable1] ADD CONSTRAINT [FK_Test] FOREIGN KEY ([TestColumn1], [TestColumn3]) REFERENCES [dbo].[TestTable2] ([TestColumn2], [TestColumn4]);");
        }

        [Test]
        public override void CanCreateNamedMultiColumnPrimaryKeyConstraintWithCustomSchema([Values] CompatibilityMode compatibilityMode)
        {
            var expression = GeneratorTestHelper.GetCreateNamedMultiColumnPrimaryKeyExpression();
            expression.Constraint.SchemaName = "TestSchema";

            Generator.CompatibilityMode = compatibilityMode;
            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestSchema].[TestTable1] ADD CONSTRAINT [TESTPRIMARYKEY] PRIMARY KEY ([TestColumn1], [TestColumn2]);");
        }

        [Test]
        public override void CanCreateNamedMultiColumnPrimaryKeyConstraintWithDefaultSchema([Values] CompatibilityMode compatibilityMode)
        {
            var expression = GeneratorTestHelper.GetCreateNamedMultiColumnPrimaryKeyExpression();

            Generator.CompatibilityMode = compatibilityMode;
            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [dbo].[TestTable1] ADD CONSTRAINT [TESTPRIMARYKEY] PRIMARY KEY ([TestColumn1], [TestColumn2]);");
        }

        [Test]
        public override void CanCreateNamedMultiColumnUniqueConstraintWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedMultiColumnUniqueConstraintExpression();
            expression.Constraint.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestSchema].[TestTable1] ADD CONSTRAINT [TESTUNIQUECONSTRAINT] UNIQUE ([TestColumn1], [TestColumn2]);");
        }

        [Test]
        public virtual void CanCreateNamedMultiColumnUniqueConstraintWithCustomSchemaAndFilter()
        {
            var expression = GeneratorTestHelper.GetCreateNamedMultiColumnUniqueConstraintExpression();
            expression.Constraint.SchemaName = "TestSchema";
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.UniqueConstraintFilter, expression.Constraint.Columns.First() + " IS NOT NULL");

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE UNIQUE INDEX [TESTUNIQUECONSTRAINT] ON [TestSchema].[TestTable1] ([TestColumn1], [TestColumn2]) WHERE TestColumn1 IS NOT NULL;");
        }

        [Test]
        public virtual void CanCreateNamedMultiColumnUniqueConstraintWithCustomSchemaAndFilterAndInclude()
        {
            var expression = GeneratorTestHelper.GetCreateNamedMultiColumnUniqueConstraintExpression();
            expression.Constraint.SchemaName = "TestSchema";
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.UniqueConstraintFilter, expression.Constraint.Columns.First() + " IS NOT NULL");
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.UniqueConstraintIncludesList, new List<IndexIncludeDefinition> { new IndexIncludeDefinition { Name = "TestColumn3" } });

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE UNIQUE INDEX [TESTUNIQUECONSTRAINT] ON [TestSchema].[TestTable1] ([TestColumn1], [TestColumn2]) INCLUDE ([TestColumn3]) WHERE TestColumn1 IS NOT NULL;");
        }

        [Test]
        public virtual void CanCreateNamedMultiColumnUniqueConstraintWithCustomSchemaAndFilterAndIncludeWithNonClusteredIndex()
        {
            var expression = GeneratorTestHelper.GetCreateNamedMultiColumnUniqueConstraintExpression();
            expression.Constraint.SchemaName = "TestSchema";
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.ConstraintType, SqlServerConstraintType.NonClustered);
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.UniqueConstraintFilter, expression.Constraint.Columns.First() + " IS NOT NULL");
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.UniqueConstraintIncludesList, new List<IndexIncludeDefinition> { new IndexIncludeDefinition { Name = "TestColumn3" } });

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE UNIQUE INDEX [TESTUNIQUECONSTRAINT] ON [TestSchema].[TestTable1] ([TestColumn1], [TestColumn2]) INCLUDE ([TestColumn3]) WHERE TestColumn1 IS NOT NULL;");
        }

        [Test]
        public virtual void CannotCreateNamedMultiColumnUniqueConstraintWithCustomSchemaAndFilterAndIncludeWithClusteredIndex()
        {
            var expression = GeneratorTestHelper.GetCreateNamedMultiColumnUniqueConstraintExpression();
            expression.Constraint.SchemaName = "TestSchema";
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.ConstraintType, SqlServerConstraintType.Clustered);
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.UniqueConstraintFilter, expression.Constraint.Columns.First() + " IS NOT NULL");
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.UniqueConstraintIncludesList, new List<IndexIncludeDefinition> { new IndexIncludeDefinition { Name = "TestColumn3" } });

            var ex = Assert.Throws<Exception>(() => Generator.Generate(expression));
            Assert.That(ex, Is.Not.Null);
            Assert.That(ex.Message, Is.EqualTo("Filtered indexes are non-clustered indexes that have the addition of a WHERE clause. SQL Server does not support clustered filtered indexes. Create a non-clustered index with include columns instead to create a non-clustered covering index."));
        }

        [Test]
        public virtual void CanCreateNamedMultiColumnUniqueConstraintWithCustomSchemaAndFilterWithNonClusteredIndex()
        {
            var expression = GeneratorTestHelper.GetCreateNamedMultiColumnUniqueConstraintExpression();
            expression.Constraint.SchemaName = "TestSchema";
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.ConstraintType, SqlServerConstraintType.NonClustered);
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.UniqueConstraintFilter, expression.Constraint.Columns.First() + " IS NOT NULL");

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE UNIQUE INDEX [TESTUNIQUECONSTRAINT] ON [TestSchema].[TestTable1] ([TestColumn1], [TestColumn2]) WHERE TestColumn1 IS NOT NULL;");
        }

        [Test]
        public virtual void CannotCreateNamedMultiColumnUniqueConstraintWithCustomSchemaAndFilterWithClusteredIndex()
        {
            var expression = GeneratorTestHelper.GetCreateNamedMultiColumnUniqueConstraintExpression();
            expression.Constraint.SchemaName = "TestSchema";
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.ConstraintType, SqlServerConstraintType.Clustered);
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.UniqueConstraintFilter, expression.Constraint.Columns.First() + " IS NOT NULL");

            var ex = Assert.Throws<Exception>(() => Generator.Generate(expression));
            Assert.That(ex, Is.Not.Null);
            Assert.That(ex.Message, Is.EqualTo("Filtered indexes are non-clustered indexes that have the addition of a WHERE clause. SQL Server does not support clustered filtered indexes. Create a non-clustered index with include columns instead to create a non-clustered covering index."));
        }

        [Test]
        public override void CanCreateNamedMultiColumnUniqueConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedMultiColumnUniqueConstraintExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [dbo].[TestTable1] ADD CONSTRAINT [TESTUNIQUECONSTRAINT] UNIQUE ([TestColumn1], [TestColumn2]);");
        }

        [Test]
        public virtual void CanCreateNamedMultiColumnUniqueConstraintWithDefaultSchemaAndFilter()
        {
            var expression = GeneratorTestHelper.GetCreateNamedMultiColumnUniqueConstraintExpression();
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.UniqueConstraintFilter, expression.Constraint.Columns.First() + " IS NOT NULL");

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE UNIQUE INDEX [TESTUNIQUECONSTRAINT] ON [dbo].[TestTable1] ([TestColumn1], [TestColumn2]) WHERE TestColumn1 IS NOT NULL;");
        }

        [Test]
        public virtual void CanCreateNamedMultiColumnUniqueConstraintWithDefaultSchemaAndFilterAndInclude()
        {
            var expression = GeneratorTestHelper.GetCreateNamedMultiColumnUniqueConstraintExpression();
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.UniqueConstraintFilter, expression.Constraint.Columns.First() + " IS NOT NULL");
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.UniqueConstraintIncludesList, new List<IndexIncludeDefinition> { new IndexIncludeDefinition { Name = "TestColumn3" } });

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE UNIQUE INDEX [TESTUNIQUECONSTRAINT] ON [dbo].[TestTable1] ([TestColumn1], [TestColumn2]) INCLUDE ([TestColumn3]) WHERE TestColumn1 IS NOT NULL;");
        }

        [Test]
        public virtual void CanCreateNamedMultiColumnUniqueConstraintWithDefaultSchemaAndFilterAndIncludeWithNonClusteredIndex()
        {
            var expression = GeneratorTestHelper.GetCreateNamedMultiColumnUniqueConstraintExpression();
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.ConstraintType, SqlServerConstraintType.NonClustered);
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.UniqueConstraintFilter, expression.Constraint.Columns.First() + " IS NOT NULL");
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.UniqueConstraintIncludesList, new List<IndexIncludeDefinition> { new IndexIncludeDefinition { Name = "TestColumn3" } });

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE UNIQUE INDEX [TESTUNIQUECONSTRAINT] ON [dbo].[TestTable1] ([TestColumn1], [TestColumn2]) INCLUDE ([TestColumn3]) WHERE TestColumn1 IS NOT NULL;");
        }

        [Test]
        public virtual void CannotCreateNamedMultiColumnUniqueConstraintWithDefaultSchemaAndFilterAndIncludeWithClusteredIndex()
        {
            var expression = GeneratorTestHelper.GetCreateNamedMultiColumnUniqueConstraintExpression();
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.ConstraintType, SqlServerConstraintType.Clustered);
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.UniqueConstraintFilter, expression.Constraint.Columns.First() + " IS NOT NULL");
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.UniqueConstraintIncludesList, new List<IndexIncludeDefinition> { new IndexIncludeDefinition { Name = "TestColumn3" } });

            var ex = Assert.Throws<Exception>(() => Generator.Generate(expression));
            Assert.That(ex, Is.Not.Null);
            Assert.That(ex.Message, Is.EqualTo("Filtered indexes are non-clustered indexes that have the addition of a WHERE clause. SQL Server does not support clustered filtered indexes. Create a non-clustered index with include columns instead to create a non-clustered covering index."));
        }

        [Test]
        public virtual void CanCreateNamedMultiColumnUniqueConstraintWithDefaultSchemaAndFilterWithNonClusteredIndex()
        {
            var expression = GeneratorTestHelper.GetCreateNamedMultiColumnUniqueConstraintExpression();
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.ConstraintType, SqlServerConstraintType.NonClustered);
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.UniqueConstraintFilter, expression.Constraint.Columns.First() + " IS NOT NULL");

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE UNIQUE INDEX [TESTUNIQUECONSTRAINT] ON [dbo].[TestTable1] ([TestColumn1], [TestColumn2]) WHERE TestColumn1 IS NOT NULL;");
        }

        [Test]
        public virtual void CannotCreateNamedMultiColumnUniqueConstraintWithDefaultSchemaAndFilterWithClusteredIndex()
        {
            var expression = GeneratorTestHelper.GetCreateNamedMultiColumnUniqueConstraintExpression();
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.ConstraintType, SqlServerConstraintType.Clustered);
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.UniqueConstraintFilter, expression.Constraint.Columns.First() + " IS NOT NULL");

            var ex = Assert.Throws<Exception>(() => Generator.Generate(expression));
            Assert.That(ex, Is.Not.Null);
            Assert.That(ex.Message, Is.EqualTo("Filtered indexes are non-clustered indexes that have the addition of a WHERE clause. SQL Server does not support clustered filtered indexes. Create a non-clustered index with include columns instead to create a non-clustered covering index."));
        }

        [Test]
        public override void CanCreateNamedPrimaryKeyConstraintWithCustomSchema([Values] CompatibilityMode compatibilityMode)
        {
            var expression = GeneratorTestHelper.GetCreateNamedPrimaryKeyExpression();
            expression.Constraint.SchemaName = "TestSchema";

            Generator.CompatibilityMode = compatibilityMode;
            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestSchema].[TestTable1] ADD CONSTRAINT [TESTPRIMARYKEY] PRIMARY KEY ([TestColumn1]);");
        }

        [Test]
        public override void CanCreateNamedPrimaryKeyConstraintWithDefaultSchema([Values] CompatibilityMode compatibilityMode)
        {
            var expression = GeneratorTestHelper.GetCreateNamedPrimaryKeyExpression();

            Generator.CompatibilityMode = compatibilityMode;
            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [dbo].[TestTable1] ADD CONSTRAINT [TESTPRIMARYKEY] PRIMARY KEY ([TestColumn1]);");
        }

        [Test]
        public override void CanCreateNamedUniqueConstraintWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedUniqueConstraintExpression();
            expression.Constraint.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestSchema].[TestTable1] ADD CONSTRAINT [TESTUNIQUECONSTRAINT] UNIQUE ([TestColumn1]);");
        }

        [Test]
        public override void CanCreateNamedUniqueConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedUniqueConstraintExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [dbo].[TestTable1] ADD CONSTRAINT [TESTUNIQUECONSTRAINT] UNIQUE ([TestColumn1]);");
        }

        [Test]
        public override void CanCreatePrimaryKeyConstraintWithCustomSchema([Values] CompatibilityMode compatibilityMode)
        {
            var expression = GeneratorTestHelper.GetCreatePrimaryKeyExpression();
            expression.Constraint.SchemaName = "TestSchema";

            Generator.CompatibilityMode = compatibilityMode;
            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestSchema].[TestTable1] ADD CONSTRAINT [PK_TestTable1_TestColumn1] PRIMARY KEY ([TestColumn1]);");
        }

        [Test]
        public override void CanCreatePrimaryKeyConstraintWithDefaultSchema([Values] CompatibilityMode compatibilityMode)
        {
            var expression = GeneratorTestHelper.GetCreatePrimaryKeyExpression();

            Generator.CompatibilityMode = compatibilityMode;
            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [dbo].[TestTable1] ADD CONSTRAINT [PK_TestTable1_TestColumn1] PRIMARY KEY ([TestColumn1]);");
        }

        [Test]
        public override void CanCreateUniqueConstraintWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueConstraintExpression();
            expression.Constraint.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestSchema].[TestTable1] ADD CONSTRAINT [UC_TestTable1_TestColumn1] UNIQUE ([TestColumn1]);");
        }

        [Test]
        public virtual void CanCreateUniqueConstraintWithCustomSchemaAndFilter()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueConstraintExpression();
            expression.Constraint.SchemaName = "TestSchema";
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.UniqueConstraintFilter, expression.Constraint.Columns.First() + " IS NOT NULL");

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE UNIQUE INDEX [UC_TestTable1_TestColumn1] ON [TestSchema].[TestTable1] ([TestColumn1]) WHERE TestColumn1 IS NOT NULL;");
        }

        [Test]
        public virtual void CanCreateUniqueConstraintWithDefaultSchemaAndFilterAndInclude()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueConstraintExpression();
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.UniqueConstraintFilter, expression.Constraint.Columns.First() + " IS NOT NULL");
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.UniqueConstraintIncludesList, new List<IndexIncludeDefinition> { new IndexIncludeDefinition { Name = "TestColumn3" } });

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE UNIQUE INDEX [UC_TestTable1_TestColumn1] ON [dbo].[TestTable1] ([TestColumn1]) INCLUDE ([TestColumn3]) WHERE TestColumn1 IS NOT NULL;");
        }

        [Test]
        public virtual void CanCreateUniqueConstraintWithDefaultSchemaAndFilterAndIncludeWithNonClusteredIndex()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueConstraintExpression();
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.ConstraintType, SqlServerConstraintType.NonClustered);
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.UniqueConstraintFilter, expression.Constraint.Columns.First() + " IS NOT NULL");
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.UniqueConstraintIncludesList, new List<IndexIncludeDefinition> { new IndexIncludeDefinition { Name = "TestColumn3" } });

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE UNIQUE INDEX [UC_TestTable1_TestColumn1] ON [dbo].[TestTable1] ([TestColumn1]) INCLUDE ([TestColumn3]) WHERE TestColumn1 IS NOT NULL;");
        }

        [Test]
        public virtual void CannotCreateUniqueConstraintWithDefaultSchemaAndFilterAndIncludeWithClusteredIndex()
        {
            var expression = GeneratorTestHelper.GetCreateNamedMultiColumnUniqueConstraintExpression();
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.ConstraintType, SqlServerConstraintType.Clustered);
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.UniqueConstraintFilter, expression.Constraint.Columns.First() + " IS NOT NULL");
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.UniqueConstraintIncludesList, new List<IndexIncludeDefinition> { new IndexIncludeDefinition { Name = "TestColumn3" } });

            var ex = Assert.Throws<Exception>(() => Generator.Generate(expression));
            Assert.That(ex, Is.Not.Null);
            Assert.That(ex.Message, Is.EqualTo("Filtered indexes are non-clustered indexes that have the addition of a WHERE clause. SQL Server does not support clustered filtered indexes. Create a non-clustered index with include columns instead to create a non-clustered covering index."));
        }


        [Test]
        public virtual void CanCreateUniqueConstraintWithCustomSchemaAndFilterWithNonClusteredIndex()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueConstraintExpression();
            expression.Constraint.SchemaName = "TestSchema";
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.ConstraintType, SqlServerConstraintType.NonClustered);
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.UniqueConstraintFilter, expression.Constraint.Columns.First() + " IS NOT NULL");

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE UNIQUE INDEX [UC_TestTable1_TestColumn1] ON [TestSchema].[TestTable1] ([TestColumn1]) WHERE TestColumn1 IS NOT NULL;");
        }

        [Test]
        public virtual void CannotCreateUniqueConstraintWithCustomSchemaAndFilterWithClusteredIndex()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueConstraintExpression();
            expression.Constraint.SchemaName = "TestSchema";
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.ConstraintType, SqlServerConstraintType.Clustered);
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.UniqueConstraintFilter, expression.Constraint.Columns.First() + " IS NOT NULL;");

            var ex = Assert.Throws<Exception>(() => Generator.Generate(expression));
            Assert.That(ex, Is.Not.Null);
            Assert.That(ex.Message, Is.EqualTo("Filtered indexes are non-clustered indexes that have the addition of a WHERE clause. SQL Server does not support clustered filtered indexes. Create a non-clustered index with include columns instead to create a non-clustered covering index."));
        }

        [Test]
        public override void CanCreateUniqueConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueConstraintExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [dbo].[TestTable1] ADD CONSTRAINT [UC_TestTable1_TestColumn1] UNIQUE ([TestColumn1]);");
        }

        [Test]
        public virtual void CanCreateUniqueConstraintWithDefaultSchemaAndFilter()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueConstraintExpression();
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.UniqueConstraintFilter, expression.Constraint.Columns.First() + " IS NOT NULL");

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE UNIQUE INDEX [UC_TestTable1_TestColumn1] ON [dbo].[TestTable1] ([TestColumn1]) WHERE TestColumn1 IS NOT NULL;");
        }

        [Test]
        public virtual void CanCreateUniqueConstraintWithDefaultSchemaAndFilterWithNonClusteredIndex()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueConstraintExpression();
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.ConstraintType, SqlServerConstraintType.NonClustered);
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.UniqueConstraintFilter, expression.Constraint.Columns.First() + " IS NOT NULL");

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE UNIQUE INDEX [UC_TestTable1_TestColumn1] ON [dbo].[TestTable1] ([TestColumn1]) WHERE TestColumn1 IS NOT NULL;");
        }

        [Test]
        public virtual void CannotCreateUniqueConstraintWithDefaultSchemaAndFilterWithClusteredIndex()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueConstraintExpression();
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.ConstraintType, SqlServerConstraintType.Clustered);
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.UniqueConstraintFilter, expression.Constraint.Columns.First() + " IS NOT NULL");

            var ex = Assert.Throws<Exception>(() => Generator.Generate(expression));
            Assert.That(ex, Is.Not.Null);
            Assert.That(ex.Message, Is.EqualTo("Filtered indexes are non-clustered indexes that have the addition of a WHERE clause. SQL Server does not support clustered filtered indexes. Create a non-clustered index with include columns instead to create a non-clustered covering index."));
        }

        [Test]
        public void CanCreatePrimaryKeyWithOnlineOn()
        {
            var expression = GeneratorTestHelper.GetCreatePrimaryKeyExpression();
            new CreateConstraintExpressionBuilder(expression).Online();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [dbo].[TestTable1] ADD CONSTRAINT [PK_TestTable1_TestColumn1] PRIMARY KEY ([TestColumn1]) WITH (ONLINE=ON);");
        }

        [Test]
        public void CanCreatePrimaryKeyWithOnlineOff()
        {
            var expression = GeneratorTestHelper.GetCreatePrimaryKeyExpression();
            new CreateConstraintExpressionBuilder(expression).Online(false);

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [dbo].[TestTable1] ADD CONSTRAINT [PK_TestTable1_TestColumn1] PRIMARY KEY ([TestColumn1]) WITH (ONLINE=OFF);");
        }

        [Test]
        public override void CanDropForeignKeyWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteForeignKeyExpression();
            expression.ForeignKey.ForeignTableSchema = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestSchema].[TestTable1] DROP CONSTRAINT [FK_Test];");
        }

        [Test]
        public override void CanDropForeignKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteForeignKeyExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [dbo].[TestTable1] DROP CONSTRAINT [FK_Test];");
        }

        [Test]
        public override void CanDropPrimaryKeyConstraintWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetDeletePrimaryKeyExpression();
            expression.Constraint.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestSchema].[TestTable1] DROP CONSTRAINT [TESTPRIMARYKEY];");
        }

        [Test]
        public override void CanDropPrimaryKeyConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeletePrimaryKeyExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [dbo].[TestTable1] DROP CONSTRAINT [TESTPRIMARYKEY];");
        }

        [Test]
        public void CanDropPrimaryKeyWithOnlineOn()
        {
            var expression = GeneratorTestHelper.GetDeletePrimaryKeyExpression();
            new DeleteConstraintExpressionBuilder(expression).Online();
            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [dbo].[TestTable1] DROP CONSTRAINT [TESTPRIMARYKEY] WITH (ONLINE=ON);");
        }

        [Test]
        public void CanDropPrimaryKeyWithOnlineOff()
        {
            var expression = GeneratorTestHelper.GetDeletePrimaryKeyExpression();
            new DeleteConstraintExpressionBuilder(expression).Online(false);
            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [dbo].[TestTable1] DROP CONSTRAINT [TESTPRIMARYKEY] WITH (ONLINE=OFF);");
        }

        [Test]
        public override void CanDropUniqueConstraintWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteUniqueConstraintExpression();
            expression.Constraint.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestSchema].[TestTable1] DROP CONSTRAINT [TESTUNIQUECONSTRAINT];");
        }

        [Test]
        public override void CanDropUniqueConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteUniqueConstraintExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [dbo].[TestTable1] DROP CONSTRAINT [TESTUNIQUECONSTRAINT];");
        }


    }
}
