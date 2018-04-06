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

using System.Text.RegularExpressions;

namespace FluentMigrator.Runner.BatchParser.SpecialTokenSearchers
{
    /// <summary>
    /// Searches for a "GO n" or "GO" token
    /// </summary>
    public class GoSearcher : ISpecialTokenSearcher
    {
        private static readonly Regex _regex = new Regex(@"^\s*(?<statement>GO(\s+(?<count>\d)+)?)\s*$", RegexOptions.CultureInvariant | RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <inheritdoc />
        public SpecialTokenInfo Find(ILineReader reader)
        {
            if (reader.Index != 0)
                return null;

            var match = _regex.Match(reader.Line);
            if (!match.Success)
                return null;

            return new SpecialTokenInfo(0, reader.Line.Length, match.Groups["statement"].Value);
        }
    }
}
