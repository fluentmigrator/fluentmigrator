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
using System.Text;

using FluentMigrator.Model;
using FluentMigrator.Postgres;

namespace FluentMigrator.Builder.SecurityLabel;

/// <summary>
/// Generates SQL statements for PostgreSQL security labels.
/// </summary>
public static class PostgresSecurityLabelSqlGenerator
{
    /// <summary>
    /// Generates a SQL statement to create or set a security label on an object.
    /// </summary>
    /// <param name="definition">The security label definition.</param>
    /// <returns>The generated SQL statement.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="definition"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the label in <paramref name="definition"/> is null or whitespace.</exception>
    public static string GenerateCreateSecurityLabelSql(PostgresSecurityLabelDefinition definition)
    {
        if (definition is null)
        {
            throw new ArgumentNullException(nameof(definition));
        }

        if (string.IsNullOrWhiteSpace(definition.Label))
        {
            throw new ArgumentException("Label cannot be null or whitespace.", nameof(definition));
        }

        var builder = new StringBuilder("SECURITY LABEL");

        AppendProvider(builder, definition.Provider);
        AppendObjectClause(builder, definition);
        AppendLabelValue(builder, definition.Label);

        builder.Append(';');
        return builder.ToString();
    }

    /// <summary>
    /// Generates a SQL statement to delete (set to NULL) a security label on an object.
    /// </summary>
    /// <param name="definition">The security label definition.</param>
    /// <returns>The generated SQL statement.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="definition"/> is null.</exception>
    public static string GenerateDeleteSecurityLabelSql(PostgresSecurityLabelDefinition definition)
    {
        if (definition is null)
        {
            throw new ArgumentNullException(nameof(definition));
        }

        var builder = new StringBuilder("SECURITY LABEL");

        AppendProvider(builder, definition.Provider);
        AppendObjectClause(builder, definition);
        builder.Append(" IS NULL;");

        return builder.ToString();
    }

    private static void AppendProvider(StringBuilder builder, string provider)
    {
        if (!string.IsNullOrWhiteSpace(provider))
        {
            builder.Append(" FOR ");
            builder.Append(QuoteIdentifier(provider));
        }
    }

    private static void AppendObjectClause(StringBuilder builder, PostgresSecurityLabelDefinition definition)
    {
        builder.Append(" ON ");

        switch (definition.ObjectType)
        {
            case PostgresSecurityLabelObjectType.Table:
                builder.Append("TABLE ");
                builder.Append(QuoteQualifiedName(definition.SchemaName, definition.ObjectName));
                break;

            case PostgresSecurityLabelObjectType.Column:
                builder.Append("COLUMN ");
                AppendColumnName(builder, definition);
                break;

            case PostgresSecurityLabelObjectType.Schema:
                builder.Append("SCHEMA ");
                builder.Append(QuoteIdentifier(definition.ObjectName));
                break;

            case PostgresSecurityLabelObjectType.Role:
                builder.Append("ROLE ");
                builder.Append(QuoteIdentifier(definition.ObjectName));
                break;

            case PostgresSecurityLabelObjectType.View:
                builder.Append("VIEW ");
                builder.Append(QuoteQualifiedName(definition.SchemaName, definition.ObjectName));
                break;

            default:
                throw new ArgumentException($"Unsupported object type: {definition.ObjectType}", nameof(definition));
        }
    }

    private static void AppendColumnName(StringBuilder builder, PostgresSecurityLabelDefinition definition)
    {
        if (string.IsNullOrWhiteSpace(definition.ColumnName))
        {
            throw new ArgumentException("Column name must be specified for column security labels.", nameof(definition));
        }

        if (string.IsNullOrWhiteSpace(definition.ObjectName))
        {
            throw new ArgumentException("Table name must be specified for column security labels.", nameof(definition));
        }

        var qualifiedTable = QuoteQualifiedName(definition.SchemaName, definition.ObjectName);
        builder.Append(qualifiedTable);
        builder.Append('.');
        builder.Append(QuoteIdentifier(definition.ColumnName));
    }

    private static void AppendLabelValue(StringBuilder builder, string label)
    {
        builder.Append(" IS ");
        builder.Append(QuoteLiteral(label));
    }

    private static string QuoteQualifiedName(string schemaName, string objectName)
    {
        if (string.IsNullOrWhiteSpace(schemaName))
        {
            return QuoteIdentifier(objectName);
        }

        return $"{QuoteIdentifier(schemaName)}.{QuoteIdentifier(objectName)}";
    }

    private static string QuoteIdentifier(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
        {
            throw new ArgumentException("Identifier cannot be null or whitespace.", nameof(identifier));
        }

        // Escape any double quotes in the identifier by doubling them
        var escaped = identifier.Replace("\"", "\"\"");
        return $"\"{escaped}\"";
    }

    private static string QuoteLiteral(string value)
    {
        if (value is null)
        {
            return "NULL";
        }

        // Escape any single quotes in the value by doubling them
        var escaped = value.Replace("'", "''");
        return $"'{escaped}'";
    }
}
