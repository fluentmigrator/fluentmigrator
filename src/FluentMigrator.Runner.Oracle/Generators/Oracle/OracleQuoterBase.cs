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
    public class OracleQuoterBase : GenericQuoter
    {
        // http://www.dba-oracle.com/t_ora_01704_string_literal_too_long.htm
        public const int MaxChunkLength = 3900;

        public static readonly char[] EscapeCharacters = new[] { '\'', '\t', '\r', '\n' };

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

        public override string FormatDateTime(DateTime value)
        {
            var result = string.Format("to_date({0}{1}{0}, {0}yyyy-mm-dd hh24:mi:ss{0})", ValueQuote, value.ToString("yyyy-MM-dd HH:mm:ss")); //ISO 8601 DATETIME FORMAT (EXCEPT 'T' CHAR)
            return result;
        }

        public override string QuoteIndexName(string indexName, string schemaName)
        {
            return CreateSchemaPrefixedQuotedIdentifier(QuoteSchemaName(schemaName), IsQuoted(indexName) ? indexName : Quote(indexName));
        }

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

        public override string FormatGuid(Guid value)
        {
            return string.Format("{0}{1}{0}", ValueQuote, BitConverter.ToString(value.ToByteArray()).Replace("-", string.Empty));
        }

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

        public override string FormatAnsiString(string value)
        {
            if (value.Length < MaxChunkLength)
            {
                return base.FormatAnsiString(value);
            }

            return FormatString(value, "TO_CLOB", base.FormatAnsiString);
        }

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
