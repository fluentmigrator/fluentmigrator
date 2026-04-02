#region License
//
// Copyright (c) 2007-2026, Fluent Migrator Project
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

using System;

using FluentMigrator.Builders.Create.Table;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Runner.Generators.SQLite;

/// <summary>
/// SQLite-specific extensions for FluentMigrator builders.
/// </summary>
// ReSharper disable once InconsistentNaming
public static class SQLiteExtensions
{
    /// <summary>
    /// The key used in <see cref="ISupportAdditionalFeatures.AdditionalFeatures"/> to indicate a WITHOUT ROWID table.
    /// </summary>
    public static readonly string WithoutRowIdTable = "SQLiteWithoutRowId";

    /// <summary>
    /// Creates the table as a WITHOUT ROWID table.
    /// </summary>
    /// <param name="expression">The created table expression builder.</param>
    /// <returns>The created table expression builder.</returns>
    /// <remarks>
    /// WITHOUT ROWID tables are only suitable for tables where every row satisfies a PRIMARY KEY constraint.
    /// See https://www.sqlite.org/withoutrowid.html for details.
    /// </remarks>
    public static ICreateTableWithColumnOrSchemaOrDescriptionSyntax WithoutRowId(
        this ICreateTableWithColumnOrSchemaOrDescriptionSyntax expression)
    {
        var additionalFeatures = GetAdditionalFeatures(expression, nameof(WithoutRowId));
        additionalFeatures.AdditionalFeatures[WithoutRowIdTable] = true;

        return expression;
    }

    private static ISupportAdditionalFeatures GetAdditionalFeatures(IFluentSyntax expression, string methodName)
    {
        if (expression is ISupportAdditionalFeatures supportAdditionalFeatures)
            return supportAdditionalFeatures;

        throw new InvalidOperationException(
            string.Format(
                ErrorMessages.MethodXMustBeCalledOnObjectImplementingY,
                methodName,
                nameof(ISupportAdditionalFeatures)));
    }
}
