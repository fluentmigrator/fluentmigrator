﻿using System;
using FluentMigrator.Runner.Generators.Firebird;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.Firebird
{
    [TestFixture]
    public class FirebirdQuoterTests
    {
        private readonly string[] _fbKeywords = new[]
        {
            "!<", "^<", "^=", "^>", ",", ":=", "!=", "!>", "(", ")", "<", "<=", "<>", "=", ">", ">=", "||", "~<", "~=", "~>",
            "ABS", "ACCENT", "ACOS", "ACTION", "ACTIVE", "ADD", "ADMIN", "AFTER", "ALL", "ALTER", "ALWAYS", "AND", "ANY", 
            "AS", "ASC", "ASCENDING", "ASCII_CHAR", "ASCII_VAL", "ASIN", "AT", "ATAN", "ATAN2", "AUTO", "AUTONOMOUS", "AVG",
            "BACKUP", "BEFORE", "BEGIN", "BETWEEN", "BIGINT", "BIN_AND", "BIN_NOT", "BIN_OR", "BIN_SHL", "BIN_SHR", "BIN_XOR",
            "BIT_LENGTH", "BLOB", "BLOCK", "BOTH", "BREAK", "BY", "CALLER", "CASCADE", "CASE", "CAST", "CEIL", "CEILING", "CHAR",
            "CHAR_LENGTH", "CHAR_TO_UUID", "CHARACTER", "CHARACTER_LENGTH", "CHECK", "CLOSE", "COALESCE", "COLLATE", "COLLATION",
            "COLUMN", "COMMENT", "COMMIT", "COMMITTED", "COMMON", "COMPUTED", "CONDITIONAL", "CONNECT", "CONSTRAINT", "CONTAINING",
            "COS", "COSH", "COT", "COUNT", "CREATE", "CROSS", "CSTRING", "CURRENT", "CURRENT_CONNECTION", "CURRENT_DATE", "CURRENT_ROLE",
            "CURRENT_TIME", "CURRENT_TIMESTAMP", "CURRENT_TRANSACTION", "CURRENT_USER", "CURSOR", "DATA", "DATABASE", "DATE", "DATEADD",
            "DATEDIFF", "DAY", "DEC", "DECIMAL", "DECLARE", "DECODE", "DEFAULT", "DELETE", "DELETING", "DESC", "DESCENDING", "DESCRIPTOR",
            "DIFFERENCE", "DISCONNECT", "DISTINCT", "DO", "DOMAIN", "DOUBLE", "DROP", "ELSE", "END", "ENTRY_POINT", "ESCAPE", "EXCEPTION",
            "EXECUTE", "EXISTS", "EXIT", "EXP", "EXTERNAL", "EXTRACT", "FETCH", "FILE", "FILTER", "FIRST", "FIRSTNAME", "FLOAT", "FLOOR",
            "FOR", "FOREIGN", "FREE_IT", "FROM", "FULL", "FUNCTION", "GDSCODE", "GEN_ID", "GEN_UUID", "GENERATED", "GENERATOR", "GLOBAL",
            "GRANT", "GRANTED", "GROUP", "HASH", "HAVING", "HOUR", "IF", "IGNORE", "IIF", "IN",  "INACTIVE", "INDEX", "INNER", "INPUT_TYPE",
            "INSENSITIVE", "INSERT", "INSERTING", "INT", "INTEGER", "INTO", "IS", "ISOLATION", "JOIN", "KEY", "LAST", "LASTNAME", "LEADING",
            "LEAVE", "LEFT", "LENGTH", "LEVEL", "LIKE", "LIMBO", "LIST", "LN", "LOCK", "LOG", "LOG10", "LONG", "LOWER", "LPAD", "MANUAL", 
            "MAPPING", "MATCHED", "MATCHING", "MAX", "MAXIMUM_SEGMENT", "MAXVALUE", "MERGE", "MIDDLENAME", "MILLISECOND", "MIN", "MINUTE",
            "MINVALUE", "MOD", "MODULE_NAME", "MONTH", "NAMES", "NATIONAL", "NATURAL", "NCHAR", "NEXT", "NO", "NOT", "NULL", "NULLIF", "NULLS",
            "NUMERIC", "OCTET_LENGTH", "OF", "ON", "ONLY", "OPEN", "OPTION", "OR", "ORDER", "OS_NAME", "OUTER", "OUTPUT_TYPE", "OVERFLOW", 
            "OVERLAY", "PAD", "PAGE", "PAGE_SIZE", "PAGES", "PARAMETER", "PASSWORD", "PI", "PLACING", "PLAN", "POSITION", "POST_EVENT", "POWER",
            "PRECISION", "PRESERVE", "PRIMARY", "PRIVILEGES", "PROCEDURE", "PROTECTED", "RAND", "RDB$DB_KEY", "READ", "REAL", "RECORD_VERSION",
            "RECREATE", "RECURSIVE", "REFERENCES", "RELEASE", "REPLACE", "REQUESTS", "RESERV", "RESERVING", "RESTART", "RESTRICT", "RETAIN",
            "RETURNING", "RETURNING_VALUES", "RETURNS", "REVERSE", "REVOKE", "RIGHT", "ROLE", "ROLLBACK", "ROUND", "ROW_COUNT", "ROWS",
            "RPAD", "SAVEPOINT", "SCALAR_ARRAY", "SCHEMA", "SECOND", "SEGMENT", "SELECT", "SENSITIVE", "SEQUENCE", "SET", "SHADOW", "SHARED",
            "SIGN", "SIMILAR", "SIN", "SINGULAR", "SINH", "SIZE", "SKIP", "SMALLINT", "SNAPSHOT", "SOME", "SORT", "SOURCE", "SPACE", "SQLCODE",
            "SQLSTATE", "SQRT", "STABILITY", "START", "STARTING", "STARTS", "STATEMENT", "STATISTICS", "SUB_TYPE", "SUBSTRING", "SUM", "SUSPEND",
            "TABLE", "TAN", "TANH", "TEMPORARY", "THEN", "TIME", "TIMEOUT", "TIMESTAMP", "TO", "TRAILING", "TRANSACTION", "TRIGGER", "TRIM", 
            "TRUNC", "TWO_PHASE", "TYPE", "UNCOMMITTED", "UNDO", "UNION", "UNIQUE", "UPDATE", "UPDATING", "UPPER", "USER", "USING", "UUID_TO_CHAR",
            "VALUE", "VALUES", "VARCHAR", "VARIABLE", "VARYING", "VIEW", "WAIT", "WEEK", "WEEKDAY", "WHEN", "WHERE", "WHILE", "WITH", "WORK",
            "WRITE", "YEAR", "YEARDAY"
        };

        [Test, TestCaseSource("_fbKeywords")]
        public void Quote_ArgIsFirebirdKeyword_ArgShouldBeQuoted(string quoteArg)
        {
            var actual = new FirebirdQuoter().Quote(quoteArg);
            var expected = String.Format("\"{0}\"", quoteArg);
            actual.ShouldBe(expected);
        }

        [TestCase("one")]
        [TestCase("silly")]
        public void Quote_ArgIsNotAKeyWord_ArgShouldNotBeQuoted(string quoteArg)
        {
            var actual = new FirebirdQuoter().Quote(quoteArg);
            actual.ShouldBe(quoteArg);
        }

        [TestCase("_test")]
        public void Quote_ArgBeginsWithUnderscore_ArgShouldBeQuoted(string quoteArg)
        {
            var actual = new FirebirdQuoter().Quote(quoteArg);
            actual.ShouldBe(string.Format("\"{0}\"", quoteArg));
        }

        [Test, SetCulture("tr-TR")]
        public void Quote_PassesTurkishTest()
        {
            var actual = new FirebirdQuoter().Quote("similar");
            actual.ShouldBe("\"similar\"");
        }

        [Test, TestCaseSource("_fbKeywords")]
        public void Quote_ArgIsKeywordInLowercase_ArgShouldBeQuoted(string quoteArg)
        {
            var argInLowerCase = quoteArg.ToLower();
            var actual = new FirebirdQuoter().Quote(argInLowerCase);
            var expected = String.Format("\"{0}\"", argInLowerCase);
            actual.ShouldBe(expected);
        }
    }
}
