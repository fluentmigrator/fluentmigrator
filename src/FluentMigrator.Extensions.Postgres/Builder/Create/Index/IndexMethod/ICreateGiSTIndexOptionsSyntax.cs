#region License
// Copyright (c) 2021, Fluent Migrator Project
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

using FluentMigrator.Postgres;

namespace FluentMigrator.Builder.Create.Index
{
    /// <summary>
    /// GiST index options
    /// </summary>
    public interface ICreateGiSTIndexOptionsSyntax : ICreateIndexMethodOptionsSyntax
    {
        /// <inheritdoc cref="ICreateIndexMethodOptionsSyntax.Fillfactor"/>
        new ICreateGiSTIndexOptionsSyntax Fillfactor(int fillfactor);

        /// <summary>
        /// Building large GiST indexes by simply inserting all the tuples tends to be slow, because if the index
        /// tuples are scattered across the index and the index is large enough to not fit in cache, the insertions need
        /// to perform a lot of random I/O.
        /// For more information about it see: https://www.postgresql.org/docs/current/gist-implementation.html#GIST-BUFFERING-BUILD
        /// </summary>
        /// <param name="buffering">The <see cref="GistBuffering"/> value.</param>
        /// <returns>The next step</returns>
        ICreateGiSTIndexOptionsSyntax Buffering(GistBuffering buffering);
    }
}
