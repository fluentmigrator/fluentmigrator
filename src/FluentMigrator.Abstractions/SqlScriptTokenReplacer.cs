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
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

using FluentMigrator.Generation;

namespace FluentMigrator
{
    /// <summary>
    /// Function to replace token in an SQL script
    /// </summary>
    public static class SqlScriptTokenReplacer
    {
        /// <summary>
        /// Replace tokens in an SQL script
        /// </summary>
        /// <param name="sqlText">The SQL script where the tokens will be replaced</param>
        /// <param name="parameters">The tokens to be replaced</param>
        /// <param name="quoter">
        /// The <see cref="IQuoter"/> used to safely quote/format <c>$[name]</c> parameter values
        /// (e.g. numbers, dates, booleans, <see langword="null"/>, strings). When <see langword="null"/>,
        /// a basic string-literal quoting fallback is used, which treats every value as a string.
        /// </param>
        /// <returns>The SQL script with the replaced tokens</returns>
        /// <remarks>
        /// Two token styles are supported:
        /// <list type="bullet">
        /// <item>
        /// <description>
        /// <c>$(name)</c> is replaced with the raw, unmodified parameter value. This is intended for
        /// substituting identifiers (table/column names) or SQL fragments/keywords that must not be
        /// quoted. Since the value is inserted verbatim, callers are responsible for validating/sanitizing
        /// any value used with this token style to avoid SQL injection.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// <c>$[name]</c> is replaced with the parameter value rendered as a safely quoted SQL literal
        /// using <paramref name="quoter"/>. Use this style whenever a value should be treated as
        /// data instead of raw SQL. Any value type supported by <see cref="IQuoter.QuoteValue"/> can be
        /// used, e.g. <see cref="string"/>, numbers, <see cref="System.DateTime"/>, <see cref="bool"/>,
        /// <see cref="System.Guid"/>, or <see langword="null"/>. When no <paramref name="quoter"/> is
        /// supplied, an invariant-culture rendering equivalent to the default
        /// <c>GenericQuoter.QuoteValue</c> is used.
        /// </description>
        /// </item>
        /// </list>
        /// <para>
        /// <strong><see cref="RawSql"/> is exempt from quoting in both styles.</strong> It is an
        /// explicit opt-in escape hatch, so a <see cref="RawSql"/> value is emitted verbatim even via
        /// <c>$[name]</c>. That token style is therefore not an injection-safety boundary when the
        /// value is a <see cref="RawSql"/> - the caller has already accepted responsibility for the
        /// SQL. It is a safety boundary for every other value type.
        /// </para>
        /// <para>
        /// The literal text <c>$(name)</c> or <c>$[name]</c> can be produced (without substitution) by
        /// escaping it as <c>$$((name))</c> or <c>$$[[name]]</c> respectively.
        /// </para>
        /// </remarks>
        public static string ReplaceSqlScriptTokens([StringSyntax("sql")] string sqlText, IDictionary<string, object> parameters, IQuoter quoter = null)
        {
            return ReplaceSqlScriptTokensCore(sqlText, parameters, quoter);
        }

        private static string ReplaceSqlScriptTokensCore<TValue>([StringSyntax("sql")] string sqlText, IDictionary<string, TValue> parameters, IQuoter quoter)
        {
            // Are parameters set?
            if (parameters != null && parameters.Count != 0)
            {
                // Replace $[word] elements with the values stored in the Parameters
                // dictionary, rendered as a safely quoted/escaped SQL literal.
                sqlText = Regex.Replace(
                    sqlText,
                    @"\$\[(?<token>\w+)\]",
                    m =>
                    {
                        var key = m.Groups["token"].Value;
                        if (parameters.TryGetValue(key, out var keyValue))
                        {
                            object value = keyValue;
                            return quoter != null
                                ? quoter.QuoteValue(value)
                                : QuoteSqlLiteral(value);
                        }

                        // Return the whole match value when the key
                        // wasn't found in the Parameters dictionary.
                        // This might help finding unset variables.
                        return m.Value;
                    }
                );

                // Replace $(word) elements with values stored
                // in the Parameters dictionary.
                sqlText = Regex.Replace(
                    sqlText,
                    @"\$\((?<token>\w+)\)",
                    m =>
                    {
                        var key = m.Groups["token"].Value;
                        if (parameters.TryGetValue(key, out var keyValue))
                        {
                            return FormatRawTokenValue(keyValue);
                        }

                        // Return the whole match value when the key
                        // wasn't found in the Parameters dictionary.
                        // This might help finding unset variables.
                        return m.Value;
                    }
                );

                // Replace $$((word)) with $(word)
                sqlText = Regex.Replace(sqlText, @"\${2}\({2}(\w+)\){2}", @"$$($1)");

                // Replace $$[[word]] with $[word]
                sqlText = Regex.Replace(sqlText, @"\${2}\[{2}(\w+)\]{2}", @"$$[$1]");
            }

            return sqlText;
        }

        private static string FormatRawTokenValue(object value)
        {
            return value switch
            {
                null => null,
                RawSql rawSql => rawSql.Value,
                IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
                _ => value.ToString(),
            };
        }

