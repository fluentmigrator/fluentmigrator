﻿using System;
using FluentMigrator.Runner.Extensions;
using FluentMigrator.Runner.Generators.SqlServer;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.SqlServer2005
{
    [TestFixture]
    public class SqlServer2005DataTests
    {
        protected SqlServer2005Generator generator;

        [SetUp]
        public void Setup()
        {
            generator = new SqlServer2005Generator();
        }

        [Test]
        public void CanDeleteDataAllRowsWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteDataAllRowsExpression();

            var sql = generator.Generate(expression);

            sql.ShouldBe("DELETE FROM [dbo].[TestTable1] WHERE 1 = 1");
        }

        [Test]
        public void CanDeleteDataMulitpleRowsWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteDataMultipleRowsExpression();

            var sql = generator.Generate(expression);

            sql.ShouldBe("DELETE FROM [dbo].[TestTable1] WHERE [Name] = 'Just''in' AND [Website] IS NULL; DELETE FROM [dbo].[TestTable1] WHERE [Website] = 'github.com'");
        }

        [Test]
        public void CanDeleteDataWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteDataExpression();
            expression.SchemaName = "TestSchema";
            var sql = generator.Generate(expression);

            sql.ShouldBe("DELETE FROM [TestSchema].[TestTable1] WHERE [Name] = 'Just''in' AND [Website] IS NULL");
        }

        [Test]
        public void CanDeleteDataWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteDataExpression();

            var sql = generator.Generate(expression);

            sql.ShouldBe("DELETE FROM [dbo].[TestTable1] WHERE [Name] = 'Just''in' AND [Website] IS NULL");
        }

        [Test]
        public void CanInsertDataWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetInsertDataExpression();
            expression.SchemaName = "TestSchema";
            var sql = generator.Generate(expression);

            var expected = "INSERT INTO [TestSchema].[TestTable1] ([Id], [Name], [Website]) VALUES (1, 'Just''in', 'codethinked.com');";
            expected += @" INSERT INTO [TestSchema].[TestTable1] ([Id], [Name], [Website]) VALUES (2, 'Na\te', 'kohari.org')";

            sql.ShouldBe(expected);
        }

        [Test]
        public void CanInsertDataWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetInsertDataExpression();
            var sql = generator.Generate(expression);

            var expected = "INSERT INTO [dbo].[TestTable1] ([Id], [Name], [Website]) VALUES (1, 'Just''in', 'codethinked.com');";
            expected += @" INSERT INTO [dbo].[TestTable1] ([Id], [Name], [Website]) VALUES (2, 'Na\te', 'kohari.org')";

            sql.ShouldBe(expected);
        }

        [Test]
        public void CanInsertGuidDataWithCustomSchema()
        {

            var expression = GeneratorTestHelper.GetInsertGUIDExpression();
            expression.SchemaName = "TestSchema";
            var sql = generator.Generate(expression);

            var expected = String.Format("INSERT INTO [TestSchema].[TestTable1] ([guid]) VALUES ('{0}')", GeneratorTestHelper.TestGuid.ToString());

            sql.ShouldBe(expected);
        }

        [Test]
        public void CanInsertGuidDataWithDefaultSchema()
        {

            var expression = GeneratorTestHelper.GetInsertGUIDExpression();

            var sql = generator.Generate(expression);

            var expected = String.Format("INSERT INTO [dbo].[TestTable1] ([guid]) VALUES ('{0}')", GeneratorTestHelper.TestGuid.ToString());

            sql.ShouldBe(expected);
        }

        [Test]
        public void CanUpdateDataForAllRows()
        {
            var expression = GeneratorTestHelper.GetUpdateDataExpressionWithAllRows();

            var sql = generator.Generate(expression);
            sql.ShouldBe("UPDATE [dbo].[TestTable1] SET [Name] = 'Just''in', [Age] = 25 WHERE 1 = 1");
        }

        [Test]
        public void CanUpdateDataWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetUpdateDataExpression();
            expression.SchemaName = "TestSchema";
            var sql = generator.Generate(expression);
            sql.ShouldBe("UPDATE [TestSchema].[TestTable1] SET [Name] = 'Just''in', [Age] = 25 WHERE [Id] = 9 AND [Homepage] IS NULL");
        }

        [Test]
        public void CanUpdateDataWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetUpdateDataExpression();

            var sql = generator.Generate(expression);
            sql.ShouldBe("UPDATE [dbo].[TestTable1] SET [Name] = 'Just''in', [Age] = 25 WHERE [Id] = 9 AND [Homepage] IS NULL");
        }

        [Test]
        public void CanInsertDataWithIdentityInsert()
        {
            var expression = GeneratorTestHelper.GetInsertDataExpression();
            expression.AdditionalFeatures.Add(SqlServerExtensions.IdentityInsert, true);
            var sql = generator.Generate(expression);

            var expected = "SET IDENTITY_INSERT [dbo].[TestTable1] ON;";
            expected += " INSERT INTO [dbo].[TestTable1] ([Id], [Name], [Website]) VALUES (1, 'Just''in', 'codethinked.com');";
            expected += @" INSERT INTO [dbo].[TestTable1] ([Id], [Name], [Website]) VALUES (2, 'Na\te', 'kohari.org');";
            expected += " SET IDENTITY_INSERT [dbo].[TestTable1] OFF";

            sql.ShouldBe(expected);
        }

        [Test]
        public void CanInsertDataWithIdentityInsertInStrictMode()
        {
            var expression = GeneratorTestHelper.GetInsertDataExpression();
            expression.AdditionalFeatures.Add(SqlServerExtensions.IdentityInsert, true);
            generator.compatabilityMode = Runner.CompatabilityMode.STRICT;
            var sql = generator.Generate(expression);

            var expected = "SET IDENTITY_INSERT [dbo].[TestTable1] ON;";
            expected += " INSERT INTO [dbo].[TestTable1] ([Id], [Name], [Website]) VALUES (1, 'Just''in', 'codethinked.com');";
            expected += @" INSERT INTO [dbo].[TestTable1] ([Id], [Name], [Website]) VALUES (2, 'Na\te', 'kohari.org');";
            expected += " SET IDENTITY_INSERT [dbo].[TestTable1] OFF";

            sql.ShouldBe(expected);
        }
    }
}
