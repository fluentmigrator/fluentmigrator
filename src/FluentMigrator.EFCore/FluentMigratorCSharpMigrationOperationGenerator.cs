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
using System.Linq;

using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Design;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.Extensions.Options;

namespace FluentMigrator.EFCore;

/// <summary>
/// Generates FluentMigrator fluent API code from EF Core <see cref="MigrationOperation"/> instances.
/// </summary>
public class FluentMigratorCSharpMigrationOperationGenerator : CSharpMigrationOperationGenerator
{
    private readonly FluentMigratorOptions _options;

    public FluentMigratorCSharpMigrationOperationGenerator(
        CSharpMigrationOperationGeneratorDependencies dependencies,
        IOptions<FluentMigratorOptions> options)
        : base(dependencies)
    {
        _options = options.Value;
    }

    private ICSharpHelper Code => Dependencies.CSharpHelper;

    protected override void Generate(CreateTableOperation operation, IndentedStringBuilder builder)
    {
        GenerateCreateTable(operation, builder);
    }

    private string WriteTableName(string name)
    {
        return _options.TableNameWriter.Invoke(Code, name);
    }

    private string InSchema(string? schema)
    {
        return schema is null ? string.Empty : $".InSchema({Code.Literal(schema)})";
    }

    public void GenerateCreateTable(CreateTableOperation operation, IndentedStringBuilder builder)
    {
        var tableName = WriteTableName(operation.Name);

        builder.AppendLine($"Create.Table({tableName}){InSchema(operation.Schema)}");
        using (builder.Indent())
        {
            foreach (var column in operation.Columns)
            {
                var isPrimaryKey = operation.PrimaryKey != null && operation.PrimaryKey.Columns.Contains(column.Name);
                GenerateColumnDefinition(column, builder, isPrimaryKey, true);
            }
        }
        builder.AppendLine(";");
        builder.AppendLine();

        // Generate foreign keys if they exist in the table operation
        foreach (var foreignKey in operation.ForeignKeys)
        {
            GenerateAddForeignKey(foreignKey, builder);
        }
    }

    protected override void Generate(AddColumnOperation operation, IndentedStringBuilder builder)
    {
        GenerateAddColumn(operation, builder);
    }

    public void GenerateAddColumn(AddColumnOperation operation, IndentedStringBuilder builder)
    {
        var tableName = WriteTableName(operation.Table);

        builder.AppendLine($"Alter.Table({tableName}){InSchema(operation.Schema)}");
        using (builder.Indent())
        {
            GenerateColumnDefinition(operation, builder, false);
        }
        builder.AppendLine(";");
    }

    protected override void Generate(DropTableOperation operation, IndentedStringBuilder builder)
    {
        GenerateDropTable(operation, builder);
    }

    public void GenerateDropTable(DropTableOperation operation, IndentedStringBuilder builder)
    {
        var tableName = WriteTableName(operation.Name);

        builder.AppendLine($"Delete.Table({tableName}){InSchema(operation.Schema)};");
    }

    protected override void Generate(DropColumnOperation operation, IndentedStringBuilder builder)
    {
        GenerateDropColumn(operation, builder);
    }

    public void GenerateDropColumn(DropColumnOperation operation, IndentedStringBuilder builder)
    {
        var tableName = WriteTableName(operation.Table);

        builder.AppendLine($"Delete.Column({Code.Literal(operation.Name)}).FromTable({tableName}){InSchema(operation.Schema)};");
    }

    protected override void Generate(AlterColumnOperation operation, IndentedStringBuilder builder)
    {
        GenerateAlterColumn(operation, builder);
    }

    public void GenerateAlterColumn(AlterColumnOperation operation, IndentedStringBuilder builder)
    {
        var tableName = WriteTableName(operation.Table);

        builder.AppendLine($"Alter.Table({tableName}){InSchema(operation.Schema)}");
        using (builder.Indent())
        {
            GenerateAlterColumnDefinition(operation, builder);
        }
        builder.AppendLine(";");
    }

    protected override void Generate(RenameTableOperation operation, IndentedStringBuilder builder)
    {
        GenerateRenameTable(operation, builder);
    }

    public void GenerateRenameTable(RenameTableOperation operation, IndentedStringBuilder builder)
    {
        if (operation.NewName is null || operation.NewName == operation.Name)
        {
            builder.AppendLine($"// Moving table {operation.Name} to schema {operation.NewSchema} is not supported by FluentMigrator");
            return;
        }

        var tableName = WriteTableName(operation.Name);
        var newTableName = WriteTableName(operation.NewName);

        builder.AppendLine($"Rename.Table({tableName}){InSchema(operation.Schema)}.To({newTableName});");
    }

