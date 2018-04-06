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
    /// Provides special information about the found token
    /// </summary>
    public class SpecialTokenInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpecialTokenInfo"/> class.
        /// </summary>
        /// <param name="index">The index to the first character that is assigned to the <paramref name="token"/></param>
        /// <param name="length">The content length that is assigned to the <paramref name="token"/></param>
        /// <param name="token">The found token</param>
        /// <param name="opaque">An opaque (token specific) value</param>
        /// <remarks>
        /// The <paramref name="index"/> may not point to the real token text and the <paramref name="length"/> might be longer
        /// than the <paramref name="token"/> itself. This is usually the case when the token should be the only text on the line,
        /// but is instead surrounded by whitespace.
        /// </remarks>
        public SpecialTokenInfo(int index, int length, [NotNull] string token, object opaque = null)
        {
            Index = index;
            Length = length;
            Token = token;
            Opaque = opaque;
        }

        /// <summary>
        /// Gets the index to the first character that is assigned to the <see cref="Token"/>
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// Gets the content length that is assigned to the <see cref="Token"/>
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// Gets the found token
        /// </summary>
        [NotNull]
        public string Token { get; }

        /// <summary>
        /// Gets an opaque (token specific) value
        /// </summary>
        [CanBeNull]
        public object Opaque { get; }
    }
}
