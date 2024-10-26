#region License
// Copyright (c) 2007-2024, Fluent Migrator Project
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

namespace FluentMigrator.SqlServer
{
    public static partial class SqlServerExtensions
    {
        /// <summary>
        /// Column should have unique values, but multiple rows with null values should be accepted.
        /// </summary>
        /// <param name="expression">The expression to set this option for</param>
        /// <returns>The <paramref name="expression"/></returns>
        public static ICreateIndexColumnUniqueOptionsSyntax NullsNotDistinct(
            this ICreateIndexColumnUniqueOptionsSyntax expression)
        {
            return NullsDistinct(expression, false);
        }

        /// <summary>
        /// Column should have unique values. Only one row with null value should be accepted (default for most known database engines).
        /// </summary>
        /// <param name="expression">The expression to set this option for</param>
        /// <param name="nullsAreDistinct"><c>true</c> when nulls should be distinct</param>
        /// <returns>The <paramref name="expression"/></returns>
        public static ICreateIndexColumnUniqueOptionsSyntax NullsDistinct(
            this ICreateIndexColumnUniqueOptionsSyntax expression,
            bool nullsAreDistinct = true)
        {
            expression.CurrentColumn.AdditionalFeatures[IndexColumnNullsDistinct] = nullsAreDistinct;
            return expression;
        }

        /// <summary>
        /// Column should have unique values, but multiple rows with null values should be accepted.
        /// </summary>
        /// <param name="expression">The expression to set this option for</param>
        /// <returns>The <paramref name="expression"/></returns>
        public static ICreateIndexOnColumnSyntax NullsNotDistinct(
            this ICreateIndexMoreColumnOptionsSyntax expression)
        {
            return NullsDistinct(expression, false);
        }

        /// <summary>
        /// Column should have unique values. Only one row with null value should be accepted (default for most known database engines).
        /// </summary>
        /// <param name="expression">The expression to set this option for</param>
        /// <param name="nullsAreDistinct"><c>true</c> when nulls should be distinct</param>
        /// <returns>The <paramref name="expression"/></returns>
        public static ICreateIndexOnColumnSyntax NullsDistinct(
            this ICreateIndexMoreColumnOptionsSyntax expression,
            bool nullsAreDistinct = true)
        {
            expression.CurrentColumn.AdditionalFeatures[IndexColumnNullsDistinct] = nullsAreDistinct;
            return expression;
        }

        /// <summary>
        /// Index should have unique values, but multiple rows with null values should be accepted.
        /// </summary>
        /// <param name="expression">The expression to set this option for</param>
        /// <returns>The <paramref name="expression"/></returns>
        public static ICreateIndexOnColumnSyntax UniqueNullsNotDistinct(
            this ICreateIndexOptionsSyntax expression)
        {
            return UniqueNullsDistinct(expression, false);
        }

        /// <summary>
        /// Index should have unique values. Only one row with null value should be accepted (default for most known database engines).
        /// </summary>
        /// <param name="expression">The expression to set this option for</param>
        /// <param name="nullsAreDistinct"><c>true</c> when nulls should be distinct</param>
        /// <returns>The <paramref name="expression"/></returns>
        public static ICreateIndexOnColumnSyntax UniqueNullsDistinct(
            this ICreateIndexOptionsSyntax expression,
            bool nullsAreDistinct = true)
        {
            var additionalFeatures = expression as ISupportAdditionalFeatures ??
                throw new InvalidOperationException(UnsupportedMethodMessage("Nulls(Not)Distinct", nameof(ISupportAdditionalFeatures)));
            additionalFeatures.AdditionalFeatures[IndexColumnNullsDistinct] = nullsAreDistinct;
            return expression.Unique();
        }
    }
}
