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

using System.Collections.Generic;
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
        /// using <paramref name="quoter"/> (falling back to a quoted/escaped string literal when no
        /// <paramref name="quoter"/> is supplied). Use this style whenever a value should be treated as
        /// data instead of raw SQL. Any value type supported by <see cref="IQuoter.QuoteValue"/> can be
        /// used, e.g. <see cref="string"/>, numbers, <see cref="System.DateTime"/>, <see cref="bool"/>,
        /// <see cref="System.Guid"/>, or <see langword="null"/>.
        /// </description>
        /// </item>
        /// </list>
        /// The literal text <c>$(name)</c> or <c>$[name]</c> can be produced (without substitution) by
        /// escaping it as <c>$$((name))</c> or <c>$$[[name]]</c> respectively.
        /// </remarks>
        public static string ReplaceSqlScriptTokens(string sqlText, IDictionary<string, object> parameters, IQuoter quoter = null)
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
                            return quoter != null
                                ? quoter.QuoteValue(keyValue)
                                : QuoteSqlStringLiteral(keyValue?.ToString());
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
                            return keyValue?.ToString();
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

        /// <summary>
        /// Renders a value as a safely quoted/escaped SQL string literal.
        /// </summary>
        /// <param name="value">The value to quote</param>
        /// <returns>
        /// <c>NULL</c> when <paramref name="value"/> is <see langword="null"/>, otherwise the value
        /// wrapped in single quotes with any embedded single quotes escaped by doubling them.
        /// </returns>
        private static string QuoteSqlStringLiteral(string value)
        {
            if (value is null)
            {
                return "NULL";
            }

            return "'" + value.Replace("'", "''") + "'";
        }
    }
}
