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
using System.Collections.Generic;

using FluentMigrator.Builder.Create.Index;
using FluentMigrator.Builders.Create.Index;
using FluentMigrator.Infrastructure;
using FluentMigrator.Infrastructure.Extensions;
using FluentMigrator.Model;

namespace FluentMigrator.Postgres
{
    /// <summary>
    /// Provides extension methods for PostgreSQL-specific functionality in FluentMigrator.
    /// </summary>
    /// <remarks>
    /// This class contains methods to extend the functionality of FluentMigrator for PostgreSQL,
    /// such as adding support for including non-key columns in index definitions.
    /// </remarks>
    public static partial class PostgresExtensions
    {
        /// <summary>
        /// Adds a non-key column to the index definition for PostgreSQL.
        /// </summary>
        /// <param name="expression">The index options syntax to extend.</param>
        /// <param name="columnName">The name of the column to include in the index.</param>
        /// <returns>The updated index options syntax.</returns>
        /// <remarks>
        /// This method is specific to PostgreSQL and allows the inclusion of non-key columns
        /// in an index definition. This is useful for scenarios where additional columns
        /// need to be included in the index for query optimization purposes.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the underlying <see cref="ISupportAdditionalFeatures"/> implementation
        /// does not support the <c>Include</c> method.
        /// </exception>
        public static ICreateIndexOptionsSyntax Include(this ICreateIndexOptionsSyntax expression, string columnName)
        {
            var additionalFeatures = expression as ISupportAdditionalFeatures;
            additionalFeatures.Include(columnName);
            return expression;
        }

        /// <summary>
        /// Adds a non-key column to the index definition for PostgreSQL.
        /// </summary>
        /// <param name="expression">The index column syntax to which the non-key column will be added.</param>
        /// <param name="columnName">The name of the column to include as a non-key column in the index.</param>
        /// <returns>An object that allows further configuration of the index definition.</returns>
        /// <remarks>
        /// This method is specific to PostgreSQL and enables the inclusion of non-key columns in an index.
        /// It extends the functionality of FluentMigrator for PostgreSQL-specific use cases.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the method is used in an unsupported context or with an invalid <paramref name="expression"/>.
        /// </exception>
        public static ICreateIndexNonKeyColumnSyntax Include(this ICreateIndexOnColumnSyntax expression, string columnName)
        {
            var additionalFeatures = expression as ISupportAdditionalFeatures;
            additionalFeatures.Include(columnName);
            return new CreateIndexExpressionNonKeyBuilder(expression, additionalFeatures);
        }

        /// <summary>
        /// Adds a column to the list of included columns for a PostgreSQL index.
        /// </summary>
        /// <param name="additionalFeatures">The object supporting additional features, such as included columns.</param>
        /// <param name="columnName">The name of the column to include in the index.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the <paramref name="additionalFeatures"/> does not support additional features.
        /// </exception>
        /// <remarks>
        /// This method is used to specify columns that are included in the index but are not part of the index key.
        /// </remarks>
        internal static void Include(this ISupportAdditionalFeatures additionalFeatures, string columnName)
        {
            if (additionalFeatures == null)
            {
                throw new InvalidOperationException(UnsupportedMethodMessage(nameof(Include), nameof(ISupportAdditionalFeatures)));
            }

            var includes = additionalFeatures.GetAdditionalFeature<IList<PostgresIndexIncludeDefinition>>(IncludesList, () => new List<PostgresIndexIncludeDefinition>());
            includes.Add(new PostgresIndexIncludeDefinition { Name = columnName });
        }
    }
}
