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

using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Postgres;

namespace FluentMigrator.Builder.SecurityLabel;

/// <summary>
/// Builds an expression to create a security label on a PostgreSQL object.
/// </summary>
public class CreateSecurityLabelExpressionBuilder<TBuilder> :
    ICreateSecurityLabelSyntax<TBuilder>,
    ICreateSecurityLabelOnObjectSyntax<TBuilder>,
    ICreateSecurityLabelOnTableSyntax<TBuilder>,
    ICreateSecurityLabelOnColumnSyntax<TBuilder>,
    ICreateSecurityLabelOnColumnTableSyntax<TBuilder>,
    ICreateSecurityLabelOnViewSyntax<TBuilder>,
    ICreateSecurityLabelWithLabelSyntax<TBuilder>
    where TBuilder : ISecurityLabelSyntaxBuilder, new()
{
    private readonly IMigrationContext _context;
    private readonly PostgresSecurityLabelDefinition _definition;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateSecurityLabelExpressionBuilder{TBuilder}"/> class.
    /// </summary>
    /// <param name="context">The migration context.</param>
    public CreateSecurityLabelExpressionBuilder(IMigrationContext context)
    {
        _context = context;
        _definition = new PostgresSecurityLabelDefinition();
    }

    /// <inheritdoc />
    public ICreateSecurityLabelOnObjectSyntax<TBuilder> For(string provider)
    {
        _definition.Provider = provider;
        return this;
    }

    /// <inheritdoc />
    public ICreateSecurityLabelOnTableSyntax<TBuilder> OnTable(string tableName)
    {
        _definition.ObjectType = PostgresSecurityLabelObjectType.Table;
        _definition.ObjectName = tableName;
        return this;
    }

    /// <inheritdoc />
    public ICreateSecurityLabelOnColumnSyntax<TBuilder> OnColumn(string columnName)
    {
        _definition.ObjectType = PostgresSecurityLabelObjectType.Column;
        _definition.ColumnName = columnName;
        return this;
    }

    /// <inheritdoc />
    public ICreateSecurityLabelWithLabelSyntax<TBuilder> OnSchema(string schemaName)
    {
        _definition.ObjectType = PostgresSecurityLabelObjectType.Schema;
        _definition.ObjectName = schemaName;
        return this;
    }

    /// <inheritdoc />
    public ICreateSecurityLabelWithLabelSyntax<TBuilder> OnRole(string roleName)
    {
        _definition.ObjectType = PostgresSecurityLabelObjectType.Role;
        _definition.ObjectName = roleName;
        return this;
    }

    /// <inheritdoc />
    public ICreateSecurityLabelOnViewSyntax<TBuilder> OnView(string viewName)
    {
        _definition.ObjectType = PostgresSecurityLabelObjectType.View;
        _definition.ObjectName = viewName;
        return this;
    }

    /// <inheritdoc />
    ICreateSecurityLabelWithLabelSyntax<TBuilder> ICreateSecurityLabelOnTableSyntax<TBuilder>.InSchema(string schemaName)
    {
        _definition.SchemaName = schemaName;
        return this;
    }

    /// <inheritdoc />
    ICreateSecurityLabelOnColumnTableSyntax<TBuilder> ICreateSecurityLabelOnColumnSyntax<TBuilder>.OnTable(string tableName)
    {
        _definition.ObjectName = tableName;
        return this;
    }

    /// <inheritdoc />
    ICreateSecurityLabelWithLabelSyntax<TBuilder> ICreateSecurityLabelOnColumnTableSyntax<TBuilder>.InSchema(string schemaName)
    {
        _definition.SchemaName = schemaName;
        return this;
    }

    /// <inheritdoc />
    ICreateSecurityLabelWithLabelSyntax<TBuilder> ICreateSecurityLabelOnViewSyntax<TBuilder>.InSchema(string schemaName)
    {
        _definition.SchemaName = schemaName;
        return this;
    }

    /// <inheritdoc />
    public void WithLabel(string label)
    {
        _definition.Label = label;
        AddExpression();
    }

    /// <inheritdoc />
    public void WithLabel(Action<TBuilder> configure)
    {
        if (configure is null)
        {
            throw new ArgumentNullException(nameof(configure));
        }

        var builder = new TBuilder();
        configure(builder);

        _definition.Provider = builder.ProviderName;
        _definition.Label = builder.Build();

        AddExpression();
    }

    private void AddExpression()
    {
        var sql = PostgresSecurityLabelSqlGenerator.GenerateCreateSecurityLabelSql(_definition);
        var expression = new ExecuteSqlStatementExpression { SqlStatement = sql };
        _context.Expressions.Add(expression);
    }
}
