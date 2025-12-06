#region License
// Copyright (c) 2020, Fluent Migrator Project
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

using System;

using FluentMigrator.Builders.Create.Index;
using FluentMigrator.Infrastructure;
using FluentMigrator.Infrastructure.Extensions;
using FluentMigrator.Model;

namespace FluentMigrator.Postgres
{
    /// <summary>
    /// Provides extension methods for PostgreSQL-specific index creation options.
    /// </summary>
    public static partial class PostgresExtensions
    {
        /// <summary>
        /// Represents the key used to store or retrieve the NULL sorting behavior
        /// for PostgreSQL index columns in the additional features dictionary.
        /// </summary>
        /// <remarks>
        /// This constant is utilized to specify or retrieve the sorting of NULL values
        /// (e.g., "NULLS FIRST" or "NULLS LAST") when creating or modifying PostgreSQL indexes.
        /// </remarks>
        public const string NullsSort = "PostgresNulls";

        /// <summary>
        /// Specifies that nulls sort before non-nulls. This is the default when DESC is specified.
        /// </summary>
        /// <param name="expression">The <see cref="ICreateIndexMoreColumnOptionsSyntax"/></param>
        /// <returns>The <see cref="ICreateIndexMoreColumnOptionsSyntax"/></returns>
        public static ICreateIndexMoreColumnOptionsSyntax NullsFirst(this ICreateIndexMoreColumnOptionsSyntax expression)
        {
            var additionalFeatures = expression.CurrentColumn as ISupportAdditionalFeatures;
            additionalFeatures.Nulls(NullSort.First);
            return expression;
        }

        /// <summary>
        /// Specifies that nulls sort after non-nulls. This is the default when DESC is not specified.
        /// </summary>
        /// <param name="expression">The <see cref="ICreateIndexMoreColumnOptionsSyntax"/></param>
        /// <returns>The <see cref="ICreateIndexMoreColumnOptionsSyntax"/></returns>

        public static ICreateIndexMoreColumnOptionsSyntax NullsLast(this ICreateIndexMoreColumnOptionsSyntax expression)
        {
            var additionalFeatures = expression.CurrentColumn as ISupportAdditionalFeatures;
            additionalFeatures.Nulls(NullSort.Last);
            return expression;
        }

        /// <summary>
        /// Specifies that nulls sort before non-nulls.
        /// </summary>
        /// <param name="expression">The <see cref="ICreateIndexMoreColumnOptionsSyntax"/></param>
        /// <param name="sort">The <see cref="NullsSort"/>.</param>
        /// <returns>The <see cref="ICreateIndexMoreColumnOptionsSyntax"/></returns>
        public static ICreateIndexMoreColumnOptionsSyntax Nulls(this ICreateIndexMoreColumnOptionsSyntax expression, NullSort sort)
        {
            var additionalFeatures = expression as ISupportAdditionalFeatures;
            additionalFeatures.Nulls(sort);
            return expression;
        }

        internal static void Nulls(this ISupportAdditionalFeatures additionalFeatures, NullSort sort)
        {
            if (additionalFeatures == null)
            {
                throw new InvalidOperationException(UnsupportedMethodMessage(nameof(Nulls), nameof(ISupportAdditionalFeatures)));
            }

            var nullSort = additionalFeatures.GetAdditionalFeature(NullsSort, () => new PostgresIndexNullsSort());
            nullSort.Sort = sort;
        }

    }
}
