using System.Data;

using FluentMigrator.Exceptions;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Extensions;
using FluentMigrator.Runner.Generators.SQLite;

using Xunit;

namespace FluentMigrator.Tests.Unit.Generators.SQLite
{
    public class SQLiteGeneratorTests
    {
        protected SQLiteGenerator Generator;

        public void Setup()
        {
            Generator = new SQLiteGenerator();
        }

        [Fact]
        public void CanAlterColumnInStrictMode()
        {
            var expression = GeneratorTestHelper.GetRenameColumnExpression();
            Generator.compatabilityMode = CompatabilityMode.STRICT;

            Assert.Throws<DatabaseOperationNotSupportedException>(() => Generator.Generate(expression));
        }

        [Fact]
        public void CanAlterSchemaInStrictMode()
        {
            Generator.compatabilityMode = CompatabilityMode.STRICT;

            Assert.Throws<DatabaseOperationNotSupportedException>(() => Generator.Generate(new CreateSchemaExpression()));
        }

        [Fact]
        public void CanCreateForeignKeyInStrictMode()
        {
            Generator.compatabilityMode = CompatabilityMode.STRICT;

            Assert.Throws<DatabaseOperationNotSupportedException>(() => Generator.Generate(GeneratorTestHelper.GetCreateNamedForeignKeyExpression()));
        }

        [Fact]
        public void CanCreateMulitColumnForeignKeyInStrictMode()
        {
            Generator.compatabilityMode = CompatabilityMode.STRICT;

            Assert.Throws<DatabaseOperationNotSupportedException>(() => Generator.Generate(GeneratorTestHelper.GetCreateNamedMultiColumnForeignKeyExpression()));
        }

        [Fact]
        public void CanCreateSchemaInStrictMode()
        {
            Generator.compatabilityMode = CompatabilityMode.STRICT;

            Assert.Throws<DatabaseOperationNotSupportedException>(() => Generator.Generate(new CreateSchemaExpression()));
        }

        [Fact]
        public void CanCreateTableWithSeededIdentityAndLooseCompatibility()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithAutoIncrementExpression();
            expression.Columns[0].IsPrimaryKey = true;
            expression.Columns[0].AdditionalFeatures.Add(SqlServerExtensions.IdentitySeed, 3);
            expression.Columns[0].AdditionalFeatures.Add(SqlServerExtensions.IdentityIncrement, 3);
            Generator.compatabilityMode = CompatabilityMode.LOOSE;

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE \"TestTable1\" (\"TestColumn1\" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, \"TestColumn2\" INTEGER NOT NULL)");
        }

        [Fact]
        public void CanDropForeignKeyInStrictMode()
        {
            var expression = GeneratorTestHelper.GetDeleteForeignKeyExpression();
            Generator.compatabilityMode = CompatabilityMode.STRICT;

            Assert.Throws<DatabaseOperationNotSupportedException>(() => Generator.Generate(expression));
        }

        [Fact]
        public void CanDropSchemaInStrictMode()
        {
            Generator.compatabilityMode = CompatabilityMode.STRICT;

            Assert.Throws<DatabaseOperationNotSupportedException>(() => Generator.Generate(new DeleteSchemaExpression()));
        }

        [Fact]
        public void CanNotCreateTableWithSeededIdentityAndStrictCompatibility()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithAutoIncrementExpression();
            expression.Columns[0].IsPrimaryKey = true;
            expression.Columns[0].AdditionalFeatures.Add(SqlServerExtensions.IdentitySeed, 3);
            expression.Columns[0].AdditionalFeatures.Add(SqlServerExtensions.IdentityIncrement, 3);
            Generator.compatabilityMode = CompatabilityMode.STRICT;

            Assert.Throws<DatabaseOperationNotSupportedException>(() => Generator.Generate(expression));
        }

        [Fact]
        public void CanUseSystemMethodCurrentDateTimeAsADefaultValueForAColumn()
        {
            var expression = new CreateTableExpression { TableName = "TestTable1" };
            expression.Columns.Add(new ColumnDefinition { Name = "DateTimeCol", Type = DbType.DateTime, DefaultValue = SystemMethods.CurrentDateTime});

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE \"TestTable1\" (\"DateTimeCol\" DATETIME NOT NULL DEFAULT datetime('now','localtime'))");
        }

        [Fact]
        public void CanUseSystemMethodCurrentUTCDateTimeAsDefaultValueForColumn()
        {
            var expression = new CreateTableExpression { TableName = "TestTable1" };
            expression.Columns.Add(new ColumnDefinition { Name = "DateTimeCol", Type = DbType.DateTime, DefaultValue = SystemMethods.CurrentUTCDateTime });

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE \"TestTable1\" (\"DateTimeCol\" DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP)");
        }
        [Fact]
        public void CanRenameColumnInStrictMode()
        {
            Generator.compatabilityMode = CompatabilityMode.STRICT;

            Assert.Throws<DatabaseOperationNotSupportedException>(() => Generator.Generate(GeneratorTestHelper.GetRenameColumnExpression()));
        }
    }
}
