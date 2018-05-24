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

using FluentMigrator.Runner.Generators.SqlAnywhere;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Generators.SqlAnywhere
{
    [TestFixture]
    [Category("SqlAnywhere")]
    [Category("SqlAnywhere16")]
    public class SqlAnywhere16DataTests : BaseDataTests
    {
        protected SqlAnywhere16Generator Generator;

        [SetUp]
        public void Setup()
        {
            Generator = new SqlAnywhere16Generator();
        }

        [Test]
        [Category("Delete")]
        public override void CanDeleteDataForAllRowsWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteDataAllRowsExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("DELETE FROM [TestSchema].[TestTable1] WHERE 1 = 1");
        }

        [Test]
        [Category("Delete")]
        public override void CanDeleteDataForAllRowsWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteDataAllRowsExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("DELETE FROM [dbo].[TestTable1] WHERE 1 = 1");
        }

        [Test]
        [Category("Delete")]
        public override void CanDeleteDataForMultipleRowsWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteDataMultipleRowsExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("DELETE FROM [TestSchema].[TestTable1] WHERE [Name] = N'Just''in' AND [Website] IS NULL; DELETE FROM [TestSchema].[TestTable1] WHERE [Website] = N'github.com'");
        }

        [Test]
        [Category("Delete")]
        public override void CanDeleteDataForMultipleRowsWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteDataMultipleRowsExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("DELETE FROM [dbo].[TestTable1] WHERE [Name] = N'Just''in' AND [Website] IS NULL; DELETE FROM [dbo].[TestTable1] WHERE [Website] = N'github.com'");
        }

        [Test]
        [Category("Delete")]
        public override void CanDeleteDataWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteDataExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("DELETE FROM [TestSchema].[TestTable1] WHERE [Name] = N'Just''in' AND [Website] IS NULL");
        }

        [Test]
        [Category("Delete")]
        public override void CanDeleteDataWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteDataExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("DELETE FROM [dbo].[TestTable1] WHERE [Name] = N'Just''in' AND [Website] IS NULL");
        }

        public override void CanDeleteDataWithDbNullCriteria()
        {
            var expression = GeneratorTestHelper.GetDeleteDataExpressionWithDbNullValue();
            var result = Generator.Generate(expression);
            result.ShouldBe("DELETE FROM [dbo].[TestTable1] WHERE [Name] = N'Just''in' AND [Website] IS NULL");
        }

        [Test]
        [Category("Insert")]
        public override void CanInsertDataWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetInsertDataExpression();
            expression.SchemaName = "TestSchema";

            var expected = "INSERT INTO [TestSchema].[TestTable1] ([Id], [Name], [Website]) VALUES (1, N'Just''in', N'codethinked.com');";
            expected += @" INSERT INTO [TestSchema].[TestTable1] ([Id], [Name], [Website]) VALUES (2, N'Na\te', N'kohari.org')";

            var result = Generator.Generate(expression);
            result.ShouldBe(expected);
        }

        [Test]
        [Category("Insert")]
        public override void CanInsertDataWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetInsertDataExpression();

            var expected = "INSERT INTO [dbo].[TestTable1] ([Id], [Name], [Website]) VALUES (1, N'Just''in', N'codethinked.com');";
            expected += @" INSERT INTO [dbo].[TestTable1] ([Id], [Name], [Website]) VALUES (2, N'Na\te', N'kohari.org')";

            var result = Generator.Generate(expression);
            result.ShouldBe(expected);
        }

        [Test]
        [Category("Insert")]
        public override void CanInsertGuidDataWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetInsertGUIDExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe(string.Format("INSERT INTO [TestSchema].[TestTable1] ([guid]) VALUES ('{0}')", GeneratorTestHelper.TestGuid.ToString()));
        }

        [Test]
        [Category("Insert")]
        public override void CanInsertGuidDataWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetInsertGUIDExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe(string.Format("INSERT INTO [dbo].[TestTable1] ([guid]) VALUES ('{0}')", GeneratorTestHelper.TestGuid.ToString()));
        }

        [Test]
        [Category("Update")]
        public override void CanUpdateDataForAllDataWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetUpdateDataExpressionWithAllRows();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("UPDATE [TestSchema].[TestTable1] SET [Name] = N'Just''in', [Age] = 25 WHERE 1 = 1");
        }

        [Test]
        [Category("Update")]
        public override void CanUpdateDataForAllDataWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetUpdateDataExpressionWithAllRows();

            var result = Generator.Generate(expression);
            result.ShouldBe("UPDATE [dbo].[TestTable1] SET [Name] = N'Just''in', [Age] = 25 WHERE 1 = 1");
        }

        [Test]
        [Category("Update")]
        public override void CanUpdateDataWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetUpdateDataExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("UPDATE [TestSchema].[TestTable1] SET [Name] = N'Just''in', [Age] = 25 WHERE [Id] = 9 AND [Homepage] IS NULL");
        }

        [Test]
        [Category("Update")]
        public override void CanUpdateDataWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetUpdateDataExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("UPDATE [dbo].[TestTable1] SET [Name] = N'Just''in', [Age] = 25 WHERE [Id] = 9 AND [Homepage] IS NULL");
        }

        [Test]
        [Category("Update")]
        public override void CanUpdateDataWithDbNullCriteria()
        {
            var expression = GeneratorTestHelper.GetUpdateDataExpressionWithDbNullValue();

            var result = Generator.Generate(expression);
            result.ShouldBe("UPDATE [dbo].[TestTable1] SET [Name] = N'Just''in', [Age] = 25 WHERE [Id] = 9 AND [Homepage] IS NULL");
        }
    }
}