    protected override void Generate(RenameColumnOperation operation, IndentedStringBuilder builder)
    {
        GenerateRenameColumn(operation, builder);
    }

    public void GenerateRenameColumn(RenameColumnOperation operation, IndentedStringBuilder builder)
    {
        var tableName = WriteTableName(operation.Table);

        builder.AppendLine($"Rename.Column({Code.Literal(operation.Name)}).OnTable({tableName}){InSchema(operation.Schema)}.To({Code.Literal(operation.NewName)});");
    }

    protected override void Generate(AddForeignKeyOperation operation, IndentedStringBuilder builder)
    {
        GenerateAddForeignKey(operation, builder);
    }

    public void GenerateAddForeignKey(AddForeignKeyOperation operation, IndentedStringBuilder builder)
    {
        var columns = string.Join(", ", operation.Columns.Select(c => Code.Literal(c)));
        var principalColumns = string.Join(", ", (operation.PrincipalColumns ?? []).Select(c => Code.Literal(c)));
        var tableName = WriteTableName(operation.Table);
        var principalTable = WriteTableName(operation.PrincipalTable);

        builder.AppendLine($"Create.ForeignKey({Code.Literal(operation.Name)})");
        using (builder.Indent())
        {
            builder.AppendLine($".FromTable({tableName}){InSchema(operation.Schema)}.ForeignColumns({columns})");
            builder.Append($".ToTable({principalTable}){InSchema(operation.PrincipalSchema)}.PrimaryColumns({principalColumns})");

            var onDelete = GetCascadeRule(operation.OnDelete);
            if (onDelete != null)
            {
                builder.AppendLine();
                builder.Append($".OnDelete({onDelete})");
            }

            var onUpdate = GetCascadeRule(operation.OnUpdate);
            if (onUpdate != null)
            {
                builder.AppendLine();
                builder.Append($".OnUpdate({onUpdate})");
            }

            builder.AppendLine(";");
        }
        builder.AppendLine();
    }

    private static string? GetCascadeRule(ReferentialAction action)
    {
        return action switch
        {
            ReferentialAction.Cascade => "System.Data.Rule.Cascade",
            ReferentialAction.SetNull => "System.Data.Rule.SetNull",
            ReferentialAction.SetDefault => "System.Data.Rule.SetDefault",
            _ => null,
        };
    }

    protected override void Generate(DropForeignKeyOperation operation, IndentedStringBuilder builder)
    {
        GenerateDropForeignKey(operation, builder);
    }

    public void GenerateDropForeignKey(DropForeignKeyOperation operation, IndentedStringBuilder builder)
    {
        var tableName = WriteTableName(operation.Table);

        builder.AppendLine($"Delete.ForeignKey({Code.Literal(operation.Name)}).OnTable({tableName}){InSchema(operation.Schema)};");
    }

    protected override void Generate(AddPrimaryKeyOperation operation, IndentedStringBuilder builder)
    {
        GenerateAddPrimaryKey(operation, builder);
    }

    public void GenerateAddPrimaryKey(AddPrimaryKeyOperation operation, IndentedStringBuilder builder)
    {
        var tableName = WriteTableName(operation.Table);
        var columns = string.Join(", ", operation.Columns.Select(c => Code.Literal(c)));
        var withSchema = operation.Schema is null ? string.Empty : $".WithSchema({Code.Literal(operation.Schema)})";

        builder.AppendLine($"Create.PrimaryKey({Code.Literal(operation.Name)}).OnTable({tableName}){withSchema}.Columns({columns});");
    }

    protected override void Generate(DropPrimaryKeyOperation operation, IndentedStringBuilder builder)
    {
        GenerateDropPrimaryKey(operation, builder);
    }

    public void GenerateDropPrimaryKey(DropPrimaryKeyOperation operation, IndentedStringBuilder builder)
    {
        var tableName = WriteTableName(operation.Table);

        builder.AppendLine($"Delete.PrimaryKey({Code.Literal(operation.Name)}).FromTable({tableName}){InSchema(operation.Schema)};");
    }

    protected override void Generate(AddUniqueConstraintOperation operation, IndentedStringBuilder builder)
    {
        GenerateAddUniqueConstraint(operation, builder);
    }

