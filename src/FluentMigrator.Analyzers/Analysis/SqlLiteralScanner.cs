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

using System;

namespace FluentMigrator.Analyzers.Analysis
{
    /// <summary>
    /// What kind of region a position in a SQL statement falls in.
    /// </summary>
    internal enum SqlRegionKind
    {
        /// <summary>Ordinary SQL code.</summary>
        Code,

        /// <summary>A single-quoted string literal, including any prefixed form such as <c>N'...'</c>.</summary>
        StringLiteral,

        /// <summary>A double-quoted run that this dialect treats as a string literal.</summary>
        DoubleQuotedStringLiteral,

        /// <summary>A double-quoted run that this dialect treats as a quoted identifier.</summary>
        QuotedIdentifier,

        /// <summary>
        /// A PostgreSQL dollar-quoted block. These normally hold SQL or PL/pgSQL source rather than
        /// data, so a quoted token inside one is not necessarily a mistake.
        /// </summary>
        DollarQuoted,

        /// <summary>An Oracle alternative-quoted literal, <c>q'[...]'</c>.</summary>
        AlternativeQuoted,

        /// <summary>A line (<c>--</c>) or block (<c>/* */</c>) comment.</summary>
        Comment,
    }

    /// <summary>
    /// The region enclosing a position, and the syntax that opened it.
    /// </summary>
    internal readonly struct SqlRegion
    {
        public SqlRegion(SqlRegionKind kind, int start, int end, string opener)
        {
            Kind = kind;
            Start = start;
            End = end;
            Opener = opener;
        }

        /// <summary>Gets the kind of region.</summary>
        public SqlRegionKind Kind { get; }

        /// <summary>Gets the index at which the region's opening delimiter starts.</summary>
        public int Start { get; }

        /// <summary>Gets the index just past the region's closing delimiter.</summary>
        public int End { get; }

        /// <summary>Gets the index of the first character of the region's content.</summary>
        public int ContentStart => Start + (Opener?.Length ?? 0);

        /// <summary>
        /// Gets the opening delimiter as written, including any prefix - for example <c>'</c>,
        /// <c>N'</c>, <c>E'</c>, <c>_utf8mb4'</c>, <c>q'[</c> or <c>$tag$</c>.
        /// </summary>
        public string Opener { get; }
    }

