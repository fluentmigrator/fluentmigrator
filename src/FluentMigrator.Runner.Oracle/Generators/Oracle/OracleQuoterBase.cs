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
using System.Text;

using FluentMigrator.Runner.Generators.Generic;

namespace FluentMigrator.Runner.Generators.Oracle
{
    /// <summary>
    /// Provides a base implementation for quoting and formatting Oracle-specific SQL expressions.
    /// </summary>
    /// <remarks>
    /// This class extends the functionality of <see cref="FluentMigrator.Runner.Generators.Generic.GenericQuoter"/> 
    /// to handle Oracle-specific requirements, such as escaping characters, splitting strings into chunks, 
    /// and formatting various data types like dates, GUIDs, and time spans.
    /// </remarks>
    public class OracleQuoterBase : GenericQuoter
    {

        /// <summary>
        /// The maximum chunk length for Oracle string literals.
        /// </summary>
        /// <remarks>
        /// See http://www.dba-oracle.com/t_ora_01704_string_literal_too_long.htm
        /// </remarks>
        public const int MaxChunkLength = 3900;

        /// <summary>
        /// The escape characters for Oracle string literals.
        /// </summary>
        public static readonly char[] EscapeCharacters = new[] { '\'', '\t', '\r', '\n' };

        /// <summary>
        /// Splits a string into chunks of the specified maximum length, considering escape characters.
        /// </summary>
        /// <param name="str">The string to split.</param>
        /// <param name="maxChunkLength">The maximum chunk length.</param>
        /// <returns>An enumerable of string chunks.</returns>
        public static IEnumerable<string> SplitBy(string str, int maxChunkLength)
        {
            if (string.IsNullOrEmpty(str))
                throw new ArgumentNullException(nameof(str));

            // Having escape characters the chunk length less than 2 does not make a sense.
            if (maxChunkLength < 2)
                throw new ArgumentException($"'{nameof(maxChunkLength)}' must be greater than 1.");

            var chunk = new StringBuilder();
            var chunkLength = 0;

            foreach (var ch in str)
            {
                // Every escape character will give us two characters in the final query
                chunkLength += EscapeCharacters.Contains(ch) ? 2 : 1;
                if (chunkLength > maxChunkLength)
                {
                    yield return chunk.ToString();
                    chunk.Clear();
                    chunkLength = 0;
                }
                chunk.Append(ch);
            }
            yield return chunk.ToString();
        }

        /// <inheritdoc />
        public override string FormatDateTime(DateTime value)
        {
            var result = string.Format("to_date({0}{1}{0}, {0}yyyy-mm-dd hh24:mi:ss{0})", ValueQuote, value.ToString("yyyy-MM-dd HH:mm:ss")); //ISO 8601 DATETIME FORMAT (EXCEPT 'T' CHAR)
            return result;
        }

        /// <inheritdoc />
        public override string QuoteIndexName(string indexName, string schemaName)
        {
            return CreateSchemaPrefixedQuotedIdentifier(QuoteSchemaName(schemaName), IsQuoted(indexName) ? indexName : Quote(indexName));
        }

        /// <inheritdoc />
        public override string FromTimeSpan(TimeSpan value)
        {
            return string.Format("{0}{1} {2}:{3}:{4}.{5}{0}"
                , ValueQuote
                , value.Days
                , value.Hours
                , value.Minutes
                , value.Seconds
                , value.Milliseconds);
        }

        /// <inheritdoc />
        public override string FormatGuid(Guid value)
        {
            return string.Format("{0}{1}{0}", ValueQuote, BitConverter.ToString(value.ToByteArray()).Replace("-", string.Empty));
        }

        /// <inheritdoc />
        public override string FormatSystemMethods(SystemMethods value)
        {
            switch (value)
            {
                case SystemMethods.NewGuid:
                case SystemMethods.NewSequentialId:
                    return "sys_guid()";
                case SystemMethods.CurrentDateTime:
                    return "LOCALTIMESTAMP";
                case SystemMethods.CurrentDateTimeOffset:
                    return "CURRENT_TIMESTAMP";
                case SystemMethods.CurrentUTCDateTime:
                    return "sys_extract_utc(SYSTIMESTAMP)";
                case SystemMethods.CurrentUser:
                    return "USER";
            }

            return base.FormatSystemMethods(value);
        }

        /// <summary>
        /// Formats a string value using the specified Oracle function and formatter, splitting into chunks if necessary.
        /// </summary>
        /// <param name="value">The string value to format.</param>
        /// <param name="oracleFunction">The Oracle function to use.</param>
        /// <param name="formatter">The formatter function.</param>
        /// <returns>The formatted string.</returns>
        private static string FormatString(string value, string oracleFunction, Func<string, string> formatter)
        {
            if (value.Length < MaxChunkLength)
            {
                return formatter(value);
            }

            var chunks = SplitBy(value, MaxChunkLength)
                .Select(v => $"{oracleFunction}({formatter(v)})");

            return string.Join(" || ", chunks);
        }

        /// <inheritdoc />
        public override string FormatAnsiString(string value)
        {
            if (value.Length < MaxChunkLength)
            {
                return base.FormatAnsiString(value);
            }

            return FormatString(value, "TO_CLOB", base.FormatAnsiString);
        }

        /// <inheritdoc />
        public override string FormatNationalString(string value)
        {
            if (value.Length < MaxChunkLength)
            {
                return base.FormatAnsiString(value);
            }

            return FormatString(value, "TO_NCLOB", base.FormatNationalString);
        }
    }
}
