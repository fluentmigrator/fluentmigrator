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

using FluentMigrator.Exceptions;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Generators.Oracle;
using FluentMigrator.Runner.Processors.Oracle;
using NUnit.Framework;
using Shouldly;

namespace FluentMigrator.Tests.Unit.Generators.Oracle
{
    [TestFixture]
    public class OracleTableTests : OracleBaseTableTests<OracleGenerator>
    {
        [Test]
        public override void CanCreateTableWithIdentityWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithAutoIncrementExpression();
            expression.SchemaName = "TestSchema";

            Assert.Throws<DatabaseOperationNotSupportedException>(() => Generator.Generate(expression));
        }

        [Test]
        public override void CanCreateTableWithIdentityWithDefaultSchema()
        {
            Assert.Throws<DatabaseOperationNotSupportedException>(() => Generator.Generate(GeneratorTestHelper.GetCreateTableWithAutoIncrementExpression()));
        }

        [TestCase(CompatibilityMode.LOOSE)]
        [TestCase(CompatibilityMode.STRICT)]
        public void CanCreateTableIfNotExistsWithDefaultSchema(CompatibilityMode compatibilityMode)
        {
            Generator.CompatibilityMode = compatibilityMode;
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            expression.IfNotExists = true;

            var result = Generator.Generate(expression);
            result.ShouldBe(
                "BEGIN" + Environment.NewLine +
                "FOR missing_table IN (SELECT 1 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM ALL_TABLES WHERE OWNER = SYS_CONTEXT('USERENV', 'CURRENT_SCHEMA') AND TABLE_NAME = 'TESTTABLE1')) LOOP" + Environment.NewLine +
                "EXECUTE IMMEDIATE 'CREATE TABLE TestTable1 (TestColumn1 NVARCHAR2(255) NOT NULL, TestColumn2 NUMBER(10,0) NOT NULL)';" + Environment.NewLine +
                "END LOOP;" + Environment.NewLine +
                "END;");
        }

        [Test]
        public void CanCreateTableIfNotExistsWithIndexAndForeignKey()
        {
            var expression = GeneratorTestHelper.GetCreateTableIfNotExistsWithIndexAndForeignKey();

            var result = Generator.Generate(expression);

            // In the loop with the table, so they are created only with it.
            result.ShouldBe(
                "BEGIN" + Environment.NewLine +
                "FOR missing_table IN (SELECT 1 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM ALL_TABLES WHERE OWNER = SYS_CONTEXT('USERENV', 'CURRENT_SCHEMA') AND TABLE_NAME = 'TESTTABLE1')) LOOP" + Environment.NewLine +
                "EXECUTE IMMEDIATE 'CREATE TABLE TestTable1 (TestColumn1 NVARCHAR2(255) NOT NULL, TestColumn2 NUMBER(10,0) NOT NULL)';" + Environment.NewLine +
                "EXECUTE IMMEDIATE 'CREATE UNIQUE INDEX UX_TestTable1_TestColumn1 ON TestTable1 (TestColumn1 ASC)';" + Environment.NewLine +
                "EXECUTE IMMEDIATE 'ALTER TABLE TestTable1 ADD CONSTRAINT FK_TestTable1_TestTable2 FOREIGN KEY (TestColumn2) REFERENCES TestTable2 (TestColumn2)';" + Environment.NewLine +
                "END LOOP;" + Environment.NewLine +
                "END;");
        }

        [Test]
        public void CanCreateTableIfNotExistsWithCustomSchemaAndDescriptions()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithTableDescriptionAndColumnDescriptions();
            expression.IfNotExists = true;
            expression.SchemaName = "TestSchema";
            expression.TableDescription = "It's a table";

            var result = Generator.Generate(expression);

            // The descriptions run in the loop, so only when the table is created. Quotes are doubled once for the
            // COMMENT literal and once more for EXECUTE IMMEDIATE.
            result.ShouldBe(
                "BEGIN" + Environment.NewLine +
                "FOR missing_table IN (SELECT 1 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM ALL_TABLES WHERE OWNER = 'TESTSCHEMA' AND TABLE_NAME = 'TESTTABLE1')) LOOP" + Environment.NewLine +
                "EXECUTE IMMEDIATE 'CREATE TABLE TestSchema.TestTable1 (TestColumn1 NVARCHAR2(255), TestColumn2 NUMBER(10,0) NOT NULL)';" + Environment.NewLine +
                "EXECUTE IMMEDIATE 'COMMENT ON TABLE TestSchema.TestTable1 IS ''It''''s a table''';" + Environment.NewLine +
                "EXECUTE IMMEDIATE 'COMMENT ON COLUMN TestSchema.TestTable1.TestColumn1 IS ''TestColumn1Description''';" + Environment.NewLine +
                "EXECUTE IMMEDIATE 'COMMENT ON COLUMN TestSchema.TestTable1.TestColumn2 IS ''TestColumn2Description''';" + Environment.NewLine +
                "END LOOP;" + Environment.NewLine +
                "END;");
        }

        [TestCase("Users", false, "USERS")]
        [TestCase("\"Users\"", false, "Users")]
        [TestCase("Users", true, "Users")]
        [TestCase("Order", false, "Order")]
        public void CreateTableIfNotExistsLooksForTheNameAsOracleStoresIt(string tableName, bool useQuotedIdentifiers, string storedName)
        {
            // Unquoted names are stored in upper case, quoted ones as written, and USERS and "Users" are different
            // tables, so the guard compares the stored name exactly. Order is a keyword, which the quoter quotes.
            var generator = new OracleGenerator(useQuotedIdentifiers);
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            expression.TableName = tableName;
            expression.SchemaName = tableName;
            expression.IfNotExists = true;

            var result = generator.Generate(expression);

            result.ShouldContain($"WHERE OWNER = '{storedName}' AND TABLE_NAME = '{storedName}'");
        }

        [Test]
        public void CreateTableIfNotExistsIsOneStatementForTheScriptSplitter()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithTableDescriptionAndColumnDescriptions();
            expression.IfNotExists = true;

            var script = Generator.Generate(expression) + Environment.NewLine + "SELECT 1 FROM DUAL;";

            var statements = OracleSqlParser.SplitOracleSqlStatements(script);

            statements.Count.ShouldBe(2);
            statements[0].ShouldStartWith("BEGIN");
            statements[0].ShouldEndWith("END;");
        }
    }
}