    /// <summary>
    /// A single-pass scanner that answers one question: which region encloses a given index?
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is deliberately not a SQL parser. It tracks only what is needed to decide whether a
    /// substitution token sits inside a string literal, which means it must model per-dialect escape
    /// behaviour as lexer state - see <see cref="SqlLiteralSyntax"/> for why backslash handling
    /// cannot be deferred to a later phase.
    /// </para>
    /// <para>
    /// Where a construct cannot be resolved from the text alone (SQLite's schema-dependent double
    /// quotes, any session-flippable mode), the scanner reports the region kind it can prove and
    /// leaves the decision about whether to report a diagnostic to the caller.
    /// </para>
    /// </remarks>
    internal static class SqlLiteralScanner
    {
        /// <summary>
        /// Finds the region enclosing <paramref name="index"/>.
        /// </summary>
        /// <param name="sql">The SQL statement text</param>
        /// <param name="index">The index to classify</param>
        /// <param name="syntax">The dialect's literal rules</param>
        /// <returns>The enclosing region</returns>
        public static SqlRegion GetRegionAt(string sql, int index, SqlLiteralSyntax syntax)
        {
            if (sql == null)
            {
                throw new ArgumentNullException(nameof(sql));
            }

            if (syntax == null)
            {
                throw new ArgumentNullException(nameof(syntax));
            }

            var position = 0;
            while (position < sql.Length && position <= index)
            {
                var c = sql[position];

                // --- comments ---------------------------------------------------------------
                if (c == '-' && Peek(sql, position + 1) == '-')
                {
                    var end = EndOfLine(sql, position);
                    if (index < end)
                    {
                        return new SqlRegion(SqlRegionKind.Comment, position, end, "--");
                    }

                    position = end;
                    continue;
                }

                if (c == '/' && Peek(sql, position + 1) == '*')
                {
                    var end = sql.IndexOf("*/", position + 2, StringComparison.Ordinal);
                    end = end < 0 ? sql.Length : end + 2;
                    if (index < end)
                    {
                        return new SqlRegion(SqlRegionKind.Comment, position, end, "/*");
                    }

                    position = end;
                    continue;
                }

                // --- PostgreSQL dollar quoting ----------------------------------------------
                // Must be checked before anything else that could consume a '$', and the closing
                // tag must match the opening tag exactly.
                if (syntax.DollarQuoting && c == '$')
                {
                    var tag = ReadDollarTag(sql, position);
                    if (tag != null)
                    {
                        var bodyStart = position + tag.Length;
                        var close = sql.IndexOf(tag, bodyStart, StringComparison.Ordinal);
                        var bodyEnd = close < 0 ? sql.Length : close;
                        var end = close < 0 ? sql.Length : close + tag.Length;

                        if (index < end)
                        {
                            // A dollar-quoted block holds SQL or PL/pgSQL source, not data, so the
                            // body is scanned as SQL in its own right. A literal nested inside one is
                            // still a literal, and a token inside that is still quoted twice.
                            if (index >= bodyStart && index < bodyEnd)
                            {
                                var body = sql.Substring(bodyStart, bodyEnd - bodyStart);
                                var inner = GetRegionAt(body, index - bodyStart, syntax);

                                if (inner.Kind != SqlRegionKind.Code)
                                {
                                    return new SqlRegion(
                                        inner.Kind,
                                        inner.Start + bodyStart,
                                        inner.End + bodyStart,
                                        inner.Opener);
                                }
                            }

                            return new SqlRegion(SqlRegionKind.DollarQuoted, position, end, tag);
                        }

                        position = end;
                        continue;
                    }
                }

                // --- Oracle alternative quoting: q'[ ... ]' ---------------------------------
                if (syntax.AlternativeQuoting && (c == 'q' || c == 'Q') && Peek(sql, position + 1) == '\'')
                {
                    var delimiter = Peek(sql, position + 2);
                    if (delimiter != '\0')
                    {
                        var closer = MatchingDelimiter(delimiter);
                        var bodyStart = position + 3;
                        var close = IndexOfAlternativeClose(sql, bodyStart, closer);
                        var end = close < 0 ? sql.Length : close + 2;
                        if (index < end)
                        {
                            return new SqlRegion(
                                SqlRegionKind.AlternativeQuoted,
                                position,
                                end,
                                sql.Substring(position, Math.Min(3, sql.Length - position)));
                        }

                        position = end;
                        continue;
                    }
                }

                // --- single-quoted literals, with optional prefix ---------------------------
                if (c == '\'')
                {
                    var prefixStart = ReadLiteralPrefixStart(sql, position);
                    var end = EndOfSingleQuoted(sql, position, syntax);
                    if (index < end)
                    {
                        return new SqlRegion(
                            SqlRegionKind.StringLiteral,
                            prefixStart,
                            end,
                            sql.Substring(prefixStart, position - prefixStart + 1));
                    }

                    position = end;
                    continue;
                }

                // --- double-quoted runs -----------------------------------------------------
                if (c == '"')
                {
                    var end = EndOfDoubleQuoted(sql, position, syntax);
                    if (index < end)
                    {
                        var kind = syntax.DoubleQuoteIsStringLiteral
                            ? SqlRegionKind.DoubleQuotedStringLiteral
                            : SqlRegionKind.QuotedIdentifier;

                        return new SqlRegion(kind, position, end, "\"");
                    }

                    position = end;
                    continue;
                }

                position++;
            }

            return new SqlRegion(SqlRegionKind.Code, index, index, string.Empty);
        }

        private static char Peek(string sql, int index) =>
            index >= 0 && index < sql.Length ? sql[index] : '\0';

        private static int EndOfLine(string sql, int from)
        {
            var newline = sql.IndexOf('\n', from);
            return newline < 0 ? sql.Length : newline + 1;
        }

