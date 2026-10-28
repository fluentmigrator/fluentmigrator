#region License
// Copyright (c) 2026, Fluent Migrator Project
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

namespace FluentMigrator
{
    /// <summary>
    /// Provider-agnostic control over how a unique index treats NULL values.
    /// </summary>
    /// <remarks>
    /// Database engines disagree on whether two NULLs conflict in a unique index. SQL Server
    /// and Db2 treat them as equal, so only one NULL row is permitted; PostgreSQL, SQLite,
    /// MySQL and Firebird treat them as distinct, so any number of NULL rows is permitted. A
    /// migration that relies on the default therefore means different things on different
    /// engines.
    ///
    /// These methods state the intent using the SQL:2023 <c>NULLS [NOT] DISTINCT</c> meaning,
    /// and each provider maps it to whatever native syntax expresses it. Where an engine cannot
    /// express the requested semantics the request is routed through the generator's
    /// compatibility mode rather than silently emitting a weaker constraint.
    ///
    /// The setting applies to the whole index, even though it is called from a column of the
    /// index. Requesting conflicting values on two columns of the same index throws.
    ///
    /// For the SQL that each provider emits, and what happens when a provider cannot express
    /// the request, see
    /// <see href="https://fluentmigrator.github.io/basics/indexes.html#null-handling-in-unique-indexes">NULL handling in unique indexes</see>.
    /// </remarks>
    public static class CreateIndexNullSemanticsExtensions
    {
        /// <summary>
        /// The additional-feature key carrying the requested NULL semantics.
        /// <c>true</c> means NULLs are distinct; <c>false</c> means NULLs are not distinct.
        /// </summary>
        public const string UniqueIndexNullsDistinct = "UniqueIndexNullsDistinct";

        /// <summary>
        /// NULLs are distinct: rows containing NULL in an indexed column never conflict with
        /// one another, so any number of them may exist.
        /// </summary>
        /// <remarks>
        /// Native on PostgreSQL, SQLite, MySQL and Firebird; emitted as <c>NULLS DISTINCT</c> on
        /// PostgreSQL 15 and later. On SQL Server 2008 and later this is emitted as a filtered
        /// index with <c>WHERE [column] IS NOT NULL</c> for each index column, combined with any
        /// <c>Filter()</c>. Other providers route the request through the compatibility mode. See
        /// <see href="https://fluentmigrator.github.io/basics/indexes.html#semantics-per-provider">semantics per provider</see>.
        /// </remarks>
        /// <param name="expression">The expression to set this option for</param>
        /// <returns>The <paramref name="expression"/></returns>
        /// <exception cref="InvalidOperationException">Another column of the same index requested <c>NullsNotDistinct()</c>.</exception>
        public static ICreateIndexColumnUniqueOptionsSyntax NullsDistinct(this ICreateIndexColumnUniqueOptionsSyntax expression)
            => SetNullsDistinct(expression, true);

        /// <summary>
        /// NULLs are not distinct: NULLs compare equal to one another, so at most one row may
        /// hold a given combination of values that includes NULL.
        /// </summary>
        /// <remarks>
        /// Native on SQL Server and Db2, where a plain <c>CREATE UNIQUE INDEX</c> is emitted. On
        /// PostgreSQL 15 and later this is emitted as <c>NULLS NOT DISTINCT</c>. Other providers
        /// route the request through the compatibility mode. See
        /// <see href="https://fluentmigrator.github.io/basics/indexes.html#semantics-per-provider">semantics per provider</see>.
        /// </remarks>
        /// <param name="expression">The expression to set this option for</param>
        /// <returns>The <paramref name="expression"/></returns>
        /// <exception cref="InvalidOperationException">Another column of the same index requested <c>NullsDistinct()</c>.</exception>
        public static ICreateIndexColumnUniqueOptionsSyntax NullsNotDistinct(this ICreateIndexColumnUniqueOptionsSyntax expression)
            => SetNullsDistinct(expression, false);

        private static ICreateIndexColumnUniqueOptionsSyntax SetNullsDistinct(ICreateIndexColumnUniqueOptionsSyntax expression, bool nullsDistinct)
        {
            if (expression is not ISupportAdditionalFeatures additionalFeatures)
            {
                throw new InvalidOperationException(
                    $"The used object does not implement {nameof(ISupportAdditionalFeatures)}.");
            }

            if (additionalFeatures.AdditionalFeatures.TryGetValue(UniqueIndexNullsDistinct, out var existing)
                && existing is bool existingValue
                && existingValue != nullsDistinct)
            {
                throw new InvalidOperationException(
                    "NULLS DISTINCT and NULLS NOT DISTINCT apply to the whole index, so they can't both be requested on the same index.");
            }

            additionalFeatures.AdditionalFeatures[UniqueIndexNullsDistinct] = nullsDistinct;
            return expression;
        }
    }
}
