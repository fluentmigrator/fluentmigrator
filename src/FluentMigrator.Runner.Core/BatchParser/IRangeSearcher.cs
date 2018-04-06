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
    /// Interface to search for content with a given start- and end code
    /// </summary>
    public interface IRangeSearcher
    {
        /// <summary>
        /// Gets the length of the start code
        /// </summary>
        int StartCodeLength { get; }

        /// <summary>
        /// Gets the length of the end code
        /// </summary>
        int EndCodeLength { get; }

        /// <summary>
        /// Is this range a comment?
        /// </summary>
        bool IsComment { get; }

        /// <summary>
        /// Gets the index into the <paramref name="reader"/> where the start code was found
        /// </summary>
        /// <param name="reader">The reader where the start code is searched</param>
        /// <returns><c>-1</c> when the start code couldn't be found</returns>
        int FindStartCode([NotNull] ILineReader reader);

        /// <summary>
        /// Search for an end code
        /// </summary>
        /// <param name="reader">The reader where the end code is searched</param>
        /// <returns><c>null</c> when the end code couldn't be found (or a nested start code)</returns>
        [CanBeNull]
        EndCodeSearchResult FindEndCode([NotNull] ILineReader reader);
    }
}