    public void GenerateAddUniqueConstraint(AddUniqueConstraintOperation operation, IndentedStringBuilder builder)
    {
        var tableName = WriteTableName(operation.Table);
        var columns = string.Join(", ", operation.Columns.Select(c => Code.Literal(c)));
        var withSchema = operation.Schema is null ? string.Empty : $".WithSchema({Code.Literal(operation.Schema)})";

        builder.AppendLine($"Create.UniqueConstraint({Code.Literal(operation.Name)}).OnTable({tableName}){withSchema}.Columns({columns});");
    }

    protected override void Generate(DropUniqueConstraintOperation operation, IndentedStringBuilder builder)
    {
        GenerateDropUniqueConstraint(operation, builder);
    }

    public void GenerateDropUniqueConstraint(DropUniqueConstraintOperation operation, IndentedStringBuilder builder)
    {
        var tableName = WriteTableName(operation.Table);

        builder.AppendLine($"Delete.UniqueConstraint({Code.Literal(operation.Name)}).FromTable({tableName}){InSchema(operation.Schema)};");
    }

    protected override void Generate(EnsureSchemaOperation operation, IndentedStringBuilder builder)
    {
        GenerateEnsureSchema(operation, builder);
    }

    public void GenerateEnsureSchema(EnsureSchemaOperation operation, IndentedStringBuilder builder)
    {
        builder.AppendLine($"Create.Schema({Code.Literal(operation.Name)});");
    }

    protected override void Generate(DropSchemaOperation operation, IndentedStringBuilder builder)
    {
        GenerateDropSchema(operation, builder);
    }

    public void GenerateDropSchema(DropSchemaOperation operation, IndentedStringBuilder builder)
    {
        builder.AppendLine($"Delete.Schema({Code.Literal(operation.Name)});");
    }

    protected override void Generate(SqlOperation operation, IndentedStringBuilder builder)
    {
        GenerateSql(operation, builder);
    }

    public void GenerateSql(SqlOperation operation, IndentedStringBuilder builder)
    {
        builder.AppendLine($"Execute.Sql({Code.Literal(operation.Sql)});");
    }

    protected override void Generate(CreateIndexOperation operation, IndentedStringBuilder builder)
    {
        GenerateCreateIndex(operation, builder);
    }

    public void GenerateCreateIndex(CreateIndexOperation operation, IndentedStringBuilder builder)
    {
        var tableName = WriteTableName(operation.Table);

        builder.Append($"Create.Index({Code.Literal(operation.Name)})");
        builder.Append($".OnTable({tableName}){InSchema(operation.Schema)}");

        for (var i = 0; i < operation.Columns.Length; i++)
        {
            var columnName = operation.Columns[i];
            builder.Append($".OnColumn({Code.Literal(columnName)})");

            var direction = IsColumnDescending(operation, i) ? "Descending" : "Ascending";
            builder.Append($".{direction}()");
        }

        if (operation.IsUnique)
        {
            builder.Append(".WithOptions().Unique()");
        }

        builder.AppendLine(";");
    }

    private static bool IsColumnDescending(CreateIndexOperation operation, int columnIndex)
    {
        // EF Core semantics: null means all ascending, an empty array means all
        // descending, otherwise one entry per column.
        return operation.IsDescending switch
        {
            null => false,
            { Length: 0 } => true,
            var isDescending => isDescending[columnIndex],
        };
    }

    protected override void Generate(DropIndexOperation operation, IndentedStringBuilder builder)
    {
        GenerateDropIndex(operation, builder);
    }

    public void GenerateDropIndex(DropIndexOperation operation, IndentedStringBuilder builder)
    {
        if (string.IsNullOrEmpty(operation.Table))
        {
            builder.AppendLine($"Delete.Index({Code.Literal(operation.Name)});");
            return;
        }

        var tableName = WriteTableName(operation.Table);

        builder.AppendLine($"Delete.Index({Code.Literal(operation.Name)}).OnTable({tableName}){InSchema(operation.Schema)};");
    }

    protected override void Generate(InsertDataOperation operation, IndentedStringBuilder builder)
    {
        GenerateInsertData(operation, builder);
    }

    public void GenerateInsertData(InsertDataOperation operation, IndentedStringBuilder builder)
    {
        var tableName = WriteTableName(operation.Table);

        builder.AppendLine($"Insert.IntoTable({tableName}){InSchema(operation.Schema)}");
        using (builder.Indent())
        {
            for (var row = 0; row < operation.Values.GetLength(0); row++)
            {
                builder.AppendLine(".Row(new");
                builder.AppendLine("{");

                using (builder.Indent())
                {
                    for (var column = 0; column < operation.Columns.Length; column++)
                    {
                        var value = Code.UnknownLiteral(operation.Values[row, column]);
                        builder.AppendLine($"{Code.Identifier(operation.Columns[column])} = {value},");
                    }
                }

                builder.AppendLine("})");
            }
        }

        builder.AppendLine(";");
    }

