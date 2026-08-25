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
using System.Data;
using System.Linq;

using FluentMigrator.Exceptions;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators.SQLite;
using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Generators.SQLite
{
    [TestFixture]
    // ReSharper disable once InconsistentNaming
#pragma warning disable NUnit1034 // Class is used as base class but also contains tests to execute
    public class SQLiteColumnTests : BaseColumnTests
#pragma warning restore NUnit1034
    {
        protected SQLiteGenerator Generator;
        private SQLiteQuoter quoter = new SQLiteQuoter();

        [SetUp]
        public void Setup()
        {
            Generator = new SQLiteGenerator(quoter, new SQLiteTypeMap());
        }

        [Test]
        public override void CanCreateNullableColumnWithCustomDomainTypeAndCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpressionWithNullableCustomType();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestSchema\".\"TestTable1\" ADD COLUMN \"TestColumn1\" MyDomainType;");
        }

        [Test]
        public override void CanCreateNullableColumnWithCustomDomainTypeAndDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpressionWithNullableCustomType();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestTable1\" ADD COLUMN \"TestColumn1\" MyDomainType;");
        }

        [Test]
        public override void CanAlterColumnWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetAlterColumnExpression();
            expression.SchemaName = "TestSchema";

            Assert.Throws<DatabaseOperationNotSupportedException>(() => Generator.Generate(expression));
        }

        [Test]
        public override void CanAlterColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetAlterColumnExpression();

            Assert.Throws<DatabaseOperationNotSupportedException>(() => Generator.Generate(expression));
        }

        [Test]
        public override void CanCreateAutoIncrementColumnWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetAlterTableAutoIncrementColumnExpression();
            expression.Column.IsPrimaryKey = true;
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestSchema\".\"TestTable1\" ADD COLUMN \"TestColumn1\" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT;");
        }

        [Test]
        public override void CanCreateAutoIncrementColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetAlterTableAutoIncrementColumnExpression();
            expression.Column.IsPrimaryKey = true;

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestTable1\" ADD COLUMN \"TestColumn1\" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT;");
        }

        [Test]
        public override void CanCreateColumnWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestSchema\".\"TestTable1\" ADD COLUMN \"TestColumn1\" TEXT NOT NULL;");
        }

        [Test]
        public override void CanCreateColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestTable1\" ADD COLUMN \"TestColumn1\" TEXT NOT NULL;");
        }

        [Test]
        public override void CanCreateColumnWithSystemMethodAndCustomSchema()
        {
            var expressions = GeneratorTestHelper.GetCreateColumnWithSystemMethodExpression("TestSchema");
            var result = expressions.Select(x => (string)Generator.Generate((dynamic)x));
            result.ShouldBe([
                @"ALTER TABLE ""TestSchema"".""TestTable1"" ADD COLUMN ""TestColumn1"" DATETIME;",
                @"UPDATE ""TestSchema"".""TestTable1"" SET ""TestColumn1"" = (datetime('now','localtime')) WHERE 1 = 1;"
            ]);
        }

        [Test]
        public override void CanCreateColumnWithSystemMethodAndDefaultSchema()
        {
            var expressions = GeneratorTestHelper.GetCreateColumnWithSystemMethodExpression();
            var result = expressions.Select(x => (string)Generator.Generate((dynamic)x));
            result.ShouldBe([
                @"ALTER TABLE ""TestTable1"" ADD COLUMN ""TestColumn1"" DATETIME;",
                @"UPDATE ""TestTable1"" SET ""TestColumn1"" = (datetime('now','localtime')) WHERE 1 = 1;"
            ]);
        }

        [Test]
        public override void CanCreateDecimalColumnWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateDecimalColumnExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestSchema\".\"TestTable1\" ADD COLUMN \"TestColumn1\" NUMERIC NOT NULL;");
        }

        [Test]
        public override void CanCreateDecimalColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateDecimalColumnExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestTable1\" ADD COLUMN \"TestColumn1\" NUMERIC NOT NULL;");
        }

        [Test]
        public override void CanDropColumnWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteColumnExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestSchema\".\"TestTable1\" DROP COLUMN \"TestColumn1\";");
        }

        [Test]
        public override void CanDropColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteColumnExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestTable1\" DROP COLUMN \"TestColumn1\";");
        }

        [Test]
        public override void CanDropMultipleColumnsWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteColumnExpression(new[] { "TestColumn1", "TestColumn2" });
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestSchema\".\"TestTable1\" DROP COLUMN \"TestColumn1\";ALTER TABLE \"TestSchema\".\"TestTable1\" DROP COLUMN \"TestColumn2\";");
        }

        [Test]
        public override void CanDropMultipleColumnsWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteColumnExpression(new [] { "TestColumn1", "TestColumn2" });

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestTable1\" DROP COLUMN \"TestColumn1\";ALTER TABLE \"TestTable1\" DROP COLUMN \"TestColumn2\";");
        }

        [Test]
        public override void CanRenameColumnWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetRenameColumnExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestSchema\".\"TestTable1\" RENAME COLUMN \"TestColumn1\" TO \"TestColumn2\";");
        }

        [Test]
        public override void CanRenameColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetRenameColumnExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestTable1\" RENAME COLUMN \"TestColumn1\" TO \"TestColumn2\";");
        }

        [Test]
        public virtual void CanCreateUniqueColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpression();
            expression.Column.IsUnique = true;

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestTable1\" ADD COLUMN \"TestColumn1\" TEXT NOT NULL UNIQUE;");
        }

        [Test]
        public virtual void CanCreateUniqueColumnWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpression();
            expression.Column.IsUnique = true;
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestSchema\".\"TestTable1\" ADD COLUMN \"TestColumn1\" TEXT NOT NULL UNIQUE;");
        }

        [Test]
        public override void CanCreateColumnWithComputedExpression()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpressionWithComputed();
            
            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestTable1\" ADD COLUMN \"TestColumn1\" GENERATED ALWAYS AS (Price * Quantity) VIRTUAL NOT NULL;");
        }

        [Test]
        public override void CanCreateColumnWithStoredComputedExpression()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpressionWithStoredComputed();
            
            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestTable1\" ADD COLUMN \"TestColumn1\" GENERATED ALWAYS AS (Price * Quantity) STORED NOT NULL;");
        }

        [Test]
        public override void CanAlterColumnToAddComputedExpression()
        {
            var expression = GeneratorTestHelper.GetAlterColumnExpressionWithComputed();
            
            // SQLite doesn't support altering columns
            Assert.Throws<DatabaseOperationNotSupportedException>(() => Generator.Generate(expression));
        }

        [Test]
        public override void CanAlterColumnToAddStoredComputedExpression()
        {
            var expression = GeneratorTestHelper.GetAlterColumnExpressionWithStoredComputed();

            // SQLite doesn't support altering columns
            Assert.Throws<DatabaseOperationNotSupportedException>(() => Generator.Generate(expression));
        }

        /// <summary>
        /// The expression pair produced by Alter.Table("TestTable1").AddColumn("TestColumn1")
        /// .AsInt32().Nullable().ForeignKey("TestTable2", "TestColumn2"); see issue #2388.
        /// </summary>
        private static (CreateColumnExpression AddColumn, CreateForeignKeyExpression ForeignKey) GetCreateColumnWithForeignKeyExpressions(string foreignKeyName = null)
        {
            var foreignKey = new ForeignKeyDefinition
            {
                Name = foreignKeyName,
                PrimaryTable = GeneratorTestHelper.TestTableName2,
                ForeignTable = GeneratorTestHelper.TestTableName1,
                PrimaryColumns = new[] { GeneratorTestHelper.TestColumnName2 },
                ForeignColumns = new[] { GeneratorTestHelper.TestColumnName1 }
            };

            var addColumn = new CreateColumnExpression
            {
                TableName = GeneratorTestHelper.TestTableName1,
                Column = new ColumnDefinition
                {
                    Name = GeneratorTestHelper.TestColumnName1,
                    Type = DbType.Int32,
                    IsNullable = true,
                    IsForeignKey = true,
                    ForeignKey = foreignKey
                }
            };

            var createForeignKey = new CreateForeignKeyExpression { ForeignKey = foreignKey };

            return (addColumn, createForeignKey);
        }

        [Test]
        public void CanCreateColumnWithForeignKey()
        {
            var (addColumn, _) = GetCreateColumnWithForeignKeyExpressions();

            var result = Generator.Generate(addColumn);
            result.ShouldBe("ALTER TABLE \"TestTable1\" ADD COLUMN \"TestColumn1\" INTEGER REFERENCES \"TestTable2\" (\"TestColumn2\");");
        }

        [Test]
        public void CanCreateColumnWithNamedForeignKey()
        {
            var (addColumn, _) = GetCreateColumnWithForeignKeyExpressions("FK_TestTable1_TestColumn1");

            var result = Generator.Generate(addColumn);
            result.ShouldBe("ALTER TABLE \"TestTable1\" ADD COLUMN \"TestColumn1\" INTEGER CONSTRAINT \"FK_TestTable1_TestColumn1\" REFERENCES \"TestTable2\" (\"TestColumn2\");");
        }

        [Test]
        public void CanCreateColumnWithForeignKeyWithOnDeleteAndOnUpdateOptions()
        {
            var (addColumn, _) = GetCreateColumnWithForeignKeyExpressions();
            addColumn.Column.ForeignKey.OnDelete = Rule.Cascade;
            addColumn.Column.ForeignKey.OnUpdate = Rule.SetNull;

            var result = Generator.Generate(addColumn);
            result.ShouldBe("ALTER TABLE \"TestTable1\" ADD COLUMN \"TestColumn1\" INTEGER REFERENCES \"TestTable2\" (\"TestColumn2\") ON DELETE CASCADE ON UPDATE SET NULL;");
        }

        [Test]
        public void CreatingColumnWithForeignKeyMarksTheForeignKeyExpressionAsHandled()
        {
            // The fluent builder queues a CreateForeignKeyExpression alongside the
            // CreateColumnExpression; once the column is generated with its inline
            // REFERENCES clause, the standalone expression must not throw.
            var (addColumn, createForeignKey) = GetCreateColumnWithForeignKeyExpressions("FK_TestTable1_TestColumn1");

            Generator.Generate(addColumn);

            var result = Generator.Generate(createForeignKey);
            result.ShouldBe(string.Empty);
        }
    }
}
