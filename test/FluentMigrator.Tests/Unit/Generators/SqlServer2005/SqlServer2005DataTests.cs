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

using System.Collections.Generic;

using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators.SqlServer;
using FluentMigrator.SqlServer;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Generators.SqlServer2005
{
    [TestFixture]
    [Category("SqlServer2005")]
    public class SqlServer2005DataTests : BaseDataTests
    {
        protected SqlServer2005Generator Generator;

        [SetUp]
        public void Setup()
        {
            Generator = new SqlServer2005Generator();
        }

        [Test]
        public override void CanDeleteDataForAllRowsWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteDataAllRowsExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("DELETE FROM [TestSchema].[TestTable1] WHERE 1 = 1;");
        }

        [Test]
        public override void CanDeleteDataForAllRowsWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteDataAllRowsExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("DELETE FROM [dbo].[TestTable1] WHERE 1 = 1;");
        }

        [Test]
        public override void CanDeleteDataForMultipleRowsWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteDataMultipleRowsExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("DELETE FROM [TestSchema].[TestTable1] WHERE [Name] = N'Just''in' AND [Website] IS NULL;DELETE FROM [TestSchema].[TestTable1] WHERE [Website] = N'github.com';");
        }

        [Test]
        public override void CanDeleteDataForMultipleRowsWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteDataMultipleRowsExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("DELETE FROM [dbo].[TestTable1] WHERE [Name] = N'Just''in' AND [Website] IS NULL;DELETE FROM [dbo].[TestTable1] WHERE [Website] = N'github.com';");
        }

        [Test]
        public override void CanDeleteDataWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteDataExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("DELETE FROM [TestSchema].[TestTable1] WHERE [Name] = N'Just''in' AND [Website] IS NULL;");
        }

        [Test]
        public override void CanDeleteDataWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteDataExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("DELETE FROM [dbo].[TestTable1] WHERE [Name] = N'Just''in' AND [Website] IS NULL;");
        }

        [Test]
        public override void CanDeleteDataWithDbNullCriteria()
        {
            var expression = GeneratorTestHelper.GetDeleteDataExpressionWithDbNullValue();
            var result = Generator.Generate(expression);
            result.ShouldBe("DELETE FROM [dbo].[TestTable1] WHERE [Name] = N'Just''in' AND [Website] IS NULL;");
        }

        [Test]
        public override void CanInsertDataWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetInsertDataExpression();
            expression.SchemaName = "TestSchema";

            var expected = "INSERT INTO [TestSchema].[TestTable1] ([Id], [Name], [Website]) VALUES (1, N'Just''in', N'codethinked.com');";
            expected += @"INSERT INTO [TestSchema].[TestTable1] ([Id], [Name], [Website]) VALUES (2, N'Na\te', N'kohari.org');";

            var result = Generator.Generate(expression);
            result.ShouldBe(expected);
        }

        [Test]
        public override void CanInsertDataWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetInsertDataExpression();

            var expected = "INSERT INTO [dbo].[TestTable1] ([Id], [Name], [Website]) VALUES (1, N'Just''in', N'codethinked.com');";
            expected += @"INSERT INTO [dbo].[TestTable1] ([Id], [Name], [Website]) VALUES (2, N'Na\te', N'kohari.org');";

            var result = Generator.Generate(expression);
            result.ShouldBe(expected);
        }

        [Test]
        public override void CanInsertGuidDataWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetInsertGUIDExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe(string.Format("INSERT INTO [TestSchema].[TestTable1] ([guid]) VALUES ('{0}');", GeneratorTestHelper.TestGuid.ToString()));
        }

        [Test]
        public override void CanInsertGuidDataWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetInsertGUIDExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe(string.Format("INSERT INTO [dbo].[TestTable1] ([guid]) VALUES ('{0}');", GeneratorTestHelper.TestGuid.ToString()));
        }

        [Test]
        public override void CanUpdateDataForAllDataWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetUpdateDataExpressionWithAllRows();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("UPDATE [TestSchema].[TestTable1] SET [Name] = N'Just''in', [Age] = 25 WHERE 1 = 1;");
        }

        [Test]
        public override void CanUpdateDataForAllDataWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetUpdateDataExpressionWithAllRows();

            var result = Generator.Generate(expression);
            result.ShouldBe("UPDATE [dbo].[TestTable1] SET [Name] = N'Just''in', [Age] = 25 WHERE 1 = 1;");
        }

        [Test]
        public override void CanUpdateDataWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetUpdateDataExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("UPDATE [TestSchema].[TestTable1] SET [Name] = N'Just''in', [Age] = 25 WHERE [Id] = 9 AND [Homepage] IS NULL;");
        }

        [Test]
        public override void CanUpdateDataWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetUpdateDataExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("UPDATE [dbo].[TestTable1] SET [Name] = N'Just''in', [Age] = 25 WHERE [Id] = 9 AND [Homepage] IS NULL;");
        }

        [Test]
        public void CanInsertDataWithIdentityInsert()
        {
            var expression = GeneratorTestHelper.GetInsertDataExpression();
            expression.AdditionalFeatures.Add(SqlServerExtensions.IdentityInsert, true);

            var expected = "SET IDENTITY_INSERT [dbo].[TestTable1] ON;";
            expected += "INSERT INTO [dbo].[TestTable1] ([Id], [Name], [Website]) VALUES (1, N'Just''in', N'codethinked.com');";
            expected += @"INSERT INTO [dbo].[TestTable1] ([Id], [Name], [Website]) VALUES (2, N'Na\te', N'kohari.org');";
            expected += "SET IDENTITY_INSERT [dbo].[TestTable1] OFF;";

            var result = Generator.Generate(expression);
            result.ShouldBe(expected);
        }

        [Test]
        public void CanInsertDataWithIdentityInsertInStrictMode()
        {
            var expression = GeneratorTestHelper.GetInsertDataExpression();
            expression.AdditionalFeatures.Add(SqlServerExtensions.IdentityInsert, true);
            Generator.CompatibilityMode = Runner.CompatibilityMode.STRICT;

            var expected = "SET IDENTITY_INSERT [dbo].[TestTable1] ON;";
            expected += "INSERT INTO [dbo].[TestTable1] ([Id], [Name], [Website]) VALUES (1, N'Just''in', N'codethinked.com');";
            expected += @"INSERT INTO [dbo].[TestTable1] ([Id], [Name], [Website]) VALUES (2, N'Na\te', N'kohari.org');";
            expected += "SET IDENTITY_INSERT [dbo].[TestTable1] OFF;";

            var result = Generator.Generate(expression);
            result.ShouldBe(expected);
        }

        [Test]
        public override void CanUpdateDataWithDbNullCriteria()
        {
            var expression = GeneratorTestHelper.GetUpdateDataExpressionWithDbNullValue();

            var result = Generator.Generate(expression);
            result.ShouldBe("UPDATE [dbo].[TestTable1] SET [Name] = N'Just''in', [Age] = 25 WHERE [Id] = 9 AND [Homepage] IS NULL;");
        }

        [Test]
        public void CanDeleteDataWithRawSqlSubquery()
        {
            // Test the backward compatibility case: RawSql with a subquery should add "= " operator
            var expression = new DeleteDataExpression
            {
                TableName = "RolePermissions"
            };
            expression.Rows.Add(new DeletionDataDefinition
            {
                new KeyValuePair<string, object>("PermissionId", RawSql.Insert("(SELECT [Id] FROM [dbo].[Permissions] WHERE [Name] = 'Foo')"))
            });

            var result = Generator.Generate(expression);
            result.ShouldBe("DELETE FROM [dbo].[RolePermissions] WHERE [PermissionId] = (SELECT [Id] FROM [dbo].[Permissions] WHERE [Name] = 'Foo');");
        }

        [Test]
        public void CanDeleteDataWithRawSqlExplicitOperator()
        {
            // Test the new syntax where operator is explicitly included in RawSql
            var expression = new DeleteDataExpression
            {
                TableName = "TestTable"
            };
            expression.Rows.Add(new DeletionDataDefinition
            {
                new KeyValuePair<string, object>("Status", RawSql.Insert("= 3"))
            });

            var result = Generator.Generate(expression);
            result.ShouldBe("DELETE FROM [dbo].[TestTable] WHERE [Status] = 3;");
        }

        [Test]
        public void CanDeleteDataWithRawSqlIsNull()
        {
            // Test RawSql with IS NULL operator
            var expression = new DeleteDataExpression
            {
                TableName = "TestTable"
            };
            expression.Rows.Add(new DeletionDataDefinition
            {
                new KeyValuePair<string, object>("Status", RawSql.Insert("IS NULL"))
            });

            var result = Generator.Generate(expression);
            result.ShouldBe("DELETE FROM [dbo].[TestTable] WHERE [Status] IS NULL;");
        }

        [Test]
        public void CanDeleteDataWithRawSqlInClause()
        {
            // Test RawSql with IN clause with space
            var expression = new DeleteDataExpression
            {
                TableName = "TestTable"
            };
            expression.Rows.Add(new DeletionDataDefinition
            {
                new KeyValuePair<string, object>("Status", RawSql.Insert("IN (1, 2, 3)"))
            });

            var result = Generator.Generate(expression);
            result.ShouldBe("DELETE FROM [dbo].[TestTable] WHERE [Status] IN (1, 2, 3);");
        }

        [Test]
        public void CanDeleteDataWithRawSqlInClauseNoSpace()
        {
            // Test RawSql with IN clause without space after IN
            var expression = new DeleteDataExpression
            {
                TableName = "TestTable"
            };
            expression.Rows.Add(new DeletionDataDefinition
            {
                new KeyValuePair<string, object>("Status", RawSql.Insert("IN(1, 2, 3)"))
            });

            var result = Generator.Generate(expression);
            result.ShouldBe("DELETE FROM [dbo].[TestTable] WHERE [Status] IN(1, 2, 3);");
        }

        [Test]
        public void CanDeleteDataWithRawSqlFullWhereClause()
        {
            // Test RawSql as full WHERE clause (empty key)
            var expression = new DeleteDataExpression
            {
                TableName = "TestTable"
            };
            expression.Rows.Add(new DeletionDataDefinition
            {
                new KeyValuePair<string, object>("", RawSql.Insert("Status = 1 AND Active = 0"))
            });

            var result = Generator.Generate(expression);
            result.ShouldBe("DELETE FROM [dbo].[TestTable] WHERE Status = 1 AND Active = 0;");
        }

        [Test]
        public void CanUpdateDataWithRawSqlSubquery()
        {
            // Test backward compatibility for UPDATE with RawSql subquery in WHERE clause
            var expression = new UpdateDataExpression
            {
                TableName = "TestTable"
            };
            expression.Set = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("Name", "Updated")
            };
            expression.Where = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("Id", RawSql.Insert("(SELECT MAX(Id) FROM OtherTable)"))
            };

            var result = Generator.Generate(expression);
            result.ShouldBe("UPDATE [dbo].[TestTable] SET [Name] = N'Updated' WHERE [Id] = (SELECT MAX(Id) FROM OtherTable);");
        }

        [Test]
        public void CanUpdateDataWithRawSqlLikeOperator()
        {
            // Test RawSql with LIKE operator
            var expression = new UpdateDataExpression
            {
                TableName = "TestTable"
            };
            expression.Set = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("Name", "Updated")
            };
            expression.Where = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("Name", RawSql.Insert("LIKE 'Test%'"))
            };

            var result = Generator.Generate(expression);
            result.ShouldBe("UPDATE [dbo].[TestTable] SET [Name] = N'Updated' WHERE [Name] LIKE 'Test%';");
        }

        [Test]
        public void CanUpdateDataWithRawSqlComparisonOperators()
        {
            // Test RawSql with comparison operators (<, >, <=, >=, <>)
            var expression = new UpdateDataExpression
            {
                TableName = "TestTable"
            };
            expression.Set = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("Status", 1)
            };
            expression.Where = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("Age", RawSql.Insert("> 18"))
            };

            var result = Generator.Generate(expression);
            result.ShouldBe("UPDATE [dbo].[TestTable] SET [Status] = 1 WHERE [Age] > 18;");
        }
    }
}
