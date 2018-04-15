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

using JetBrains.Annotations;

namespace FluentMigrator.Runner.BatchParser
{
    /// <summary>
    /// The result of a <see cref="IRangeSearcher.FindEndCode"/> operation
    /// </summary>
    public class EndCodeSearchResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EndCodeSearchResult"/> class.
        /// </summary>
        /// <param name="index">The index into the <see cref="ILineReader"/> where the end code was found</param>
        public EndCodeSearchResult(int index)
        {
            Index = index;
            NestedRangeSearcher = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EndCodeSearchResult"/> class.
        /// </summary>
        /// <param name="index">The index into the <see cref="ILineReader"/> where the nested start code was found</param>
        /// <param name="nestedRangeSearcher">The searcher to be used to find the end of the nested range</param>
        public EndCodeSearchResult(int index, [NotNull] IRangeSearcher nestedRangeSearcher)
        {
            Index = index;
            NestedRangeSearcher = nestedRangeSearcher;
        }

        /// <summary>
        /// Gets a value indicating whether this is a nested range
        /// </summary>
        public bool IsNestedStart => NestedRangeSearcher != null;

        /// <summary>
        /// Gets the index into the previously tested <see cref="ILineReader"/> of the end code or nested start code
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// Gets the searcher to be used to find the end of the nested range
        /// </summary>
        [CanBeNull]
        public IRangeSearcher NestedRangeSearcher { get; }

        /// <summary>
        /// Operator to convert an index of the end code into a <see cref="EndCodeSearchResult"/>
        /// </summary>
        /// <param name="index">The index into the <see cref="ILineReader"/> of the end code</param>
        public static implicit operator EndCodeSearchResult(int index)
        {
            if (index == -1)
                return null;
            return new EndCodeSearchResult(index);
        }
    }
}
