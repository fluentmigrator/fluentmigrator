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
using System.Text.RegularExpressions;

namespace FluentMigrator.Runner.BatchParser.SpecialTokenSearchers
{
    /// <summary>
    /// Searches for a "GO n" or "GO" token
    /// </summary>
    public class GoSearcher : ISpecialTokenSearcher
    {
        private static readonly Regex _regex = new Regex(@"^\s*(?<statement>GO(\s+(?<count>\d+))?)\s*$", RegexOptions.CultureInvariant | RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <inheritdoc />
        public SpecialTokenInfo Find(ILineReader reader)
        {
            if (reader.Index != 0)
                return null;

            var match = _regex.Match(reader.Line);
            if (!match.Success)
                return null;

            var token = match.Groups["statement"].Value;
            var count = GetGoCount(match) ?? 1;
            var parameters = new GoSearcherParameters(count);

            return new SpecialTokenInfo(0, reader.Line.Length, token, parameters);
        }

        /// <summary>
        /// Extracts the count value from a "GO n" or "GO" token in the provided SQL string.
        /// </summary>
        /// <param name="sql">The SQL string containing the "GO" token.</param>
        /// <returns>
        /// The count value specified in the "GO n" token, or <c>1</c> if no count is specified.
        /// Returns <c>null</c> if the input string does not match the expected "GO" token format.
        /// </returns>
        public static int? GetGoCount(string sql)
        {
            var match = _regex.Match(sql);
            return GetGoCount(match);
        }

        private static int? GetGoCount(Match match)
        {
            if (!match.Success)
                return null;

            var countGroup = match.Groups["count"];
            if (!countGroup.Success || countGroup.Length == 0)
                return 1;

            return Convert.ToInt32(countGroup.Value, 10);
        }

        /// <summary>
        /// Additional values for the GO token
        /// </summary>
        public class GoSearcherParameters
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="GoSearcherParameters"/> class.
            /// </summary>
            /// <param name="count">the number of times the batch should be executed</param>
            internal GoSearcherParameters(int count)
            {
                Count = count;
            }

            /// <summary>
            /// Gets the number of times the batch should be executed
            /// </summary>
            public int Count { get; }
        }
    }
}
