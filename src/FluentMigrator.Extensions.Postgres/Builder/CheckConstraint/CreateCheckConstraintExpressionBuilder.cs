#region License
// Copyright (c) 2026, Fluent Migrator Project
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
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

namespace FluentMigrator.Builder.CheckConstraint;

internal sealed class CreateCheckConstraintExpressionBuilder :
    ICreateCheckConstraintOnTableSyntax,
    ICreateCheckConstraintInSchemaSyntax
{
    private readonly IMigrationContext _context;
    private readonly string _constraintName;
    private string _tableName;
    private string _schemaName;

    public CreateCheckConstraintExpressionBuilder(IMigrationContext context, string constraintName)
    {
        _context = context;
        ValidateIdentifier(constraintName, nameof(constraintName));
        _constraintName = constraintName;
    }

    public ICreateCheckConstraintInSchemaSyntax OnTable(string tableName)
    {
        ValidateIdentifier(tableName, nameof(tableName));
        _tableName = tableName;
        return this;
    }

    public ICreateCheckConstraintWithConditionSyntax InSchema(string schemaName)
    {
        if (!string.IsNullOrEmpty(schemaName))
        {
            ValidateIdentifier(schemaName, nameof(schemaName));
        }

        _schemaName = schemaName;
        return this;
    }

    public void WithCondition(string condition)
    {
        if (string.IsNullOrWhiteSpace(condition))
        {
            throw new ArgumentException("The check condition must contain a SQL expression.", nameof(condition));
        }

        var resolver = _context.ServiceProvider?.GetService(typeof(IDefaultSchemaNameResolver)) as IDefaultSchemaNameResolver;
        var resolvedSchema = resolver == null ? _schemaName : resolver.GetSchemaName(_schemaName);
        _context.Expressions.Add(new CreateCheckConstraintExpression(_constraintName, _tableName, resolvedSchema, condition));
    }

    private static void ValidateIdentifier(string identifier, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(identifier))
        {
            throw new ArgumentException("The identifier must contain a name.", parameterName);
        }
    }
}
