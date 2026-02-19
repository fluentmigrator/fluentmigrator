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

using FluentMigrator.Builders.Create.Index;
using FluentMigrator.Infrastructure;
using FluentMigrator.Infrastructure.Extensions;

namespace FluentMigrator.Postgres
{
    /// <summary>
    /// Provides extension methods and constants for PostgreSQL-specific functionality in FluentMigrator.
    /// </summary>
    public static partial class PostgresExtensions
    {
        /// <summary>
        /// Represents the key used to store or retrieve the filter condition for PostgreSQL index creation.
        /// </summary>
        /// <remarks>
        /// This constant is utilized in conjunction with the <see cref="Filter"/> method
        /// to specify a WHERE clause for an index in PostgreSQL.
        /// </remarks>
        public const string IndexFilter = "PostgresIndexFilter";

        /// <summary>
        /// The constraint expression for a partial index.
        /// For more information about partial index see: https://www.postgresql.org/docs/current/indexes-partial.html
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="filter">The constraint expression</param>
        /// <returns>The next step</returns>
        public static ICreateIndexOptionsSyntax Filter(this ICreateIndexOptionsSyntax expression, string filter)
        {
            var additionalFeatures = expression as ISupportAdditionalFeatures;
            additionalFeatures.SetAdditionalFeature(IndexFilter, filter);
            return expression;
        }
    }
}
