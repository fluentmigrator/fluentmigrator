#region License
//
// Copyright (c) 2026, Fluent Migrator Project
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
using System.Linq;

using FluentMigrator.Exceptions;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Conventions;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Generators.Firebird;
using FluentMigrator.Runner.Generators.MySql;
using FluentMigrator.Runner.Generators.Oracle;
using FluentMigrator.Runner.Generators.Postgres;
using FluentMigrator.Runner.Generators.Redshift;
using FluentMigrator.Runner.Generators.SQLite;
using FluentMigrator.Runner.Generators.SqlServer;
using FluentMigrator.Runner.Processors.Firebird;
using FluentMigrator.Runner.Processors.Postgres;
using FluentMigrator.Tests.Unit.Generators;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit
{
    /// <summary>
    /// The indexes and foreign keys of <c>Create.Table(...).IfNotExists()</c>, which live in
    /// <see cref="CreateTableExpression.ExpressionsForNewTable"/> (issue #2444).
    /// </summary>
    [TestFixture]
    public class CreateTableIfNotExistsTests
    {
        [Test]
        public void ConventionsNameTheIndexesAndForeignKeysOfTheTable()
        {
            var expression = CreateWidget();

            expression.Apply(new DefaultConventionSet("app", workingDirectory: null));

            var index = (CreateIndexExpression) expression.ExpressionsForNewTable[0];
            var foreignKey = (CreateForeignKeyExpression) expression.ExpressionsForNewTable[1];

            index.Index.Name.ShouldBe("IX_Widget_KindId");
            index.Index.SchemaName.ShouldBe("app");
            foreignKey.ForeignKey.Name.ShouldBe("FK_Widget_KindId_Kind_Id");
            foreignKey.ForeignKey.ForeignTableSchema.ShouldBe("app");
        }

        [Test]
        public void AGeneratorWithoutTheGuardLeavesThemToTheProcessor()
        {
            var expression = CreateWidget();

            expression.GetExpressionsProcessedSeparately(new RedshiftGenerator())
                .ShouldBe(expression.ExpressionsForNewTable);
        }

        [Test]
        public void AGuardingGeneratorKeepsThemFromTheProcessor()
        {
            var expression = CreateWidget();

            expression.GetExpressionsProcessedSeparately(new SqlServer2016Generator()).ShouldBeEmpty();
        }

        [Test]
        public void MySqlLeavesAForeignKeyFromAnotherTableToTheProcessor()
        {
            var expression = CreateWidget();
            var referencedBy = new CreateForeignKeyExpression
            {
                ForeignKey = new ForeignKeyDefinition
                {
                    Name = "FK_Part_Widget",
                    ForeignTable = "Part",
                    ForeignColumns = { "WidgetId" },
                    PrimaryTable = "Widget",
                    PrimaryColumns = { "Id" },
                },
            };
            expression.ExpressionsForNewTable.Add(referencedBy);

            expression.GetExpressionsProcessedSeparately(new MySql5Generator()).ShouldBe([referencedBy]);
        }

        [Test]
        public void SQLiteLeavesASelfReferencingReferencedByKeyToTheProcessor()
        {
            // Create.Table("Node").IfNotExists().WithColumn("Id")...ReferencedBy("FK_Node_Parent", "Node", "ParentId"):
            // the key is on this table, but ReferencedBy gives the ParentId column no foreign key, so SQLiteColumn
            // doesn't write it in the CREATE TABLE. If it counted as created with the table, the migration would pass
            // without the key, even in STRICT mode. Left to the processor, SQLite reports that it can't add it.
            var expression = new CreateTableExpression { TableName = "Node", IfNotExists = true };
            expression.Columns.Add(new ColumnDefinition { Name = "Id", Type = System.Data.DbType.Int32, IsPrimaryKey = true, TableName = "Node" });
            expression.Columns.Add(new ColumnDefinition { Name = "ParentId", Type = System.Data.DbType.Int32, IsNullable = true, TableName = "Node" });
            var referencedBy = new CreateForeignKeyExpression
            {
                ForeignKey = new ForeignKeyDefinition
                {
                    Name = "FK_Node_Parent",
                    ForeignTable = "Node",
                    ForeignColumns = { "ParentId" },
                    PrimaryTable = "Node",
                    PrimaryColumns = { "Id" },
                },
            };
            expression.ExpressionsForNewTable.Add(referencedBy);
            var generator = new SQLiteGenerator { CompatibilityMode = CompatibilityMode.STRICT };

            generator.Generate(expression).ShouldNotContain("FK_Node_Parent");
            expression.GetExpressionsProcessedSeparately(generator).ShouldBe([referencedBy]);
            Should.Throw<DatabaseOperationNotSupportedException>(() => generator.Generate(referencedBy));
        }

        [Test]
        public void SQLiteKeepsTheKeysItWritesInTheCreateTable()
        {
            // WithColumn("KindId").ForeignKey("Kind", "Id") gives the column the key, which SQLiteColumn writes.
            var expression = CreateWidget();
            var foreignKey = (CreateForeignKeyExpression) expression.ExpressionsForNewTable[1];
            expression.Columns[0].IsForeignKey = true;
            expression.Columns[0].ForeignKey = foreignKey.ForeignKey;

            expression.GetExpressionsProcessedSeparately(new SQLiteGenerator()).ShouldBeEmpty();
        }

        /// <summary>
        /// Generators whose <see cref="ICreateTableIfNotExistsGenerator.GeneratesWithTable"/> is overridden to leave the
        /// foreign key out of the table.
        /// </summary>
        private static IEnumerable<TestCaseData> GeneratorsThatLeaveTheForeignKeyOut()
        {
            yield return new TestCaseData(new SqlServer2000WithoutForeignKeys()).SetArgDisplayNames("SqlServer2000");
            yield return new TestCaseData(new SqlServer2005WithoutForeignKeys()).SetArgDisplayNames("SqlServer2005");
            yield return new TestCaseData(new OracleWithoutForeignKeys()).SetArgDisplayNames("Oracle");
            yield return new TestCaseData(new FirebirdWithoutForeignKeys()).SetArgDisplayNames("Firebird");
            yield return new TestCaseData(new PostgresWithoutForeignKeys()).SetArgDisplayNames("Postgres");
            yield return new TestCaseData(new MySql5WithoutForeignKeys()).SetArgDisplayNames("MySql5");
        }

        [TestCaseSource(nameof(GeneratorsThatLeaveTheForeignKeyOut))]
        public void AnOverriddenGeneratesWithTableDecidesWhatTheTablesSqlHas(IMigrationGenerator generator)
        {
            // The processor runs what GeneratesWithTable leaves out. If the generator chose the table's statements by
            // another test, an override would make the two disagree: a statement in both would run twice, and one in
            // neither not at all.
            var expression = GeneratorTestHelper.GetCreateTableIfNotExistsWithIndexAndForeignKey();
            var foreignKey = expression.ExpressionsForNewTable.OfType<CreateForeignKeyExpression>().Single();

            var sql = generator.Generate(expression);

            Assert.Multiple(
                () =>
                {
                    Assert.That(sql, Does.Contain("UX_TestTable1_TestColumn1"), "the index, which stays with the table");
                    Assert.That(sql, Does.Not.Contain("FK_TestTable1_TestTable2"), "the foreign key, which the override leaves out");
                    Assert.That(expression.GetExpressionsProcessedSeparately(generator), Is.EqualTo(new[] { foreignKey }), "processed separately");
                });
        }

        [Test]
        public void WithoutIfNotExistsTheProcessorRunsThemAll()
        {
            var expression = CreateWidget();
            expression.IfNotExists = false;

            expression.GetExpressionsProcessedSeparately(new SqlServer2016Generator())
                .ShouldBe(expression.ExpressionsForNewTable);
        }

        /// <summary>
        /// <c>Create.Table("Widget").IfNotExists().WithColumn("KindId").AsInt32().Indexed().ForeignKey("Kind", "Id")</c>,
        /// before the conventions name the index and the foreign key.
        /// </summary>
        private static CreateTableExpression CreateWidget()
        {
            var expression = new CreateTableExpression { TableName = "Widget", IfNotExists = true };
            expression.Columns.Add(new ColumnDefinition { Name = "KindId", Type = System.Data.DbType.Int32, TableName = "Widget" });

            var index = new CreateIndexExpression { Index = new IndexDefinition { TableName = "Widget" } };
            index.Index.Columns.Add(new IndexColumnDefinition { Name = "KindId" });

            var foreignKey = new CreateForeignKeyExpression
            {
                ForeignKey = new ForeignKeyDefinition
                {
                    ForeignTable = "Widget",
                    ForeignColumns = { "KindId" },
                    PrimaryTable = "Kind",
                    PrimaryColumns = { "Id" },
                },
            };

            expression.ExpressionsForNewTable.Add(index);
            expression.ExpressionsForNewTable.Add(foreignKey);
            return expression;
        }

        private static bool IsNotAForeignKey(IMigrationExpression expressionForNewTable) =>
            !(expressionForNewTable is CreateForeignKeyExpression);

        private sealed class SqlServer2000WithoutForeignKeys : SqlServer2000Generator
        {
            public override bool GeneratesWithTable(CreateTableExpression expression, IMigrationExpression expressionForNewTable) =>
                IsNotAForeignKey(expressionForNewTable) && base.GeneratesWithTable(expression, expressionForNewTable);
        }

        private sealed class SqlServer2005WithoutForeignKeys : SqlServer2005Generator
        {
            public override bool GeneratesWithTable(CreateTableExpression expression, IMigrationExpression expressionForNewTable) =>
                IsNotAForeignKey(expressionForNewTable) && base.GeneratesWithTable(expression, expressionForNewTable);
        }

        private sealed class OracleWithoutForeignKeys : OracleGenerator
        {
            public override bool GeneratesWithTable(CreateTableExpression expression, IMigrationExpression expressionForNewTable) =>
                IsNotAForeignKey(expressionForNewTable) && base.GeneratesWithTable(expression, expressionForNewTable);
        }

        private sealed class FirebirdWithoutForeignKeys : FirebirdGenerator
        {
            public FirebirdWithoutForeignKeys()
                : base(FirebirdOptions.StandardBehaviour())
            {
            }

            public override bool GeneratesWithTable(CreateTableExpression expression, IMigrationExpression expressionForNewTable) =>
                IsNotAForeignKey(expressionForNewTable) && base.GeneratesWithTable(expression, expressionForNewTable);
        }

        private sealed class PostgresWithoutForeignKeys : PostgresGenerator
        {
            public PostgresWithoutForeignKeys()
                : base(new PostgresQuoter(new PostgresOptions()))
            {
            }

            public override bool GeneratesWithTable(CreateTableExpression expression, IMigrationExpression expressionForNewTable) =>
                IsNotAForeignKey(expressionForNewTable) && base.GeneratesWithTable(expression, expressionForNewTable);
        }

        private sealed class MySql5WithoutForeignKeys : MySql5Generator
        {
            public override bool GeneratesWithTable(CreateTableExpression expression, IMigrationExpression expressionForNewTable) =>
                IsNotAForeignKey(expressionForNewTable) && base.GeneratesWithTable(expression, expressionForNewTable);
        }
    }
}
