#region License
// Copyright (c) 2024, Fluent Migrator Project
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

namespace FluentMigrator.Analyzers.Analysis
{
    /// <summary>
    /// The string-literal lexing rules of a <see cref="SqlDialect"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// These are deliberately modelled as <em>lexer modes</em> rather than as parse-time concerns.
    /// Backslash handling in particular cannot be deferred, because it changes where a literal
    /// <em>ends</em>: <c>'\''</c> is one complete literal in MySQL's default mode and an unterminated
    /// literal in PostgreSQL with <c>standard_conforming_strings</c> on.
    /// </para>
    /// <para>
    /// Session-level settings that flip these modes (<c>NO_BACKSLASH_ESCAPES</c>, <c>ANSI_QUOTES</c>,
    /// <c>QUOTED_IDENTIFIER</c>, <c>standard_conforming_strings</c>, <c>SQLITE_DQS</c>) are not
    /// visible to a compile-time analyzer. Each mode below therefore encodes the <em>default</em> for
    /// the dialect, and any diagnostic that depends on a flippable mode is reported conservatively.
    /// </para>
    /// </remarks>
    internal sealed class SqlLiteralSyntax
    {
        private SqlLiteralSyntax(
            SqlDialect dialect,
            bool backslashEscapes,
            bool doubleQuoteIsStringLiteral,
            bool doubleQuoteIsAmbiguous,
            bool dollarQuoting,
            bool alternativeQuoting)
        {
            Dialect = dialect;
            BackslashEscapes = backslashEscapes;
            DoubleQuoteIsStringLiteral = doubleQuoteIsStringLiteral;
            DoubleQuoteIsAmbiguous = doubleQuoteIsAmbiguous;
            DollarQuoting = dollarQuoting;
            AlternativeQuoting = alternativeQuoting;
        }

        /// <summary>
        /// Gets the dialect these rules describe.
        /// </summary>
        public SqlDialect Dialect { get; }

        /// <summary>
        /// Gets a value indicating whether a backslash escapes the next character inside a
        /// single-quoted literal. MySQL only, and only without <c>NO_BACKSLASH_ESCAPES</c>.
        /// </summary>
        public bool BackslashEscapes { get; }

        /// <summary>
        /// Gets a value indicating whether a double-quoted run is a string literal rather than a
        /// quoted identifier. MySQL only, and only without <c>ANSI_QUOTES</c>.
        /// </summary>
        public bool DoubleQuoteIsStringLiteral { get; }

        /// <summary>
        /// Gets a value indicating whether a double-quoted run may be either an identifier or a
        /// string literal depending on whether it resolves to an identifier.
        /// </summary>
        /// <remarks>
        /// SQLite's documented misfeature: <c>"foo"</c> is an identifier unless no such identifier
        /// exists, in which case it silently degrades to a string literal. Optionally disabled with
        /// <c>SQLITE_DQS</c>. Because the outcome depends on the schema rather than on the text, a
        /// double-quoted run is never reported for SQLite.
        /// </remarks>
        public bool DoubleQuoteIsAmbiguous { get; }

        /// <summary>
        /// Gets a value indicating whether <c>$$text$$</c> / <c>$tag$text$tag$</c> dollar quoting is
        /// supported. PostgreSQL only.
        /// </summary>
        public bool DollarQuoting { get; }

        /// <summary>
        /// Gets a value indicating whether Oracle's alternative quoting mechanism
        /// (<c>q'[text]'</c>, <c>q'!text!'</c>) is supported.
        /// </summary>
        public bool AlternativeQuoting { get; }

        /// <summary>
        /// Gets the literal rules for a dialect.
        /// </summary>
        /// <param name="dialect">The dialect</param>
        /// <returns>The rules</returns>
        /// <remarks>
        /// <see cref="SqlDialect.Unknown"/> maps to the conservative intersection: single quotes
        /// only, doubled-quote escaping, no backslash escapes, and no dialect-specific forms. Every
        /// dialect agrees on that core, so diagnostics derived from it need no dialect signal.
        /// </remarks>
        public static SqlLiteralSyntax For(SqlDialect dialect)
        {
            switch (dialect)
            {
                case SqlDialect.MySql:
                    return new SqlLiteralSyntax(
                        dialect,
                        backslashEscapes: true,
                        doubleQuoteIsStringLiteral: true,
                        doubleQuoteIsAmbiguous: false,
                        dollarQuoting: false,
                        alternativeQuoting: false);

                case SqlDialect.Postgres:
                    return new SqlLiteralSyntax(
                        dialect,
                        backslashEscapes: false,
                        doubleQuoteIsStringLiteral: false,
                        doubleQuoteIsAmbiguous: false,
                        dollarQuoting: true,
                        alternativeQuoting: false);

                case SqlDialect.Oracle:
                    return new SqlLiteralSyntax(
                        dialect,
                        backslashEscapes: false,
                        doubleQuoteIsStringLiteral: false,
                        doubleQuoteIsAmbiguous: false,
                        dollarQuoting: false,
                        alternativeQuoting: true);

                case SqlDialect.SQLite:
                    return new SqlLiteralSyntax(
                        dialect,
                        backslashEscapes: false,
                        doubleQuoteIsStringLiteral: false,
                        doubleQuoteIsAmbiguous: true,
                        dollarQuoting: false,
                        alternativeQuoting: false);

                default:
                    // SqlServer, Redshift, Firebird, Db2, Hana, Snowflake and Unknown all share the
                    // standard-SQL core: single quotes, doubled-quote escaping, no backslash escapes.
                    return new SqlLiteralSyntax(
                        dialect,
                        backslashEscapes: false,
                        doubleQuoteIsStringLiteral: false,
                        doubleQuoteIsAmbiguous: false,
                        dollarQuoting: false,
                        alternativeQuoting: false);
            }
        }
    }
}
