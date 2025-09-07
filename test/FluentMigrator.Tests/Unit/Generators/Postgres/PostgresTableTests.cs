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

using System.Data;

using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Generators.Postgres;
using FluentMigrator.Runner.Processors.Postgres;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Generators.Postgres
{
    [TestFixture]
    public class PostgresTableTests : BaseTableTests
    {
        protected PostgresGenerator Generator;

        [SetUp]
        public void Setup()
        {
            var quoter = new PostgresQuoter(new PostgresOptions());
            Generator = new PostgresGenerator(quoter);
        }

        [Test]
        public override void CanCreateTableWithCustomColumnTypeWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            expression.Columns[0].IsPrimaryKey = true;
            expression.Columns[1].Type = null;
            expression.Columns[1].CustomType = "json";
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE \"TestSchema\".\"TestTable1\" (\"TestColumn1\" text NOT NULL, \"TestColumn2\" json NOT NULL, PRIMARY KEY (\"TestColumn1\"));");
        }

        [Test]
        public override void CanCreateTableWithCustomColumnTypeWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            expression.Columns[0].IsPrimaryKey = true;
            expression.Columns[1].Type = null;
            expression.Columns[1].CustomType = "json";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE \"public\".\"TestTable1\" (\"TestColumn1\" text NOT NULL, \"TestColumn2\" json NOT NULL, PRIMARY KEY (\"TestColumn1\"));");
        }

        [Test]
        public override void CanCreateTableWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE \"TestSchema\".\"TestTable1\" (\"TestColumn1\" text NOT NULL, \"TestColumn2\" integer NOT NULL);");
        }

        [Test]
        public override void CanCreateTableWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE \"public\".\"TestTable1\" (\"TestColumn1\" text NOT NULL, \"TestColumn2\" integer NOT NULL);");
        }

        [Test]
        public override void CanCreateTableWithDefaultValueExplicitlySetToNullWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithDefaultValue();
            expression.Columns[0].DefaultValue = null;
            expression.Columns[0].TableName = expression.TableName;
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE \"TestSchema\".\"TestTable1\" (\"TestColumn1\" text NOT NULL DEFAULT NULL, \"TestColumn2\" integer NOT NULL DEFAULT 0);");
        }

        [Test]
        public override void CanCreateTableWithDefaultValueExplicitlySetToNullWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithDefaultValue();
            expression.Columns[0].DefaultValue = null;
            expression.Columns[0].TableName = expression.TableName;

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE \"public\".\"TestTable1\" (\"TestColumn1\" text NOT NULL DEFAULT NULL, \"TestColumn2\" integer NOT NULL DEFAULT 0);");

        }

        [Test]
        public override void CanCreateTableWithDefaultValueWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithDefaultValue();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE \"TestSchema\".\"TestTable1\" (\"TestColumn1\" text NOT NULL DEFAULT 'Default', \"TestColumn2\" integer NOT NULL DEFAULT 0);");
        }

        [Test]
        public override void CanCreateTableWithIdentityWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithAutoIncrementExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE \"TestSchema\".\"TestTable1\" (\"TestColumn1\" serial NOT NULL, \"TestColumn2\" integer NOT NULL);");
        }

        [Test]
        public override void CanCreateTableWithIdentityWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithAutoIncrementExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE \"public\".\"TestTable1\" (\"TestColumn1\" serial NOT NULL, \"TestColumn2\" integer NOT NULL);");
        }

        [Test]
        public override void CanCreateTableWithDefaultValueWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithDefaultValue();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE \"public\".\"TestTable1\" (\"TestColumn1\" text NOT NULL DEFAULT 'Default', \"TestColumn2\" integer NOT NULL DEFAULT 0);");
        }

        [Test]
        public override void CanCreateTableWithMultiColumnPrimaryKeyWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithMultiColumnPrimaryKeyExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE \"TestSchema\".\"TestTable1\" (\"TestColumn1\" text NOT NULL, \"TestColumn2\" integer NOT NULL, PRIMARY KEY (\"TestColumn1\",\"TestColumn2\"));");
        }

        [Test]
        public override void CanCreateTableWithMultiColumnPrimaryKeyWithDefaultSchema([Values] CompatibilityMode compatibilityMode)
        {
            var expression = GeneratorTestHelper.GetCreateTableWithMultiColumnPrimaryKeyExpression();

            Generator.CompatibilityMode = compatibilityMode;
            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE \"public\".\"TestTable1\" (\"TestColumn1\" text NOT NULL, \"TestColumn2\" integer NOT NULL, PRIMARY KEY (\"TestColumn1\",\"TestColumn2\"));");
        }

        [Test]
        public override void CanCreateTableWithNamedMultiColumnPrimaryKeyWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithNamedMultiColumnPrimaryKeyExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE \"TestSchema\".\"TestTable1\" (\"TestColumn1\" text NOT NULL, \"TestColumn2\" integer NOT NULL, CONSTRAINT \"TestKey\" PRIMARY KEY (\"TestColumn1\",\"TestColumn2\"));");
        }

        [Test]
        public override void CanCreateTableWithNamedMultiColumnPrimaryKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithNamedMultiColumnPrimaryKeyExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE \"public\".\"TestTable1\" (\"TestColumn1\" text NOT NULL, \"TestColumn2\" integer NOT NULL, CONSTRAINT \"TestKey\" PRIMARY KEY (\"TestColumn1\",\"TestColumn2\"));");
        }

        [Test]
        public override void CanCreateTableWithNamedPrimaryKeyWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithNamedPrimaryKeyExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE \"TestSchema\".\"TestTable1\" (\"TestColumn1\" text NOT NULL, \"TestColumn2\" integer NOT NULL, CONSTRAINT \"TestKey\" PRIMARY KEY (\"TestColumn1\"));");
        }

        [Test]
        public override void CanCreateTableWithNamedPrimaryKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithNamedPrimaryKeyExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE \"public\".\"TestTable1\" (\"TestColumn1\" text NOT NULL, \"TestColumn2\" integer NOT NULL, CONSTRAINT \"TestKey\" PRIMARY KEY (\"TestColumn1\"));");
        }

        [Test]
        public override void CanCreateTableWithNullableFieldWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithNullableColumn();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE \"TestSchema\".\"TestTable1\" (\"TestColumn1\" text, \"TestColumn2\" integer NOT NULL);");
        }

        [Test]
        public override void CanCreateTableWithNullableFieldWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithNullableColumn();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE \"public\".\"TestTable1\" (\"TestColumn1\" text, \"TestColumn2\" integer NOT NULL);");
        }

        [Test]
        public override void CanCreateTableWithPrimaryKeyWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithPrimaryKeyExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE \"TestSchema\".\"TestTable1\" (\"TestColumn1\" text NOT NULL, \"TestColumn2\" integer NOT NULL, PRIMARY KEY (\"TestColumn1\"));");
        }

        [Test]
        public override void CanCreateTableWithPrimaryKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithPrimaryKeyExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE \"public\".\"TestTable1\" (\"TestColumn1\" text NOT NULL, \"TestColumn2\" integer NOT NULL, PRIMARY KEY (\"TestColumn1\"));");
        }

        [Test]
        public override void CanDropTableWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteTableExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("DROP TABLE \"TestSchema\".\"TestTable1\";");
        }

        [Test]
        public override void CanDropTableWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteTableExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("DROP TABLE \"public\".\"TestTable1\";");
        }

        [Test]
        public override void CanDropTableIfExistsWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteTableIfExistsExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("DROP TABLE IF EXISTS \"public\".\"TestTable1\";");
        }

        [Test]
        public override void CanRenameTableWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetRenameTableExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestSchema\".\"TestTable1\" RENAME TO \"TestTable2\";");
        }

        [Test]
        public override void CanRenameTableWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetRenameTableExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"public\".\"TestTable1\" RENAME TO \"TestTable2\";");
        }

        [Test]
        public override void CanCreateTableWithFluentMultiColumnForeignKey()
        {
            // Test the new fluent API for multi-column foreign keys
            // PostgreSQL doesn't support inline foreign keys in CREATE TABLE, so the foreign key definition is ignored
            var expression = new CreateTableExpression { TableName = "Area", SchemaName = "public" };
            expression.Columns.Add(new ColumnDefinition { Name = "ArticleId", Type = DbType.String });
            expression.Columns.Add(new ColumnDefinition { Name = "AreaGroupIndex", Type = DbType.Int32 });
            expression.Columns.Add(new ColumnDefinition { Name = "Index", Type = DbType.Int32, 
                IsForeignKey = true,
                ForeignKey = new ForeignKeyDefinition
                {
                    Name = "FK_Area_AreaGroup",
                    PrimaryTable = "AreaGroup",
                    ForeignTable = "Area",
                    PrimaryColumns = ["ArticleId", "Index"],
                    ForeignColumns = ["ArticleId", "AreaGroupIndex"]
                }
            });

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE \"public\".\"Area\" (\"ArticleId\" TEXT NOT NULL, \"AreaGroupIndex\" INTEGER NOT NULL, \"Index\" INTEGER NOT NULL);");
        }
    }
}
