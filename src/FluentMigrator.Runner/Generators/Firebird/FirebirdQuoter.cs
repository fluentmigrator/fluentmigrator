using System;
using System.Collections.Generic;
using System.Linq;
using FluentMigrator.Runner.Generators.Generic;

namespace FluentMigrator.Runner.Generators.Firebird
{
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

        public override string Quote(string name)
        {
            if (_keywords.Contains(name))
                return base.Quote(name);
            if (name.StartsWith("_"))
                return base.Quote(name);
            return name;
        }

        public override string FormatDateTime(DateTime value)
        {
            return ValueQuote + (value).ToString("yyyy-MM-dd HH:mm:ss") + ValueQuote;
        }

        public string ToFbObjectName(string objName)
        {
            if (IsQuoted(objName))
                return UnQuote(objName);
            var potentiallyQuoted = Quote(objName);
            if (IsQuoted(potentiallyQuoted))
                return objName;
            else
                return objName.ToUpper();
        }
    }
}