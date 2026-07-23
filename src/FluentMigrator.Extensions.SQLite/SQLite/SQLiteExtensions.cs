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

using FluentMigrator.Builders.Create.Table;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.SQLite
{
    /// <summary>
    /// Feature extensions for SQLite.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public static class SQLiteExtensions
    {
        /// <summary>
        /// The key used to store the WITHOUT ROWID table option in <see cref="ISupportAdditionalFeatures.AdditionalFeatures"/>.
        /// </summary>
        public static readonly string WithoutRowIdTable = "SQLiteWithoutRowId";

        /// <summary>
        /// Creates the table using the SQLite <c>WITHOUT ROWID</c> optimization (requires SQLite 3.8.2 or later).
        /// </summary>
        /// <param name="expression">The table creation expression.</param>
        /// <returns>The same expression builder to allow further configuration.</returns>
        /// <remarks>
        /// WITHOUT ROWID tables omit the implicit integer primary key (rowid), which can improve performance
        /// for tables that are frequently accessed by their declared primary key. The table must have a
        /// declared PRIMARY KEY. See https://www.sqlite.org/withoutrowid.html for details.
        /// </remarks>
        /// <example>
        /// <code>
        /// Create.Table("WordIndex")
        ///     .WithColumn("word").AsString().NotNullable().PrimaryKey()
        ///     .WithColumn("idx").AsInt64().NotNullable().PrimaryKey()
        ///     .WithoutRowId();
        /// </code>
        /// </example>
        public static ICreateTableColumnOptionOrWithColumnSyntax WithoutRowId(
            this ICreateTableColumnOptionOrWithColumnSyntax expression)
        {
            if (expression is not ISupportAdditionalFeatures castExpression)
                throw new InvalidOperationException(
                    string.Format(ErrorMessages.MethodXMustBeCalledOnObjectImplementingY,
                        nameof(WithoutRowId),
                        nameof(ISupportAdditionalFeatures)));
            castExpression.AdditionalFeatures[WithoutRowIdTable] = true;
            return expression;
        }
    }
}
