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

using FluentMigrator.Infrastructure;

namespace FluentMigrator.Postgres.Builder.SecurityLabel
{
    /// <summary>
    /// Defines the starting syntax for deleting a security label.
    /// </summary>
    public interface IDeleteSecurityLabelFromObjectSyntax : IFluentSyntax
    {
        /// <summary>
        /// Specifies that the security label is being deleted from a table.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <returns>The next step in the fluent syntax.</returns>
        IDeleteSecurityLabelFromTableSyntax FromTable(string tableName);

        /// <summary>
        /// Specifies that the security label is being deleted from a column.
        /// </summary>
        /// <param name="columnName">The name of the column.</param>
        /// <returns>The next step in the fluent syntax.</returns>
        IDeleteSecurityLabelFromColumnSyntax FromColumn(string columnName);

        /// <summary>
        /// Specifies that the security label is being deleted from a schema.
        /// </summary>
        /// <param name="schemaName">The name of the schema.</param>
        /// <returns>The next step in the fluent syntax.</returns>
        IDeleteSecurityLabelWithProviderSyntax FromSchema(string schemaName);

        /// <summary>
        /// Specifies that the security label is being deleted from a role.
        /// </summary>
        /// <param name="roleName">The name of the role.</param>
        /// <returns>The next step in the fluent syntax.</returns>
        IDeleteSecurityLabelWithProviderSyntax FromRole(string roleName);

        /// <summary>
        /// Specifies that the security label is being deleted from a view.
        /// </summary>
        /// <param name="viewName">The name of the view.</param>
        /// <returns>The next step in the fluent syntax.</returns>
        IDeleteSecurityLabelFromViewSyntax FromView(string viewName);
    }

    /// <summary>
    /// Defines the syntax for specifying a table's schema or proceeding with provider.
    /// </summary>
    public interface IDeleteSecurityLabelFromTableSyntax : IDeleteSecurityLabelWithProviderSyntax
    {
        /// <summary>
        /// Specifies the schema containing the table.
        /// </summary>
        /// <param name="schemaName">The name of the schema.</param>
        /// <returns>The next step in the fluent syntax.</returns>
        IDeleteSecurityLabelWithProviderSyntax InSchema(string schemaName);
    }

    /// <summary>
    /// Defines the syntax for specifying which table contains the column.
    /// </summary>
    public interface IDeleteSecurityLabelFromColumnSyntax : IFluentSyntax
    {
        /// <summary>
        /// Specifies the table containing the column.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <returns>The next step in the fluent syntax.</returns>
        IDeleteSecurityLabelFromColumnTableSyntax OnTable(string tableName);
    }

    /// <summary>
    /// Defines the syntax for specifying a column's table schema or proceeding with provider.
    /// </summary>
    public interface IDeleteSecurityLabelFromColumnTableSyntax : IDeleteSecurityLabelWithProviderSyntax
    {
        /// <summary>
        /// Specifies the schema containing the table.
        /// </summary>
        /// <param name="schemaName">The name of the schema.</param>
        /// <returns>The next step in the fluent syntax.</returns>
        IDeleteSecurityLabelWithProviderSyntax InSchema(string schemaName);
    }

    /// <summary>
    /// Defines the syntax for specifying a view's schema or proceeding with provider.
    /// </summary>
    public interface IDeleteSecurityLabelFromViewSyntax : IDeleteSecurityLabelWithProviderSyntax
    {
        /// <summary>
        /// Specifies the schema containing the view.
        /// </summary>
        /// <param name="schemaName">The name of the schema.</param>
        /// <returns>The next step in the fluent syntax.</returns>
        IDeleteSecurityLabelWithProviderSyntax InSchema(string schemaName);
    }

    /// <summary>
    /// Defines the syntax for specifying the provider for the label to delete.
    /// </summary>
    public interface IDeleteSecurityLabelWithProviderSyntax : IFluentSyntax
    {
        /// <summary>
        /// Specifies the security label provider and deletes the label.
        /// </summary>
        /// <param name="provider">The name of the provider (e.g., "anon", "sepgsql").</param>
        void WithProvider(string provider);

        /// <summary>
        /// Deletes the security label (without a specific provider).
        /// </summary>
        void Delete();
    }
}
