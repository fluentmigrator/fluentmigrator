#region License
// Copyright (c) 2018, Fluent Migrator Project
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System.Linq;

using FluentMigrator.Runner.Processors.Oracle;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Processors.Oracle
{
    [TestFixture]
    [Category("Oracle")]
    public class OracleSqlParserTests
    {
        [Test]
        public void SimpleSqlStatement_ShouldBeSplitCorrectly()
        {
            var sql = "SELECT * FROM users;";
            var result = OracleSqlParser.SplitOracleSqlStatements(sql);

            result.Count.ShouldBe(1);
            result[0].ShouldBe("SELECT * FROM users");
        }

        [Test]
        public void MultipleSqlStatements_ShouldBeSplitCorrectly()
        {
            var sql = "SELECT * FROM users; SELECT * FROM orders;";
            var result = OracleSqlParser.SplitOracleSqlStatements(sql);

            result.Count.ShouldBe(2);
            result[0].ShouldBe("SELECT * FROM users");
            result[1].ShouldBe("SELECT * FROM orders");
        }

        [Test]
        public void BeginEndBlock_ShouldNotBeSplit()
        {
            var sql = "BEGIN\r\n    DBMS_OUTPUT.NEW_LINE;\r\nEND;";
            var result = OracleSqlParser.SplitOracleSqlStatements(sql);

            result.Count.ShouldBe(1);
            result[0].ShouldBe("BEGIN\r\n    DBMS_OUTPUT.NEW_LINE;\r\nEND;");
        }

        [Test]
        public void BeginEndBlock_WithMultipleStatements_ShouldNotBeSplit()
        {
            var sql = @"BEGIN
    EXECUTE IMMEDIATE 'CREATE TABLE test (id INT)';
    EXECUTE IMMEDIATE 'INSERT INTO test VALUES (1)';
END;";
            var result = OracleSqlParser.SplitOracleSqlStatements(sql);

            result.Count.ShouldBe(1);
            result[0].ShouldBe(sql);
        }

        [Test]
        public void DeclareBlock_ShouldNotBeSplit()
        {
            var sql = @"DECLARE
    v_count NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_count FROM users;
    DBMS_OUTPUT.PUT_LINE(v_count);
END;";
            var result = OracleSqlParser.SplitOracleSqlStatements(sql);

            result.Count.ShouldBe(1);
            result[0].ShouldBe(sql);
        }

        [Test]
        public void MixedStatements_BeginEndAndSimple_ShouldBeSplitCorrectly()
        {
            // Use explicit newlines to avoid cross-platform issues
            var newLine = "\n";
            var sql = $"CREATE TABLE test (id INT);{newLine}BEGIN{newLine}    DBMS_OUTPUT.NEW_LINE;{newLine}END;{newLine}SELECT * FROM test;";
            var result = OracleSqlParser.SplitOracleSqlStatements(sql);

            result.Count.ShouldBe(3);
            result[0].ShouldBe("CREATE TABLE test (id INT)");
            result[1].ShouldBe($"BEGIN{newLine}    DBMS_OUTPUT.NEW_LINE;{newLine}END;");
            result[2].ShouldBe("SELECT * FROM test");
        }

        [Test]
        public void NestedBeginEnd_ShouldNotBeSplit()
        {
            var sql = @"BEGIN
    BEGIN
        DBMS_OUTPUT.PUT_LINE('Inner');
    END;
    DBMS_OUTPUT.PUT_LINE('Outer');
END;";
            var result = OracleSqlParser.SplitOracleSqlStatements(sql);

            result.Count.ShouldBe(1);
            result[0].ShouldBe(sql);
        }

        [Test]
        public void BeginInString_ShouldBeIgnored()
        {
            var sql = "SELECT 'BEGIN' FROM dual; SELECT 'END' FROM dual;";
            var result = OracleSqlParser.SplitOracleSqlStatements(sql);

            result.Count.ShouldBe(2);
            result[0].ShouldBe("SELECT 'BEGIN' FROM dual");
            result[1].ShouldBe("SELECT 'END' FROM dual");
        }

        [Test]
        public void BeginInComment_ShouldBeIgnored()
        {
            var sql = "SELECT * FROM users; -- BEGIN\r\nSELECT * FROM orders;";
            var result = OracleSqlParser.SplitOracleSqlStatements(sql);

            result.Count.ShouldBe(2);
            result[0].ShouldBe("SELECT * FROM users");
            result[1].ShouldBe("-- BEGIN\r\nSELECT * FROM orders");
        }

        [Test]
        public void CreateProcedure_ShouldNotBeSplit()
        {
            var sql = @"CREATE OR REPLACE PROCEDURE test_proc IS
BEGIN
    DBMS_OUTPUT.PUT_LINE('Test');
END test_proc;";
            var result = OracleSqlParser.SplitOracleSqlStatements(sql);

            result.Count.ShouldBe(1);
            result[0].ShouldBe(sql);
        }

        [Test]
        public void CreateFunction_ShouldNotBeSplit()
        {
            var sql = @"CREATE OR REPLACE FUNCTION test_func RETURN NUMBER IS
BEGIN
    RETURN 1;
END test_func;";
            var result = OracleSqlParser.SplitOracleSqlStatements(sql);

            result.Count.ShouldBe(1);
            result[0].ShouldBe(sql);
        }

        [Test]
        public void MultiLineCommentWithSemicolon_ShouldBeIgnored()
        {
            var sql = "SELECT * FROM users /* ; */; SELECT * FROM orders;";
            var result = OracleSqlParser.SplitOracleSqlStatements(sql);

            result.Count.ShouldBe(2);
            result[0].ShouldBe("SELECT * FROM users /* ; */");
            result[1].ShouldBe("SELECT * FROM orders");
        }

        [Test]
        public void EmptyStatement_ShouldBeIgnored()
        {
            var sql = ";;SELECT * FROM users;";
            var result = OracleSqlParser.SplitOracleSqlStatements(sql);

            result.Where(s => !string.IsNullOrWhiteSpace(s)).Count().ShouldBe(1);
        }

        [Test]
        public void SqlWithoutTrailingSemicolon_ShouldBeIncluded()
        {
            var sql = "SELECT * FROM users";
            var result = OracleSqlParser.SplitOracleSqlStatements(sql);

            result.Count.ShouldBe(1);
            result[0].ShouldBe("SELECT * FROM users");
        }

        [Test]
        public void ComplexPlSqlBlock_WithIfElse_ShouldNotBeSplit()
        {
            var sql = @"BEGIN
    IF 1 = 1 THEN
        EXECUTE IMMEDIATE 'DROP TABLE test';
    ELSE
        EXECUTE IMMEDIATE 'CREATE TABLE test (id INT)';
    END IF;
END;";
            var result = OracleSqlParser.SplitOracleSqlStatements(sql);

            result.Count.ShouldBe(1);
            result[0].ShouldBe(sql);
        }

        [Test]
        public void AnonymousBlockWithExceptionHandler_ShouldNotBeSplit()
        {
            var sql = @"BEGIN
    EXECUTE IMMEDIATE 'DROP TABLE test';
EXCEPTION
    WHEN OTHERS THEN
        NULL;
END;";
            var result = OracleSqlParser.SplitOracleSqlStatements(sql);

            result.Count.ShouldBe(1);
            result[0].ShouldBe(sql);
        }
    }
}