    private void GenerateColumnDefinition(
        AddColumnOperation column,
        IndentedStringBuilder builder,
        bool isPrimaryKey,
        bool isCreateTable = false)
    {
        var columnName = column.Name;

        var line = isCreateTable ?
            $".WithColumn({Code.Literal(columnName)})" :
            $".AddColumn({Code.Literal(columnName)})";

        line += GetFluentMigratorType(column);
        line += GetCommonColumnOptions(column, isPrimaryKey);

        builder.AppendLine(line);
    }

    private void GenerateAlterColumnDefinition(AlterColumnOperation column, IndentedStringBuilder builder)
    {
        var line = $".AlterColumn({Code.Literal(column.Name)})";

        line += GetFluentMigratorType(column);
        line += GetCommonColumnOptions(column, false);

        builder.AppendLine(line);
    }

    private string GetCommonColumnOptions(ColumnOperation column, bool isPrimaryKey)
    {
        var line = column.IsNullable ? ".Nullable()" : ".NotNullable()";

        if (isPrimaryKey)
        {
            line += ".PrimaryKey()";
        }

        if (column.DefaultValue != null)
        {
            line += $".WithDefaultValue({Code.UnknownLiteral(column.DefaultValue)})";
        }
        else if (column.DefaultValueSql != null)
        {
            line += $".WithDefaultValue(RawSql.Insert({Code.Literal(column.DefaultValueSql)}))";
        }

        if (column.Comment != null)
        {
            line += $".WithColumnDescription({Code.Literal(column.Comment)})";
        }

        return line;
    }

    protected static string GetDecimalTypeOptions(ColumnOperation column)
    {
        if (column is { Precision: not null, Scale: not null })
        {
            return $"{column.Precision.Value}, {column.Scale.Value}";
        }

        if (column.Precision.HasValue)
        {
            return $"{column.Precision.Value}";
        }

        return string.Empty;
    }

    private string GetFluentMigratorType(ColumnOperation column)
    {
        var underlyingType = Nullable.GetUnderlyingType(column.ClrType) ?? column.ClrType;

        // TODO Handle ColumnType in priority over ClrType? (because of jsonb, etc)

        if (underlyingType == typeof(int))
        {
            return ".AsInt32()";
        }

        if (underlyingType == typeof(long))
        {
            return ".AsInt64()";
        }

        if (underlyingType == typeof(short))
        {
            return ".AsInt16()";
        }

        if (underlyingType == typeof(byte))
        {
            return ".AsByte()";
        }

        if (underlyingType == typeof(bool))
        {
            return ".AsBoolean()";
        }

        if (underlyingType == typeof(string))
        {
            string options;
            if (column.MaxLength.HasValue)
            {
                options = $"{(column.MaxLength.Value == int.MaxValue ? "int.MaxValue" : column.MaxLength)}";

                if (column.Collation != null)
                {
                    options += $", {Code.Literal(column.Collation)}";
                }
            }
            else
            {
                options = column.Collation != null ? Code.Literal(column.Collation) : string.Empty;
            }

            if (column.IsFixedLength == true)
            {
                return column.IsUnicode == false ?
                    $".AsFixedLengthAnsiString({options})" :
                    $".AsFixedLengthString({options})";
            }

            return column.IsUnicode == false ?
                $".AsAnsiString({options})" :
                $".AsString({options})";
        }

        if (underlyingType == typeof(DateTime))
        {
            return ".AsDateTime()";
        }

        if (underlyingType == typeof(DateTimeOffset))
        {
            return ".AsDateTimeOffset()";
        }

        if (underlyingType == typeof(decimal))
        {
            return $".AsDecimal({GetDecimalTypeOptions(column)})";
        }

        if (underlyingType == typeof(double))
        {
            return ".AsDouble()";
        }

        if (underlyingType == typeof(float))
        {
            return ".AsFloat()";
        }

        if (underlyingType == typeof(Guid))
        {
            return ".AsGuid()";
        }

        if (underlyingType == typeof(byte[]))
        {
            return ".AsBinary()";
        }

        return ".AsCustom(" + Code.Literal(column.ColumnType) + ")";
    }
}
