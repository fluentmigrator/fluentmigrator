#region License
// Copyright (c) 2007-2024, Fluent Migrator Project
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

using System;
using System.Collections.Generic;

using FluentMigrator.Runner.Generators.Generic;
using FluentMigrator.Runner.Processors.Firebird;

namespace FluentMigrator.Runner.Generators.Firebird
{
    /// <summary>
    /// The Firebird SQL quoter for FluentMigrator.
    /// </summary>
    public class FirebirdQuoter : GenericQuoter
    {
        // see http://www.firebirdsql.org/refdocs/langrefupd25-reskeywords-full-keywords.html
        private static readonly HashSet<string> _keywords = new HashSet<string>(
            new[] { "!<", "^<", "^=", "^>", ",", ":=", "!=", "!>", "(", ")", "<", "<=", "<>", "=", ">", ">=", "||", "~<", "~=", "~>",
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
            "GRANT", "GRANTED", "GROUP", "HASH", "HAVING", "HOUR", "IF", "IGNORE", "IIF", "IN", "INACTIVE", "INDEX", "INNER", "INPUT_TYPE",
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
            "WRITE", "YEAR", "YEARDAY" },
            StringComparer.OrdinalIgnoreCase);

        private readonly bool _forceQuote;

        /// <inheritdoc />
        public FirebirdQuoter(FirebirdOptions options)
            : this(options.ForceQuote)
        {
        }

        /// <inheritdoc />
        public FirebirdQuoter(bool forceQuote)
        {
            _forceQuote = forceQuote;
        }

        /// <inheritdoc />
        protected override bool ShouldQuote(string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;
            if (_forceQuote)
                return true;
            if (_keywords.Contains(name.ToUpperInvariant()))
                return true;
            if (name.StartsWith("_"))
                return true;
            if (name.IndexOf("'", StringComparison.OrdinalIgnoreCase) != -1)
                return true;
            return false;
        }

        /// <inheritdoc />
        public override string FormatSystemMethods(SystemMethods value)
        {
            switch (value)
            {
                case SystemMethods.NewSequentialId:
                case SystemMethods.NewGuid:
                    return "gen_uuid()";
                /*
                 Firebird v.3.0.4: Context variables LOCALTIME and LOCALTIMESTAMP were added as synonyms for CURRENT_TIME and CURRENT_TIMESTAMP, respectively, in anticipation of CURRENT_TIME and CURRENT_TIMESTAMP being redefined in Firebird 4 to represent the time and timestamp in UTC time, to comply with the SQL standards.
                 Source: https://firebirdsql.org/file/documentation/release_notes/html/en/3_0/bk02ch09s06.html


                 Source: https://github.com/FirebirdSQL/firebird/blob/master/doc/sql.extensions/README.time_zone.md#changes-in-current_time-and-current_timestamp
                 */
                case SystemMethods.CurrentDateTime:
                    return "LOCALTIMESTAMP";
                case SystemMethods.CurrentDateTimeOffset:
                    throw new ArgumentException($"{nameof(SystemMethods.CurrentDateTimeOffset)} not supported.");
                case SystemMethods.CurrentUTCDateTime:
                    return "CURRENT_TIMESTAMP";
                case SystemMethods.CurrentUser:
                    return "CURRENT_USER";
            }

            return base.FormatSystemMethods(value);
        }

        /// <inheritdoc />
        public override string FormatDateTime(DateTime value)
        {
            return ValueQuote + (value).ToString("yyyy-MM-dd HH:mm:ss") + ValueQuote;
        }

        /// <inheritdoc />
        public string ToFbObjectName(string objName)
        {
            if (IsQuoted(objName))
                return UnQuote(objName);
            var potentiallyQuoted = Quote(objName);
            if (IsQuoted(potentiallyQuoted))
                return objName;
            return objName.ToUpper();
        }

        /// <inheritdoc />
        public override string QuoteSchemaName(string schemaName)
        {
            // This database doesn't support schemata
            return string.Empty;
        }
    }
}
