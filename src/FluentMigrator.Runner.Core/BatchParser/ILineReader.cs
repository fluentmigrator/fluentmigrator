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
    /// Read content from a line and advance the internal index
    /// </summary>
    public interface ILineReader
    {
        /// <summary>
        /// Gets the current line
        /// </summary>
        [NotNull]
        string Line { get; }

        /// <summary>
        /// Gets the current index into the line
        /// </summary>
        int Index { get; }

        /// <summary>
        /// Gets the remaining length
        /// </summary>
        int Length { get; }

        /// <summary>
        /// Reads a string with the given <paramref name="length"/> from the <see cref="Line"/>
        /// </summary>
        /// <param name="length">The length of the string to read from the <see cref="Line"/></param>
        /// <returns>The read string</returns>
        [NotNull]
        string ReadString(int length);

        /// <summary>
        /// Creates a new <see cref="ILineReader"/> while moving the internal <see cref="Index"/> by the given <paramref name="length"/>
        /// </summary>
        /// <param name="length">The number of characters to move the internal <see cref="Index"/></param>
        /// <returns>A new line reader with the new index</returns>
        [CanBeNull]
        ILineReader Advance(int length);
    }
}