        /// <summary>
        /// Replace tokens in an SQL script
        /// </summary>
        /// <param name="sqlText">The SQL script where the tokens will be replaced</param>
        /// <param name="parameters">The tokens to be replaced</param>
        /// <returns>The SQL script with the replaced tokens</returns>
        /// <remarks>
        /// This overload is kept for backwards compatibility. Prefer the
        /// <see cref="ReplaceSqlScriptTokens(string, IDictionary{string, object}, IQuoter)"/> overload,
        /// which supports non-string parameter values and safe SQL literal quoting via an
        /// <see cref="IQuoter"/>.
        /// <para>
        /// The supplied dictionary is queried directly, preserving its key lookup semantics.
        /// </para>
        /// </remarks>
        [Obsolete("Use the IDictionary<string, object> overload instead. The IDictionary<string, string> overload is kept for backwards compatibility and will be removed in a future major version.")]
        public static string ReplaceSqlScriptTokens([StringSyntax("sql")] string sqlText, IDictionary<string, string> parameters)
        {
            return ReplaceSqlScriptTokensCore(sqlText, parameters, null);
        }

        /// <summary>
        /// Renders a value as a SQL literal when no <see cref="IQuoter"/> was supplied.
        /// </summary>
        /// <param name="value">The value to render</param>
        /// <returns>The value as a SQL literal</returns>
        /// <exception cref="NotSupportedException">
        /// <paramref name="value"/> is a <see cref="SystemMethods"/>, which has no dialect-independent
        /// rendering.
        /// </exception>
        /// <remarks>
        /// <para>
        /// Deliberately mirrors the default rendering of <c>GenericQuoter.QuoteValue</c>, which lives
        /// in <c>FluentMigrator.Runner.Core</c> and so cannot be referenced from this assembly. The
        /// two must agree: a token expanded without a quoter and the same value written by
        /// <c>Insert</c>/<c>Update</c>/<c>Delete</c> should produce the same literal. That agreement
        /// is asserted by <c>TokenSubstitutionMatrixTests</c>.
        /// </para>
        /// <para>
        /// Culture handling, precisely: numbers, <see cref="DateTime"/> and <see cref="DateTimeOffset"/>
        /// are formatted with <see cref="CultureInfo.InvariantCulture"/>, as is any other
        /// <see cref="IFormattable"/>. Enums render by name, which is culture-independent. Anything
        /// else falls back to <see cref="object.ToString"/> and therefore renders however that type
        /// chooses - <c>GenericQuoter.QuoteValue</c> has the same fallback. Before this, *every* value
        /// was stringified with the ambient culture, so a German build agent emitted <c>'1234,5678'</c>
        /// for a <see cref="double"/>.
        /// </para>
        /// </remarks>
        private static string QuoteSqlLiteral(object value)
        {
            switch (value)
            {
                case null:
                case DBNull _:
                    return "NULL";

                // An explicit opt-in escape hatch: the caller owns this SQL, so it must not be
                // quoted, escaped or otherwise altered.
                case RawSql rawSql:
                    return rawSql.Value;

                // Matched ahead of Enum, which would otherwise render it as the string literal
                // 'CurrentDateTime' - valid SQL syntax that silently means the wrong thing. There is
                // no dialect-independent rendering for these: CURRENT_TIMESTAMP, GETDATE(), SYSDATE
                // and NOW() are all spelled differently, which is why GenericQuoter delegates to
                // FormatSystemMethods and throws by default. Failing here says "supply an IQuoter"
                // instead of emitting a string that looks like it worked.
                case SystemMethods systemMethod:
                    throw new NotSupportedException(
                        $"The system method {systemMethod} cannot be rendered without an IQuoter, because its SQL " +
                        "spelling is dialect-specific. Pass the processor's IQuoter to ReplaceSqlScriptTokens, " +
                        "or substitute an explicit RawSql value.");

                case bool b:
                    return b ? "1" : "0";

                case byte[] bytes:
                    return FormatByteArray(bytes);

                case DateTime dateTime:
                    return QuoteString(dateTime.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture));

                case DateTimeOffset dateTimeOffset:
                    return QuoteString(dateTimeOffset.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture));

                // Numeric literals are emitted unquoted so they keep their SQL type. Enum is
                // deliberately matched before the integral types, since it renders as a string.
                case Enum _:
                    return QuoteString(value.ToString());

                case sbyte _:
                case byte _:
                case short _:
                case ushort _:
                case int _:
                case uint _:
                case long _:
                case ulong _:
                case float _:
                case double _:
                case decimal _:
                    return ((IFormattable)value).ToString(null, CultureInfo.InvariantCulture);

                case IFormattable formattable:
                    return QuoteString(formattable.ToString(null, CultureInfo.InvariantCulture));

                default:
                    return QuoteString(value.ToString());
            }
        }

        /// <summary>
        /// Wraps a value in single quotes, escaping embedded single quotes by doubling them.
        /// </summary>
        /// <param name="value">The value to quote</param>
        /// <returns>The quoted value</returns>
        private static string QuoteString(string value)
        {
            return "'" + value.Replace("'", "''") + "'";
        }

        /// <summary>
        /// Renders a byte array as a hexadecimal SQL literal, matching <c>GenericQuoter</c>.
        /// </summary>
        /// <param name="value">The bytes to render</param>
        /// <returns>The hexadecimal literal</returns>
        private static string FormatByteArray(byte[] value)
        {
            var hex = new StringBuilder((value.Length * 2) + 2);
            hex.Append("0x");
            foreach (var b in value)
            {
                hex.AppendFormat(CultureInfo.InvariantCulture, "{0:x2}", b);
            }

            return hex.ToString();
        }
    }
}
