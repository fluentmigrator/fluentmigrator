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

using FluentMigrator.Exceptions;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Generators.SQLite;
using FluentMigrator.SqlServer;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Generators.SQLite
{
    [TestFixture]
    [Category("Generator")]
    [Category("SQLite")]
    // ReSharper disable once InconsistentNaming
    public class SQLiteGeneratorTests
    {
        protected SQLiteGenerator Generator;

        [SetUp]
        public void Setup()
        {
            // ReSharper disable once RedundantArgumentDefaultValue
            var quoter = new SQLiteQuoter(false);
            // ReSharper disable once RedundantArgumentDefaultValue
            var typeMap = new SQLiteTypeMap(false);
            Generator = new SQLiteGenerator(quoter, typeMap);
        }

        [Test]
        public void CanNotAlterColumnInStrictMode()
        {
            var expression = GeneratorTestHelper.GetAlterColumnExpression();

            Generator.CompatibilityMode = CompatibilityMode.STRICT;

            Assert.Throws<DatabaseOperationNotSupportedException>(() => Generator.Generate(expression));
        }

        [Test]
        public void CanNotAlterSchemaInStrictMode()
        {
            Generator.CompatibilityMode = CompatibilityMode.STRICT;

            Assert.Throws<DatabaseOperationNotSupportedException>(() => Generator.Generate(new CreateSchemaExpression()));
        }

        [Test]
        public void CanNotCreateForeignKeyInStrictMode()
        {
            Generator.CompatibilityMode = CompatibilityMode.STRICT;

            Assert.Throws<DatabaseOperationNotSupportedException>(() => Generator.Generate(GeneratorTestHelper.GetCreateNamedForeignKeyExpression()));
        }

        [Test]
        public void CanNotCreateMulitColumnForeignKeyInStrictMode()
        {
            Generator.CompatibilityMode = CompatibilityMode.STRICT;

            Assert.Throws<DatabaseOperationNotSupportedException>(() => Generator.Generate(GeneratorTestHelper.GetCreateNamedMultiColumnForeignKeyExpression()));
        }

        [Test]
        public void CanNotCreateSchemaInStrictMode()
        {
            Generator.CompatibilityMode = CompatibilityMode.STRICT;

            Assert.Throws<DatabaseOperationNotSupportedException>(() => Generator.Generate(new CreateSchemaExpression()));
        }

        [Test]
        public void CanCreateTableWithSeededIdentity([Values] CompatibilityMode compatibilityMode)
        {
            var expression = GeneratorTestHelper.GetCreateTableWithAutoIncrementExpression();
            expression.Columns[0].IsPrimaryKey = true;
            expression.Columns[0].IsIdentity = true;

            Generator.CompatibilityMode = compatibilityMode;
            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE \"TestTable1\" (\"TestColumn1\" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, \"TestColumn2\" INTEGER NOT NULL);");
        }

        [Test]
        public void CanNotDropForeignKeyInStrictMode()
        {
            var expression = GeneratorTestHelper.GetDeleteForeignKeyExpression();
            Generator.CompatibilityMode = CompatibilityMode.STRICT;

            Assert.Throws<DatabaseOperationNotSupportedException>(() => Generator.Generate(expression));
        }

        [Test]
        public void CanNotDropSchemaInStrictMode()
        {
            Generator.CompatibilityMode = CompatibilityMode.STRICT;

            Assert.Throws<DatabaseOperationNotSupportedException>(() => Generator.Generate(new DeleteSchemaExpression()));
        }

        [Test]
        public void CanNotCreateTableWithSeededIdentityAndStrictCompatibility()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithAutoIncrementExpression();
            expression.Columns[0].IsPrimaryKey = true;
            expression.Columns[0].AdditionalFeatures.Add(SqlServerExtensions.IdentitySeed, 3);
            expression.Columns[0].AdditionalFeatures.Add(SqlServerExtensions.IdentityIncrement, 3);
            Generator.CompatibilityMode = CompatibilityMode.STRICT;

            Assert.Throws<DatabaseOperationNotSupportedException>(() => Generator.Generate(expression));
        }

        [Test]
        public virtual void CanUseSystemMethodCurrentDateTimeAsADefaultValueForAColumn()
        {
            var expression = new CreateTableExpression { TableName = "TestTable1" };
            expression.Columns.Add(new ColumnDefinition { Name = "DateTimeCol", Type = DbType.DateTime, DefaultValue = SystemMethods.CurrentDateTime});

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE \"TestTable1\" (\"DateTimeCol\" DATETIME NOT NULL DEFAULT (datetime('now','localtime')));");
        }

        [Test]
        public virtual void CanUseSystemMethodCurrentUTCDateTimeAsDefaultValueForColumn()
        {
            var expression = new CreateTableExpression { TableName = "TestTable1" };
            expression.Columns.Add(new ColumnDefinition { Name = "DateTimeCol", Type = DbType.DateTime, DefaultValue = SystemMethods.CurrentUTCDateTime });

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE \"TestTable1\" (\"DateTimeCol\" DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP);");
        }

        [Test]
        public void CanRenameColumnInStrictMode()
        {
            var expression = GeneratorTestHelper.GetRenameColumnExpression();
            Generator.CompatibilityMode = CompatibilityMode.STRICT;

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestTable1\" RENAME COLUMN \"TestColumn1\" TO \"TestColumn2\";");
        }
    }
}
