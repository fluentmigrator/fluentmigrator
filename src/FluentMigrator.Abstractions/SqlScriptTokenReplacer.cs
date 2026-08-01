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
/// (e.g. numbers, dates, booleans, <see langword="null"/>, strings). Pass the processor's
/// <c>Quoter</c>. May be <see langword="null"/> only when <paramref name="sqlText"/>
/// contains no <c>$[name]</c> token that matches a key in <paramref name="parameters"/>, since nothing else consults it.
        /// </param>
        /// <returns>The SQL script with the replaced tokens</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="quoter"/> is <see langword="null"/> and <paramref name="sqlText"/>
        /// contains a <c>$[name]</c> token.
        /// </exception>
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
        /// <see cref="System.Guid"/>, or <see langword="null"/>.
        /// </description>
        /// </item>
        /// </list>
        /// <para>
        /// A <paramref name="quoter"/> is needed only for <c>$[name]</c>. A script using nothing but
        /// <c>$(name)</c> never consults it and may pass <see langword="null"/>.
        /// </para>
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
        public static string ReplaceSqlScriptTokens([StringSyntax("sql")] string sqlText, IDictionary<string, object> parameters, IQuoter quoter)
        {
            return ReplaceSqlScriptTokensCore(sqlText, parameters, quoter);
        }

        /// <param name="expandSafeTokens">
        /// Whether to expand the <c>$[name]</c> token style. The obsolete
        /// <see cref="IDictionary{TKey,TValue}"/> of <see cref="string"/> overload passes
        /// <see langword="false"/>, because that token style did not exist when it was the only
        /// API and 8.0.1 left such text untouched.
        /// </param>
        private static string ReplaceSqlScriptTokensCore<TValue>(
            [StringSyntax("sql")] string sqlText,
            IDictionary<string, TValue> parameters,
            IQuoter quoter,
            bool expandSafeTokens = true)
        {
            // Are parameters set?
            if (parameters != null && parameters.Count != 0)
            {
                // Replace $[word] elements with the values stored in the Parameters
                // dictionary, rendered as a safely quoted/escaped SQL literal.
                if (expandSafeTokens)
                {
                    sqlText = Regex.Replace(
                        sqlText,
                        @"\$\[(?<token>\w+)\]",
                        m =>
                        {
                            var key = m.Groups["token"].Value;
                            if (parameters.TryGetValue(key, out var keyValue))
                            {
                                // Guarded here rather than at method entry on purpose: a script
                                // using only $(name) never consults the quoter, so rejecting it up
                                // front would fail callers that have no need of one.
                                if (quoter is null)
                                {
                                    throw new ArgumentNullException(
                                        nameof(quoter),
                                        $"Expanding the $[{key}] token requires an IQuoter. Pass the " +
                                        "processor's Quoter.");
                                }

                                object value = keyValue;
                                return quoter.QuoteValue(value);
                            }

                            // Return the whole match value when the key
                            // wasn't found in the Parameters dictionary.
                            // This might help finding unset variables.
                            return m.Value;
                        }
                    );
                }

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

                // Replace $$[[word]] with $[word]. Gated with the token it escapes: both arrived
                // together, so a caller that does not get $[name] should not get its escape form
                // rewritten either.
                if (expandSafeTokens)
                {
                    sqlText = Regex.Replace(sqlText, @"\${2}\[{2}(\w+)\]{2}", @"$$[$1]");
                }
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
        /// <para>
        /// <strong>This overload does not expand <c>$[name]</c>.</strong> That token style did not
        /// exist when this was the only API - 8.0.1 returned such text unchanged - and there is no
        /// quoter here to render it with. <c>$[name]</c> and its <c>$$[[name]]</c> escape are left
        /// verbatim, exactly as in 8.0.1. Callers wanting safely quoted literals should move to the
        /// <see cref="ReplaceSqlScriptTokens(string, IDictionary{string, object}, IQuoter)"/>
        /// overload and pass the processor's <c>Quoter</c>.
        /// </para>
        /// </remarks>
        [Obsolete("Use the IDictionary<string, object> overload instead. The IDictionary<string, string> overload is kept for backwards compatibility and will be removed in a future major version.")]
        public static string ReplaceSqlScriptTokens([StringSyntax("sql")] string sqlText, IDictionary<string, string> parameters)
        {
            return ReplaceSqlScriptTokensCore(sqlText, parameters, quoter: null, expandSafeTokens: false);
        }
    }
}
