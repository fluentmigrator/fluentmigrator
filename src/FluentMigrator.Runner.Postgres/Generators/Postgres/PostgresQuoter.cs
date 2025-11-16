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

using System;
using System.Collections.Generic;
using System.Linq;

using FluentMigrator.Runner.Generators.Generic;
using FluentMigrator.Runner.Processors.Postgres;

namespace FluentMigrator.Runner.Generators.Postgres
{
    /// <summary>
    /// The PostgreSQL SQL quoter for FluentMigrator.
    /// </summary>
    public class PostgresQuoter : GenericQuoter
    {
        /// <inheritdoc />
        public PostgresQuoter(PostgresOptions options)
        {
            Options = options ?? new PostgresOptions();
        }

        /// <summary>
        /// https://www.postgresql.org/docs/current/sql-keywords-appendix.html
        /// <para>select * from pg_get_keywords() where catcode != 'U'</para>
        /// </summary>
        private static readonly HashSet<string> _keywords = new HashSet<string>(
            new[] { "all",  "analyse",  "analyze",  "and",  "any",  "array",  "as",  "asc",  "asymmetric",  "authorization",
                "between",  "bigint",  "binary", "bit",  "boolean",  "both", "case",  "cast",  "char",  "character", "check",
                "coalesce", "collate", "collation", "column", "concurrently", "constraint", "create", "cross", "current_catalog",
                "current_date", "current_role", "current_schema", "current_time", "current_timestamp", "current_user", "dec",
                "decimal", "default", "deferrable", "desc", "distinct", "do", "else", "end", "except", "exists", "extract", "FALSE",
                "fetch", "float", "for", "foreign", "freeze", "from", "full", "grant", "greatest", "group", "grouping", "having", "ilike",
                "in", "initially", "inner", "inout", "int", "integer", "intersect", "interval", "into", "is", "isnull", "join", "lateral",
                "leading", "least", "left", "like", "limit", "localtime", "localtimestamp", "national", "natural", "nchar", "none",
                "not", "notnull", "null", "nullif", "numeric", "offset", "on", "only", "or", "order", "out", "outer", "overlaps", "overlay",
                "placing", "position", "precision", "primary", "real", "references", "returning", "right", "row", "select", "session_user",
                "setof", "similar", "smallint", "some", "substring", "symmetric", "table", "tablesample", "then", "time", "timestamp", "to",
                "trailing", "treat", "trim", "TRUE", "union", "unique", "user", "using", "values", "varchar", "variadic", "verbose", "when",
                "where", "window", "with", "xmlattributes", "xmlconcat", "xmlelement", "xmlexists", "xmlforest", "xmlnamespaces", "xmlparse",
                "xmlpi", "xmlroot", "xmlserialize", "xmltable" }, StringComparer.OrdinalIgnoreCase);

        /// <inheritdoc />
        public PostgresOptions Options { get; }

        /// <inheritdoc />
        protected override bool ShouldQuote(string name)
        {
            return Options.ForceQuote ? base.ShouldQuote(name) : _keywords.Contains(name);
        }

        /// <inheritdoc />
        public override string FormatBool(bool value) { return value ? "true" : "false"; }

        /// <inheritdoc />
        public override string QuoteSchemaName(string schemaName)
        {
            if (string.IsNullOrEmpty(schemaName))
                schemaName = "public";
            return base.QuoteSchemaName(schemaName);
        }

        /// <inheritdoc />
        public override string QuoteSequenceName(string sequenceName, string schemaName)
        {
            return CreateSchemaPrefixedQuotedIdentifier(
                string.IsNullOrEmpty(schemaName) ? string.Empty : Quote(schemaName),
                IsQuoted(sequenceName) ? sequenceName : Quote(sequenceName));
        }

        /// <inheritdoc />
        protected override string FormatByteArray(byte[] array)
        {
            var arrayAsHex = array.Select(b => b.ToString("X2")).ToArray();
            return @"E'\\x" + string.Concat(arrayAsHex) + "'";
        }

        /// <inheritdoc />
        public string UnQuoteSchemaName(string quoted)
        {
            if (string.IsNullOrEmpty(quoted))
                return "public";

            return UnQuote(quoted);
        }

        /// <inheritdoc />
        public override string FormatSystemMethods(SystemMethods value)
        {
            switch (value)
            {
                case SystemMethods.NewGuid:
                    //need to run the script share/contrib/uuid-ossp.sql to install the uuid_generate4 function
                    return "uuid_generate_v4()";
                case SystemMethods.NewSequentialId:
                    return "uuid_generate_v1()";
                case SystemMethods.CurrentDateTime:
                    return "now()";
                case SystemMethods.CurrentUTCDateTime:
                    return "(now() at time zone 'UTC')";
                case SystemMethods.CurrentDateTimeOffset:
                    return "current_timestamp";
                case SystemMethods.CurrentUser:
                    return "current_user";
            }

            return base.FormatSystemMethods(value);
        }
    }
}
