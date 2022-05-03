#region License
// Copyright (c) 2021, FluentMigrator Project
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

namespace FluentMigrator.Builder.Create.Index
{
    /// <summary>
    /// Gin index options
    /// </summary>
    public interface ICreateGinIndexOptionsSyntax : ICreateIndexMethodOptionsSyntax
    {

        /// <inheritdoc cref="ICreateIndexMethodOptionsSyntax.Fillfactor"/>
        new ICreateGinIndexOptionsSyntax Fillfactor(int fillfactor);

        /// <summary>
        /// Enable fast update. Updating a GIN index tends to be slow because of the intrinsic nature of inverted indexes: inserting or updating one heap row can cause many inserts into the index (one for each key extracted from the indexed item).
        /// For more information about it see: https://www.postgresql.org/docs/current/gin-implementation.html#GIN-FAST-UPDATE
        /// </summary>
        /// <returns>The next step</returns>
        ICreateGinIndexOptionsSyntax FastUpdate();

        /// <summary>
        /// Disable fast update. Updating a GIN index tends to be slow because of the intrinsic nature of inverted indexes: inserting or updating one heap row can cause many inserts into the index (one for each key extracted from the indexed item).
        /// For more information about it see: https://www.postgresql.org/docs/current/gin-implementation.html#GIN-FAST-UPDATE
        /// </summary>
        /// <returns>The next step</returns>
        ICreateGinIndexOptionsSyntax DisableFastUpdate();

        /// <summary>
        /// Updating a GIN index tends to be slow because of the intrinsic nature of inverted indexes: inserting or updating one heap row can cause many inserts into the index (one for each key extracted from the indexed item).
        /// For more information about it see: https://www.postgresql.org/docs/current/gin-implementation.html#GIN-FAST-UPDATE
        /// </summary>
        /// <param name="fastUpdate">True to enable fast update or false to disable.</param>
        /// <returns>The next step</returns>
        ICreateGinIndexOptionsSyntax FastUpdate(bool fastUpdate);

        /// <summary>
        /// Sets the maximum size of a GIN index's pending list, which is used when fastUpdate is enabled. If the list grows larger than this maximum size, it is cleaned up by moving the entries in it to the index's main GIN data structure in bulk. If this value is specified without units, it is taken as kilobytes.
        /// For more information about it see: https://www.postgresql.org/docs/current/runtime-config-client.html#GUC-GIN-PENDING-LIST-LIMIT
        /// </summary>
        /// <param name="limit">The list limit in kilobytes.</param>
        /// <returns>The next step</returns>
        ICreateGinIndexOptionsSyntax PendingListLimit(long limit);
    }
}