        /// <summary>
        /// Walks backwards over a literal prefix so the reported opener includes it - <c>N'</c>,
        /// <c>E'</c>, <c>B'</c>, <c>X'</c>, <c>U&amp;'</c>, <c>_utf8mb4'</c>.
        /// </summary>
        private static int ReadLiteralPrefixStart(string sql, int quoteIndex)
        {
            var start = quoteIndex;

            // U&'...' - the ampersand is part of the prefix.
            if (quoteIndex >= 2 && sql[quoteIndex - 1] == '&' && (sql[quoteIndex - 2] == 'U' || sql[quoteIndex - 2] == 'u'))
            {
                return quoteIndex - 2;
            }

            var i = quoteIndex - 1;
            while (i >= 0 && (char.IsLetterOrDigit(sql[i]) || sql[i] == '_'))
            {
                i--;
            }

            var candidateLength = quoteIndex - (i + 1);
            if (candidateLength <= 0)
            {
                return start;
            }

            // Only treat the run as a prefix when it is not preceded by something that would make it
            // an identifier, e.g. `col='x'` has no prefix but `WHERE N'x'` does.
            if (i >= 0 && (char.IsLetterOrDigit(sql[i]) || sql[i] == '_'))
            {
                return start;
            }

            var candidate = sql.Substring(i + 1, candidateLength);
            return IsKnownLiteralPrefix(candidate) ? i + 1 : start;
        }

        private static bool IsKnownLiteralPrefix(string candidate)
        {
            if (candidate.Length == 0)
            {
                return false;
            }

            // MySQL charset introducers: _utf8mb4'...', _latin1'...'
            if (candidate[0] == '_')
            {
                return candidate.Length > 1;
            }

            switch (candidate.ToUpperInvariant())
            {
                case "N":   // national character literal - T-SQL, Oracle, MySQL, standard
                case "E":   // PostgreSQL escape-string literal
                case "B":   // bit-string literal
                case "X":   // hex-string literal
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Finds the index just past the closing quote of a single-quoted literal.
        /// </summary>
        /// <remarks>
        /// Handles both escaping conventions. Doubled quotes (<c>'it''s'</c>) are universal;
        /// backslash escapes (<c>'it\'s'</c>) are MySQL-only and change where the literal ends,
        /// which is why <see cref="SqlLiteralSyntax.BackslashEscapes"/> has to be known here rather
        /// than at parse time.
        /// </remarks>
        private static int EndOfSingleQuoted(string sql, int openIndex, SqlLiteralSyntax syntax)
        {
            var i = openIndex + 1;
            while (i < sql.Length)
            {
                var c = sql[i];

                if (syntax.BackslashEscapes && c == '\\')
                {
                    i += 2;
                    continue;
                }

                if (c == '\'')
                {
                    if (Peek(sql, i + 1) == '\'')
                    {
                        i += 2;
                        continue;
                    }

                    return i + 1;
                }

                i++;
            }

            return sql.Length;
        }

        private static int EndOfDoubleQuoted(string sql, int openIndex, SqlLiteralSyntax syntax)
        {
            var i = openIndex + 1;
            while (i < sql.Length)
            {
                var c = sql[i];

                if (syntax.BackslashEscapes && c == '\\')
                {
                    i += 2;
                    continue;
                }

                if (c == '"')
                {
                    if (Peek(sql, i + 1) == '"')
                    {
                        i += 2;
                        continue;
                    }

                    return i + 1;
                }

                i++;
            }

            return sql.Length;
        }

        /// <summary>
        /// Reads a dollar-quote tag (<c>$$</c> or <c>$tag$</c>) at <paramref name="index"/>.
        /// </summary>
        /// <returns>The tag including both dollar signs, or <see langword="null"/> when this is not a tag.</returns>
        private static string ReadDollarTag(string sql, int index)
        {
            var i = index + 1;
            while (i < sql.Length && (char.IsLetterOrDigit(sql[i]) || sql[i] == '_'))
            {
                i++;
            }

            if (i >= sql.Length || sql[i] != '$')
            {
                return null;
            }

            // A tag must be a valid identifier, so it cannot start with a digit. `$1$` is a
            // positional parameter followed by other syntax, not a dollar quote.
            var tagBody = sql.Substring(index + 1, i - index - 1);
            if (tagBody.Length > 0 && char.IsDigit(tagBody[0]))
            {
                return null;
            }

            return sql.Substring(index, i - index + 1);
        }

        private static char MatchingDelimiter(char opener)
        {
            switch (opener)
            {
                case '[': return ']';
                case '{': return '}';
                case '(': return ')';
                case '<': return '>';
                default: return opener;
            }
        }

        private static int IndexOfAlternativeClose(string sql, int from, char closer)
        {
            for (var i = from; i < sql.Length - 1; i++)
            {
                if (sql[i] == closer && sql[i + 1] == '\'')
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
