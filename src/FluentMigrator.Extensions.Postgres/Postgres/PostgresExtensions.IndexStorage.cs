#region License
// Copyright (c) 2020, FluentMigrator Project
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

using FluentMigrator.Builders.Create.Index;
using FluentMigrator.Infrastructure;
using FluentMigrator.Infrastructure.Extensions;

namespace FluentMigrator.Postgres
{
    public static partial class PostgresExtensions
    {
        public const string IndexFillFactor = "PostgresFillFactor";

        /// <summary>
        /// The fillfactor for an index is a percentage that determines how full the index method will try to pack index pages. For B-trees, leaf pages are filled to this percentage during initial index build,
        /// and also when extending the index at the right (adding new largest key values). If pages subsequently become completely full, they will be split, leading to gradual degradation in the index's efficiency.
        /// B-trees use a default fillfactor of 90, but any integer value from 10 to 100 can be selected. If the table is static then fillfactor 100 is best to minimize the index's physical size,
        /// but for heavily updated tables a smaller fillfactor is better to minimize the need for page splits. The other index methods use fillfactor in different but roughly analogous ways;
        /// the default fillfactor varies between methods.
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="fillfactor">The fillfactor value from 10 to 100 can be selected</param>
        /// <returns>The next step</returns>
        public static ICreateIndexOptionsSyntax WithFillfactor(this ICreateIndexOptionsSyntax expression, int fillfactor)
        {
            var additionalFeatures = expression as ISupportAdditionalFeatures;
            additionalFeatures.SetAdditionalFeature(IndexFillFactor, fillfactor);
            return expression;
        }

        #region B-Tree
        
        public const string IndexVacuumCleanupIndexScaleFactor = "PostgresBTreeVacuumCleanupIndexScaleFactor";

        /// <summary>
        /// Exclusive for B-Tree index. Specifies the fraction of the total number of heap tuples counted in the previous statistics collection that can be inserted without incurring an index scan at the VACUUM cleanup stage. This setting currently applies to B-tree indexes only.
        /// For more information about it see: https://www.postgresql.org/docs/current/runtime-config-client.html#GUC-VACUUM-CLEANUP-INDEX-SCALE-FACTOR
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="point">The value can range from 0 to 10000000000. When vacuum_cleanup_index_scale_factor is set to 0, index scans are never skipped during VACUUM cleanup. The default value is 0.1</param>
        /// <returns>The next step</returns>
        public static ICreateIndexOptionsSyntax WithVacuumCleanupIndexScaleFactor(this ICreateIndexOptionsSyntax expression, float point)
        {
            var additionalFeatures = expression as ISupportAdditionalFeatures;
            additionalFeatures.SetAdditionalFeature(IndexVacuumCleanupIndexScaleFactor, point);
            return expression;
        }
        #endregion

        #region GiST
        public const string IndexBuffering = "PostgresGiSTBuffering";

        /// <summary>
        /// Exclusive for GiST index. Building large GiST indexes by simply inserting all the tuples tends to be slow, because if the index tuples are scattered across the index and the index is large enough to not fit in cache, the insertions need to perform a lot of random I/O.
        /// For more information about it see: https://www.postgresql.org/docs/current/gist-implementation.html#GIST-BUFFERING-BUILD
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="buffering">The buffering value.</param>
        /// <returns>The next step</returns>
        public static ICreateIndexOptionsSyntax WithBuffering(this ICreateIndexOptionsSyntax expression, GistBuffering buffering)
        {
            var additionalFeatures = expression as ISupportAdditionalFeatures;
            additionalFeatures.SetAdditionalFeature(IndexBuffering, buffering);
            return expression;
        }
        #endregion

        #region GIN
        public const string IndexFastUpdate = "PostgresGinFastUpdate";

        /// <summary>
        /// Exclusive for GIN index. Updating a GIN index tends to be slow because of the intrinsic nature of inverted indexes: inserting or updating one heap row can cause many inserts into the index (one for each key extracted from the indexed item).
        /// For more information about it see: https://www.postgresql.org/docs/current/gin-implementation.html#GIN-FAST-UPDATE
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="fastUpdate">True to enable fast update or false to disable.</param>
        /// <returns>The next step</returns>
        public static ICreateIndexOptionsSyntax WithFastUpdate(this ICreateIndexOptionsSyntax expression, bool fastUpdate)
        {
            var additionalFeatures = expression as ISupportAdditionalFeatures;
            additionalFeatures.SetAdditionalFeature(IndexFastUpdate, fastUpdate);
            return expression;
        }

        public const string IndexGinPendingListLimit = "PostgresGinPendingListLimit";
        /// <summary>
        /// Exclusive for GIN index. Sets the maximum size of a GIN index's pending list, which is used when fastupdate is enabled. If the list grows larger than this maximum size, it is cleaned up by moving the entries in it to the index's main GIN data structure in bulk. If this value is specified without units, it is taken as kilobytes.
        /// For more information about it see: https://www.postgresql.org/docs/current/runtime-config-client.html#GUC-GIN-PENDING-LIST-LIMIT
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="limit">The list limit in kilobytes.</param>
        /// <returns>The next step</returns>
        public static ICreateIndexOptionsSyntax WithGinPendingListLimit(this ICreateIndexOptionsSyntax expression, long limit)
        {
            var additionalFeatures = expression as ISupportAdditionalFeatures;
            additionalFeatures.SetAdditionalFeature(IndexGinPendingListLimit, limit);
            return expression;
        }
        #endregion

        #region BRIN
        public const string IndexPagesPerRange = "PostgresBrinPagesPerRange";

        /// <summary>
        /// Exclusive for BRIN index. Defines the number of table blocks that make up one block range for each entry of a BRIN index.
        /// For more information about it see: https://www.postgresql.org/docs/current/brin-intro.html
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="range">The page per range</param>
        /// <returns>The next step</returns>
        public static ICreateIndexOptionsSyntax WithPagesPerRange(this ICreateIndexOptionsSyntax expression, int range)
        {
            var additionalFeatures = expression as ISupportAdditionalFeatures;
            additionalFeatures.SetAdditionalFeature(IndexPagesPerRange, range);
            return expression;
        }

        public const string IndexAutosummarize = "PostgresBrinautosummarize";

        /// <summary>
        /// Exclusive for BRIN index. Defines whether a summarization run is invoked for the previous page range whenever an insertion is detected on the next one.
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="autosummarize">True to enable fast autosummarize or false to disable.</param>
        /// <returns>The next step</returns>
        public static ICreateIndexOptionsSyntax WithAutosummarize(this ICreateIndexOptionsSyntax expression, bool autosummarize)
        {
            var additionalFeatures = expression as ISupportAdditionalFeatures;
            additionalFeatures.SetAdditionalFeature(IndexAutosummarize, autosummarize);
            return expression;
        }
        #endregion
    }

    public enum GistBuffering
    {
        On,
        Off,
        Auto
    }
}
