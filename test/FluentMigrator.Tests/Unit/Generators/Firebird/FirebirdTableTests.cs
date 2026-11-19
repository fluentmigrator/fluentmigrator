using System;
using System.Linq;

using FirebirdSql.Data.Isql;

using FluentMigrator.Runner;
using FluentMigrator.Runner.Generators.Firebird;
using FluentMigrator.Runner.Processors.Firebird;
using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Generators.Firebird
{
    [TestFixture]
    public class FirebirdTableTests : BaseTableTests
    {
        protected FirebirdGenerator Generator;

        [SetUp]
        public void Setup()
        {
            Generator = new FirebirdGenerator(FirebirdOptions.StandardBehaviour());
        }

        [Test]
        public override void CanCreateTableWithCustomColumnTypeWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            expression.Columns[0].IsPrimaryKey = true;
            expression.Columns[1].Type = null;
            expression.Columns[1].CustomType = "[timestamp]";
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE TestTable1 (TestColumn1 VARCHAR(255) NOT NULL, TestColumn2 [timestamp] NOT NULL, PRIMARY KEY (TestColumn1));");
        }

        [Test]
        public override void CanCreateTableWithCustomColumnTypeWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            expression.Columns[0].IsPrimaryKey = true;
            expression.Columns[1].Type = null;
            expression.Columns[1].CustomType = "[timestamp]";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE TestTable1 (TestColumn1 VARCHAR(255) NOT NULL, TestColumn2 [timestamp] NOT NULL, PRIMARY KEY (TestColumn1));");
        }

        [Test]
        public override void CanCreateTableWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE TestTable1 (TestColumn1 VARCHAR(255) NOT NULL, TestColumn2 INTEGER NOT NULL);");
        }

        [Test]
        public override void CanCreateTableWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE TestTable1 (TestColumn1 VARCHAR(255) NOT NULL, TestColumn2 INTEGER NOT NULL);");
        }

        [TestCase(CompatibilityMode.LOOSE)]
        [TestCase(CompatibilityMode.STRICT)]
        public void CanCreateTableIfNotExistsWithDefaultSchema(CompatibilityMode compatibilityMode)
        {
            Generator.CompatibilityMode = compatibilityMode;
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            expression.IfNotExists = true;
            expression.Columns[1].DefaultValue = "it's";

            var result = Generator.Generate(expression);
            result.ShouldBe(
                "SET TERM ^ ;" + Environment.NewLine +
                "EXECUTE BLOCK AS" + Environment.NewLine +
                "BEGIN" + Environment.NewLine +
                "IF (NOT EXISTS (SELECT 1 FROM RDB$RELATIONS WHERE (RDB$FLAGS IS NOT NULL) AND (RDB$RELATION_NAME = 'TESTTABLE1'))) THEN" + Environment.NewLine +
                "BEGIN" + Environment.NewLine +
                "IN AUTONOMOUS TRANSACTION DO EXECUTE STATEMENT 'CREATE TABLE TestTable1 (TestColumn1 VARCHAR(255) NOT NULL, TestColumn2 INTEGER DEFAULT ''it''''s'' NOT NULL)';" + Environment.NewLine +
                "END" + Environment.NewLine +
                "END^" + Environment.NewLine +
                "SET TERM ; ^");
        }

        [Test]
        public void CanCreateTableIfNotExistsWithIndexAndForeignKey()
        {
            var expression = GeneratorTestHelper.GetCreateTableIfNotExistsWithIndexAndForeignKey();

            var result = Generator.Generate(expression);

            // In the block with the table, each committed before the next, as isql's AUTODDL does.
            result.ShouldBe(
                "SET TERM ^ ;" + Environment.NewLine +
                "EXECUTE BLOCK AS" + Environment.NewLine +
                "BEGIN" + Environment.NewLine +
                "IF (NOT EXISTS (SELECT 1 FROM RDB$RELATIONS WHERE (RDB$FLAGS IS NOT NULL) AND (RDB$RELATION_NAME = 'TESTTABLE1'))) THEN" + Environment.NewLine +
                "BEGIN" + Environment.NewLine +
                "IN AUTONOMOUS TRANSACTION DO EXECUTE STATEMENT 'CREATE TABLE TestTable1 (TestColumn1 VARCHAR(255) NOT NULL, TestColumn2 INTEGER NOT NULL)';" + Environment.NewLine +
                "IN AUTONOMOUS TRANSACTION DO EXECUTE STATEMENT 'CREATE UNIQUE ASC INDEX UX_TestTable1_TestColumn1 ON TestTable1 (TestColumn1)';" + Environment.NewLine +
                "IN AUTONOMOUS TRANSACTION DO EXECUTE STATEMENT 'ALTER TABLE TestTable1 ADD CONSTRAINT FK_TestTable1_TestTable2 FOREIGN KEY (TestColumn2) REFERENCES TestTable2 (TestColumn2)';" + Environment.NewLine +
                "END" + Environment.NewLine +
                "END^" + Environment.NewLine +
                "SET TERM ; ^");
        }

        [TestCase("Users", false, "USERS")]
        [TestCase("\"Users\"", false, "Users")]
        [TestCase("Users", true, "Users")]
        [TestCase("Order", false, "Order")]
        public void CreateTableIfNotExistsLooksForTheNameAsFirebirdStoresIt(string tableName, bool forceQuote, string storedName)
        {
            // Unquoted names are stored in upper case, quoted ones as written, and USERS and "Users" are different
            // tables, so the guard compares the stored name exactly. Order is a keyword, which the quoter quotes.
            var options = FirebirdOptions.StandardBehaviour();
            options.ForceQuote = forceQuote;
            var generator = new FirebirdGenerator(options);
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            expression.TableName = tableName;
            expression.IfNotExists = true;

            var result = generator.Generate(expression);

            result.ShouldContain($"(RDB$RELATION_NAME = '{storedName}')");
        }

        [Test]
        public void CreateTableIfNotExistsIsOneStatementForIsql()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            expression.IfNotExists = true;

            var script = new FbScript(Generator.Generate(expression) + Environment.NewLine + "SELECT 1 FROM RDB$DATABASE;");
            script.Parse();

            // The parser applies SET TERM itself; it is not one of the results.
            var statements = script.Results.ToList();
            statements.Count.ShouldBe(2);
            statements[0].StatementType.ShouldBe(SqlStatementType.ExecuteBlock);
            statements[1].StatementType.ShouldBe(SqlStatementType.Select);
        }

        [Test]
        public override void CanCreateTableWithDefaultValueExplicitlySetToNullWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithDefaultValue();
            expression.Columns[0].DefaultValue = null;
            expression.Columns[0].TableName = expression.TableName;
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE TestTable1 (TestColumn1 VARCHAR(255) DEFAULT NULL NOT NULL, TestColumn2 INTEGER DEFAULT 0 NOT NULL);");
        }

        [Test]
        public override void CanCreateTableWithDefaultValueExplicitlySetToNullWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithDefaultValue();
            expression.Columns[0].DefaultValue = null;
            expression.Columns[0].TableName = expression.TableName;

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE TestTable1 (TestColumn1 VARCHAR(255) DEFAULT NULL NOT NULL, TestColumn2 INTEGER DEFAULT 0 NOT NULL);");

        }

        [Test]
        public override void CanCreateTableWithDefaultValueWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithDefaultValue();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE TestTable1 (TestColumn1 VARCHAR(255) DEFAULT 'Default' NOT NULL, TestColumn2 INTEGER DEFAULT 0 NOT NULL);");
        }

        [Test]
        public override void CanCreateTableWithDefaultValueWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithDefaultValue();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE TestTable1 (TestColumn1 VARCHAR(255) DEFAULT 'Default' NOT NULL, TestColumn2 INTEGER DEFAULT 0 NOT NULL);");
        }

        [Test]
        public override void CanCreateTableWithIdentityWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithAutoIncrementExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE TestTable1 (TestColumn1 INTEGER NOT NULL, TestColumn2 INTEGER NOT NULL);");
        }

        [Test]
        public override void CanCreateTableWithIdentityWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithAutoIncrementExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE TestTable1 (TestColumn1 INTEGER NOT NULL, TestColumn2 INTEGER NOT NULL);");
        }

        [Test]
        public override void CanCreateTableWithMultiColumnPrimaryKeyWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithMultiColumnPrimaryKeyExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE TestTable1 (TestColumn1 VARCHAR(255) NOT NULL, TestColumn2 INTEGER NOT NULL, PRIMARY KEY (TestColumn1, TestColumn2));");
        }

        [Test]
        public override void CanCreateTableWithMultiColumnPrimaryKeyWithDefaultSchema([Values] CompatibilityMode compatibilityMode)
        {
            var expression = GeneratorTestHelper.GetCreateTableWithMultiColumnPrimaryKeyExpression();

            Generator.CompatibilityMode = compatibilityMode;
            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE TestTable1 (TestColumn1 VARCHAR(255) NOT NULL, TestColumn2 INTEGER NOT NULL, PRIMARY KEY (TestColumn1, TestColumn2));");
        }

        [Test]
        public override void CanCreateTableWithNamedMultiColumnPrimaryKeyWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithNamedMultiColumnPrimaryKeyExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE TestTable1 (TestColumn1 VARCHAR(255) NOT NULL, TestColumn2 INTEGER NOT NULL, CONSTRAINT TestKey PRIMARY KEY (TestColumn1, TestColumn2));");
        }

        [Test]
        public override void CanCreateTableWithNamedMultiColumnPrimaryKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithNamedMultiColumnPrimaryKeyExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE TestTable1 (TestColumn1 VARCHAR(255) NOT NULL, TestColumn2 INTEGER NOT NULL, CONSTRAINT TestKey PRIMARY KEY (TestColumn1, TestColumn2));");
        }

        [Test]
        public override void CanCreateTableWithNamedPrimaryKeyWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithNamedPrimaryKeyExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE TestTable1 (TestColumn1 VARCHAR(255) NOT NULL, TestColumn2 INTEGER NOT NULL, CONSTRAINT TestKey PRIMARY KEY (TestColumn1));");
        }

        [Test]
        public override void CanCreateTableWithNamedPrimaryKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithNamedPrimaryKeyExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE TestTable1 (TestColumn1 VARCHAR(255) NOT NULL, TestColumn2 INTEGER NOT NULL, CONSTRAINT TestKey PRIMARY KEY (TestColumn1));");
        }

        [Test]
        public override void CanCreateTableWithNullableFieldWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithNullableColumn();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE TestTable1 (TestColumn1 VARCHAR(255), TestColumn2 INTEGER NOT NULL);");
        }

        [Test]
        public override void CanCreateTableWithNullableFieldWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithNullableColumn();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE TestTable1 (TestColumn1 VARCHAR(255), TestColumn2 INTEGER NOT NULL);");
        }

        [Test]
        public override void CanCreateTableWithPrimaryKeyWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithPrimaryKeyExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE TestTable1 (TestColumn1 VARCHAR(255) NOT NULL, TestColumn2 INTEGER NOT NULL, PRIMARY KEY (TestColumn1));");
        }

        [Test]
        public override void CanCreateTableWithPrimaryKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithPrimaryKeyExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE TestTable1 (TestColumn1 VARCHAR(255) NOT NULL, TestColumn2 INTEGER NOT NULL, PRIMARY KEY (TestColumn1));");
        }

        [Test]
        public override void CanDropTableWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteTableExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("DROP TABLE TestTable1;");
        }

        [Test]
        public override void CanDropTableWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteTableExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("DROP TABLE TestTable1;");
        }

        [Test]
        public override void CanDropTableIfExistsWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteTableIfExistsExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("IF( EXISTS( SELECT 1 FROM RDB$RELATIONS WHERE (rdb$flags IS NOT NULL) AND LOWER(RDB$RELATION_NAME) = LOWER('TestTable1'))) THEN EXECUTE STATEMENT 'DROP TABLE TestTable1');");
        }

        [Test]
        public void CanDropTableIfExistsWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteTableIfExistsExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("IF( EXISTS( SELECT 1 FROM RDB$RELATIONS WHERE (rdb$flags IS NOT NULL) AND LOWER(RDB$RELATION_NAME) = LOWER('TestTable1'))) THEN EXECUTE STATEMENT 'DROP TABLE TestTable1');");
        }

        [Test]
        public override void CanRenameTableWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetRenameTableExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe(string.Empty);
        }

        [Test]
        public override void CanRenameTableWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetRenameTableExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe(string.Empty);
        }
    }
}
