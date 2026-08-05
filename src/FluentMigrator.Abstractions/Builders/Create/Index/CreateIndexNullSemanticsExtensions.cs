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
    /// and Db2 treat them as equal, so only one NULL row is permitted; PostgreSQL, SQLite and
    /// MySQL treat them as distinct, so any number of NULL rows is permitted. A migration that
    /// relies on the default therefore means different things on different engines.
    ///
    /// These methods state the intent instead, and each provider maps it to whatever native
    /// syntax expresses it. Where an engine cannot express the requested semantics the request
    /// is routed through the runner's compatibility mode rather than silently emitting a
    /// weaker constraint.
    ///
    /// The provider-specific extensions in <c>FluentMigrator.Postgres</c> and
    /// <c>FluentMigrator.SqlServer</c> are unaffected and take precedence when both are set.
    /// </remarks>
    public static class CreateIndexNullSemanticsExtensions
    {
        /// <summary>
        /// The additional-feature key carrying the requested NULL semantics.
        /// <c>true</c> means NULLs are distinct; <c>false</c> means NULLs compare equal.
        /// </summary>
        public const string UniqueIndexNullsDistinct = "UniqueIndexNullsDistinct";

        /// <summary>
        /// Index values must be unique, and rows containing NULL in an indexed column never
        /// conflict with one another, so any number of them may exist.
        /// </summary>
        /// <remarks>
        /// Native on PostgreSQL, SQLite and MySQL. On SQL Server 2008 and later this is
        /// emitted as a filtered index excluding NULL rows.
        /// </remarks>
        /// <param name="expression">The expression to set this option for</param>
        /// <returns>The <paramref name="expression"/></returns>
        public static ICreateIndexOnColumnSyntax UniqueTreatNullsAsDistinct(this ICreateIndexOptionsSyntax expression)
            => SetNullsDistinct(expression, true);

        /// <summary>
        /// Index values must be unique, and NULLs compare equal to one another, so at most one
        /// row may hold NULL in the indexed columns.
        /// </summary>
        /// <remarks>
        /// Native on SQL Server. On PostgreSQL 15 and later this is emitted as
        /// <c>NULLS NOT DISTINCT</c>.
        /// </remarks>
        /// <param name="expression">The expression to set this option for</param>
        /// <returns>The <paramref name="expression"/></returns>
        public static ICreateIndexOnColumnSyntax UniqueTreatNullsAsEqual(this ICreateIndexOptionsSyntax expression)
            => SetNullsDistinct(expression, false);

        private static ICreateIndexOnColumnSyntax SetNullsDistinct(ICreateIndexOptionsSyntax expression, bool nullsAreDistinct)
        {
            if (expression is not ISupportAdditionalFeatures additionalFeatures)
            {
                throw new InvalidOperationException(
                    $"The used object does not implement {nameof(ISupportAdditionalFeatures)}.");
            }

            additionalFeatures.AdditionalFeatures[UniqueIndexNullsDistinct] = nullsAreDistinct;
            return expression.Unique();
        }
    }
}
