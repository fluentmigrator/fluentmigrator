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
    /// B-tree index options
    /// </summary>
    public interface ICreateBTreeIndexOptionsSyntax : ICreateIndexMethodOptionsSyntax
    {
        /// <inheritdoc cref="ICreateIndexMethodOptionsSyntax.Fillfactor"/>
        new ICreateBTreeIndexOptionsSyntax Fillfactor(int fillfactor);

        /// <summary>
        /// Specifies the fraction of the total number of heap tuples counted in the previous statistics collection that can be inserted without incurring an index scan at the VACUUM cleanup stage. This setting currently applies to B-tree indexes only.
        /// For more information about it see: https://www.postgresql.org/docs/current/runtime-config-client.html#GUC-VACUUM-CLEANUP-INDEX-SCALE-FACTOR
        /// </summary>
        /// <param name="point">The value can range from 0 to 10000000000. When vacuum_cleanup_index_scale_factor is set to 0, index scans are never skipped during VACUUM cleanup. The default value is 0.1</param>
        /// <returns>The next step</returns>
        ICreateBTreeIndexOptionsSyntax VacuumCleanupIndexScaleFactor(float point);
    }
}
