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

using FluentMigrator.Builder.Create.Index;
using FluentMigrator.Builders.Create.Index;
using FluentMigrator.Infrastructure;
using FluentMigrator.Infrastructure.Extensions;

namespace FluentMigrator.Postgres
{
    public static partial class PostgresExtensions
    {

        /// <summary>
        /// The fillfactor for an index is a percentage that determines how full the index method will try to pack index pages.
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="fillfactor">The fillfactor value from 10 to 100 can be selected</param>
        /// <returns>The next step</returns>
        /// <remarks>
        /// For B-trees, leaf pages are filled to this percentage during initial index build, and also when extending
        /// the index at the right (adding new largest key values). If pages subsequently become completely full,
        /// they will be split, leading to gradual degradation in the index's efficiency.
        /// B-trees use a default fillfactor of 90, but any integer value from 10 to 100 can be selected.
        /// If the table is static then fillfactor 100 is best to minimize the index's physical size,
        /// but for heavily updated tables a smaller fillfactor is better to minimize the need for page splits.
        /// The other index methods use fillfactor in different but roughly analogous ways; the default fillfactor varies between methods.
        /// </remarks>
        public static ICreateBTreeIndexOptionsSyntax Fillfactor(this ICreateIndexOptionsSyntax expression, int fillfactor)
        {
            return expression.UsingBTree()
                .Fillfactor(fillfactor) as ICreateBTreeIndexOptionsSyntax;
        }

        #region BRIN
        public const string IndexPagesPerRange = "PostgresBrinPagesPerRange";

        /// <summary>
        /// Exclusive for BRIN index. Defines the number of table blocks that make up one block range for each entry of a BRIN index.
        /// For more information about it see: https://www.postgresql.org/docs/current/brin-intro.html
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="range">The page per range</param>
        /// <returns>The next step</returns>
        public static ICreateIndexOptionsSyntax PagesPerRange(this ICreateIndexOptionsSyntax expression, int range)
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
        public static ICreateIndexOptionsSyntax Autosummarize(this ICreateIndexOptionsSyntax expression, bool autosummarize)
        {
            var additionalFeatures = expression as ISupportAdditionalFeatures;
            additionalFeatures.SetAdditionalFeature(IndexAutosummarize, autosummarize);
            return expression;
        }
        #endregion
    }
}
