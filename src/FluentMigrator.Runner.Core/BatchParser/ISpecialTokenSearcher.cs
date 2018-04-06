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
    /// Searches for special tokens (e.g. <c>GO</c>)
    /// </summary>
    public interface ISpecialTokenSearcher
    {
        /// <summary>
        /// Search for the special token in the given <paramref name="reader"/>
        /// </summary>
        /// <param name="reader">The reader used to search the token</param>
        /// <returns><c>null</c> when the token couldn't be found</returns>
        [CanBeNull]
        SpecialTokenInfo Find([NotNull] ILineReader reader);
    }
}
