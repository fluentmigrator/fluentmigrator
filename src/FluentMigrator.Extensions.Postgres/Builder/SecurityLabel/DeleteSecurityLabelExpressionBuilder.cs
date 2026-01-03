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

using System.Collections.Generic;

using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Model;
using FluentMigrator.Postgres;

namespace FluentMigrator.Builder.SecurityLabel;

/// <summary>
/// Builds an expression to delete a security label from a PostgreSQL object.
/// </summary>
public class DeleteSecurityLabelExpressionBuilder :
    IDeleteSecurityLabelSyntax,
    IDeleteSecurityLabelFromTableSyntax,
    IDeleteSecurityLabelFromColumnSyntax,
    IDeleteSecurityLabelFromColumnTableSyntax,
    IDeleteSecurityLabelFromViewSyntax
{
    private readonly IMigrationContext _context;
    private readonly PostgresSecurityLabelDefinition _definition;

    private bool ExpressionAdded { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteSecurityLabelExpressionBuilder"/> class.
    /// </summary>
    /// <param name="context">The migration context.</param>
    public DeleteSecurityLabelExpressionBuilder(IMigrationContext context)
    {
        _context = context;
        _definition = new PostgresSecurityLabelDefinition();
    }

    /// <summary>
    /// Specifies the security label provider for which the label should be deleted.
    /// </summary>
    /// <param name="provider">The name of the security label provider (for example, <c>selinux</c>).</param>
    /// <returns>The current <see cref="IDeleteSecurityLabelSyntax"/> instance to continue the fluent configuration.</returns>
    public IDeleteSecurityLabelSyntax For(string provider)
    {
        _definition.Provider = provider;
        return this;
    }

    /// <inheritdoc />
    public IDeleteSecurityLabelFromTableSyntax FromTable(string tableName)
    {
        _definition.ObjectType = PostgresSecurityLabelObjectType.Table;
        _definition.ObjectName = tableName;
        AddExpression();
        return this;
    }

    /// <inheritdoc />
    public IDeleteSecurityLabelFromColumnSyntax FromColumn(string columnName)
    {
        _definition.ObjectType = PostgresSecurityLabelObjectType.Column;
        _definition.ColumnName = columnName;
        return this;
    }

    /// <inheritdoc />
    public void FromSchema(string schemaName)
    {
        _definition.ObjectType = PostgresSecurityLabelObjectType.Schema;
        _definition.ObjectName = schemaName;
        AddExpression();
    }

    /// <inheritdoc />
    public void FromRole(string roleName)
    {
        _definition.ObjectType = PostgresSecurityLabelObjectType.Role;
        _definition.ObjectName = roleName;
        AddExpression();
    }

    /// <inheritdoc />
    public IDeleteSecurityLabelFromViewSyntax FromView(string viewName)
    {
        _definition.ObjectType = PostgresSecurityLabelObjectType.View;
        _definition.ObjectName = viewName;
        AddExpression();
        return this;
    }

    /// <inheritdoc />
    void IDeleteSecurityLabelFromTableSyntax.InSchema(string schemaName)
    {
        _definition.SchemaName = schemaName;
        AddExpression();
    }

    /// <inheritdoc />
    IDeleteSecurityLabelFromColumnTableSyntax IDeleteSecurityLabelFromColumnSyntax.OnTable(string tableName)
    {
        _definition.ObjectName = tableName;
        AddExpression();
        return this;
    }

    /// <inheritdoc />
    void IDeleteSecurityLabelFromColumnTableSyntax.InSchema(string schemaName)
    {
        _definition.SchemaName = schemaName;
        AddExpression();
    }

    /// <inheritdoc />
    void IDeleteSecurityLabelFromViewSyntax.InSchema(string schemaName)
    {
        _definition.SchemaName = schemaName;
        AddExpression();
    }

    private void AddExpression()
    {
        // Remove the last added expression if it was added previously.
        // This ensures that only one expression is added for the delete operation.
        // Note: IMigrationExpression.Expressions should be a list.
        if (ExpressionAdded && _context.Expressions is IList<IMigrationExpression> { Count: > 0 } list)
        {
            list.RemoveAt(_context.Expressions.Count - 1);
        }

        var sql = PostgresSecurityLabelSqlGenerator.GenerateDeleteSecurityLabelSql(_definition);
        var expression = new ExecuteSqlStatementExpression { SqlStatement = sql };
        _context.Expressions.Add(expression);

        ExpressionAdded = true;
    }
}
