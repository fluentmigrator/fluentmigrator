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

using FluentMigrator.Infrastructure;

namespace FluentMigrator.Builder.SecurityLabel;

/// <summary>
/// Defines the starting syntax for creating a security label.
/// </summary>
public interface ICreateSecurityLabelSyntax<TBuilder> : ICreateSecurityLabelOnObjectSyntax<TBuilder>
{
    /// <summary>
    /// Specifies the security label provider.
    /// </summary>
    /// <param name="provider">The name of the provider (e.g., "anon", "sepgsql").</param>
    /// <returns>The next step in the fluent syntax.</returns>
    ICreateSecurityLabelOnObjectSyntax<TBuilder> For(string provider);
}

/// <summary>
/// Defines the syntax for selecting the object type for the security label.
/// </summary>
public interface ICreateSecurityLabelOnObjectSyntax<TBuilder> : IFluentSyntax
{
    /// <summary>
    /// Specifies that the security label is for a table.
    /// </summary>
    /// <param name="tableName">The name of the table.</param>
    /// <returns>The next step in the fluent syntax.</returns>
    ICreateSecurityLabelOnTableSyntax<TBuilder> OnTable(string tableName);

    /// <summary>
    /// Specifies that the security label is for a column.
    /// </summary>
    /// <param name="columnName">The name of the column.</param>
    /// <returns>The next step in the fluent syntax.</returns>
    ICreateSecurityLabelOnColumnSyntax<TBuilder> OnColumn(string columnName);

    /// <summary>
    /// Specifies that the security label is for a schema.
    /// </summary>
    /// <param name="schemaName">The name of the schema.</param>
    /// <returns>The next step in the fluent syntax.</returns>
    ICreateSecurityLabelWithLabelSyntax<TBuilder> OnSchema(string schemaName);

    /// <summary>
    /// Specifies that the security label is for a role.
    /// </summary>
    /// <param name="roleName">The name of the role.</param>
    /// <returns>The next step in the fluent syntax.</returns>
    ICreateSecurityLabelWithLabelSyntax<TBuilder> OnRole(string roleName);

    /// <summary>
    /// Specifies that the security label is for a view.
    /// </summary>
    /// <param name="viewName">The name of the view.</param>
    /// <returns>The next step in the fluent syntax.</returns>
    ICreateSecurityLabelOnViewSyntax<TBuilder> OnView(string viewName);
}

/// <summary>
/// Defines the syntax for specifying a table's schema or proceeding with the label.
/// </summary>
public interface ICreateSecurityLabelOnTableSyntax<TBuilder> : ICreateSecurityLabelWithLabelSyntax<TBuilder>
{
    /// <summary>
    /// Specifies the schema containing the table.
    /// </summary>
    /// <param name="schemaName">The name of the schema.</param>
    /// <returns>The next step in the fluent syntax.</returns>
    ICreateSecurityLabelWithLabelSyntax<TBuilder> InSchema(string schemaName);
}

/// <summary>
/// Defines the syntax for specifying which table contains the column.
/// </summary>
public interface ICreateSecurityLabelOnColumnSyntax<TBuilder> : IFluentSyntax
{
    /// <summary>
    /// Specifies the table containing the column.
    /// </summary>
    /// <param name="tableName">The name of the table.</param>
    /// <returns>The next step in the fluent syntax.</returns>
    ICreateSecurityLabelOnColumnTableSyntax<TBuilder> OnTable(string tableName);
}

/// <summary>
/// Defines the syntax for specifying a column's table schema or proceeding with the label.
/// </summary>
public interface ICreateSecurityLabelOnColumnTableSyntax<TBuilder> : ICreateSecurityLabelWithLabelSyntax<TBuilder>
{
    /// <summary>
    /// Specifies the schema containing the table.
    /// </summary>
    /// <param name="schemaName">The name of the schema.</param>
    /// <returns>The next step in the fluent syntax.</returns>
    ICreateSecurityLabelWithLabelSyntax<TBuilder> InSchema(string schemaName);
}

/// <summary>
/// Defines the syntax for specifying a view's schema or proceeding with the label.
/// </summary>
public interface ICreateSecurityLabelOnViewSyntax<TBuilder> : ICreateSecurityLabelWithLabelSyntax<TBuilder>
{
    /// <summary>
    /// Specifies the schema containing the view.
    /// </summary>
    /// <param name="schemaName">The name of the schema.</param>
    /// <returns>The next step in the fluent syntax.</returns>
    ICreateSecurityLabelWithLabelSyntax<TBuilder> InSchema(string schemaName);
}

/// <summary>
/// Defines the syntax for specifying the label value.
/// </summary>
public interface ICreateSecurityLabelWithLabelSyntax<TBuilder> : IFluentSyntax
{
    /// <summary>
    /// Specifies the security label value as a raw string.
    /// </summary>
    /// <param name="label">The label value.</param>
    void WithLabel(string label);

    /// <summary>
    /// Specifies the security label using a strongly-typed PostgreSQL Anonymizer builder.
    /// This automatically sets the provider to "anon".
    /// </summary>
    /// <param name="configure">The action to configure the anonymizer label.</param>
    /// <example>
    /// <code>
    /// Create.SecurityLabel()
    ///     .OnColumn("email")
    ///     .OnTable("users")
    ///     .InSchema("public")
    ///     .WithLabel(label => label.MaskedWithFakeEmail());
    ///
    /// Create.SecurityLabel()
    ///     .OnColumn("name")
    ///     .OnTable("users")
    ///     .InSchema("public")
    ///     .WithLabel(label => label.MaskedWithValue("CONFIDENTIAL"));
    /// </code>
    /// </example>
    void WithLabel(Action<TBuilder> configure);
}
