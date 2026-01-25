using System;
using System.Linq;

using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations.Design;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.Extensions.Options;

namespace FluentMigrator.EFCore;

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

    private string TransformTableName(string name)
    {
        return _options.TableNameTransformer.Invoke(name);
    }

    private string TransformColumnName(string name)
    {
        return _options.ColumnNameTransformer.Invoke(name);
    }

    public void GenerateCreateTable(CreateTableOperation operation, IndentedStringBuilder builder)
    {
        var tableName = TransformTableName(operation.Name);

        builder.AppendLine($"Create.Table({Code.Literal(tableName)})");
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
        if (operation.ForeignKeys.Count > 0)
        {
            foreach (var foreignKey in operation.ForeignKeys)
            {
                var columns = string.Join(", ", foreignKey.Columns.Select(c => Code.Literal(TransformColumnName(c))));
                var principalColumns = string.Join(", ", (foreignKey.PrincipalColumns ?? []).Select(c => Code.Literal(TransformColumnName(c))));
                var principalTable = TransformTableName(foreignKey.PrincipalTable);

                builder.AppendLine($"Create.ForeignKey({Code.Literal(foreignKey.Name)})");
                using (builder.Indent())
                {
                    builder.AppendLine($".FromTable({Code.Literal(tableName)}).ForeignColumns({columns})");
                    builder.AppendLine($".ToTable({Code.Literal(principalTable)}).PrimaryColumns({principalColumns});");
                }
                builder.AppendLine();
            }
        }
    }

    protected override void Generate(AddColumnOperation operation, IndentedStringBuilder builder)
    {
        GenerateAddColumn(operation, builder);
    }

    public void GenerateAddColumn(AddColumnOperation operation, IndentedStringBuilder builder)
    {
        var tableName = TransformTableName(operation.Name);

        builder.AppendLine($"Alter.Table({Code.Literal(tableName)})");
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
        var tableName = TransformTableName(operation.Name);

        builder.AppendLine($"Delete.Table({Code.Literal(tableName)});");
    }

    protected override void Generate(DropColumnOperation operation, IndentedStringBuilder builder)
    {
        GenerateDropColumn(operation, builder);
    }

    public void GenerateDropColumn(DropColumnOperation operation, IndentedStringBuilder builder)
    {
        builder.AppendLine($"Delete.Column({Code.Literal(operation.Name)}).FromTable({Code.Literal(operation.Table)});");
    }

    protected override void Generate(AlterColumnOperation operation, IndentedStringBuilder builder)
    {
        GenerateAlterColumn(operation, builder);
    }

    public void GenerateAlterColumn(AlterColumnOperation operation, IndentedStringBuilder builder)
    {
        var tableName = TransformTableName(operation.Name);

        builder.AppendLine($"Alter.Table({Code.Literal(tableName)})");
        using (builder.Indent())
        {
            GenerateAlterColumnDefinition(operation, builder);
        }
        builder.AppendLine(";");
    }

    protected override void Generate(AddForeignKeyOperation operation, IndentedStringBuilder builder)
    {
        GenerateAddForeignKey(operation, builder);
    }

    public void GenerateAddForeignKey(AddForeignKeyOperation operation, IndentedStringBuilder builder)
    {
        var columns = string.Join(", ", operation.Columns.Select(c => Code.Literal(c)));
        var principalColumns = string.Join(", ", (operation.PrincipalColumns ?? []).Select(c => Code.Literal(c)));

        builder.AppendLine($"Create.ForeignKey({Code.Literal(operation.Name)})");
        using (builder.Indent())
        {
            builder.AppendLine($".FromTable({Code.Literal(operation.Table)}).ForeignColumns({columns})");
            builder.AppendLine($".ToTable({Code.Literal(operation.PrincipalTable)}).PrimaryColumns({principalColumns});");
        }
        builder.AppendLine();
    }

    protected override void Generate(CreateIndexOperation operation, IndentedStringBuilder builder)
    {
        GenerateCreateIndex(operation, builder);
    }

    public void GenerateCreateIndex(CreateIndexOperation operation, IndentedStringBuilder builder)
    {
        var tableName = TransformTableName(operation.Table);

        var columns = operation.Columns.Length == 1
            ? Code.Literal(TransformColumnName(operation.Columns[0]))
            : $"new[] {{ {string.Join(", ", operation.Columns.Select(c => Code.Literal(TransformColumnName(c))))} }}";

        builder.Append($"Create.Index({Code.Literal(operation.Name)})");
        builder.Append($".OnTable({Code.Literal(tableName)}).OnColumn({columns})");

        if (operation.IsUnique)
        {
            builder.Append(".Unique()");
        }

        builder.AppendLine(";");
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

        var tableName = TransformTableName(operation.Table);

        builder.AppendLine($"Delete.Index({Code.Literal(operation.Name)}).OnTable({Code.Literal(tableName)});");
    }

    private void GenerateColumnDefinition(
        AddColumnOperation column,
        IndentedStringBuilder builder,
        bool isPrimaryKey,
        bool isCreateTable = false)
    {
        var columnName = TransformColumnName(column.Name);

        var line = isCreateTable ?
            $".WithColumn({Code.Literal(columnName)})" :
            $".AddColumn({Code.Literal(columnName)})";

        line += $".As{GetFluentMigratorType(column.ClrType, column.ColumnType)}()";

        if (column.IsNullable)
        {
            line += ".Nullable()";
        }
        else
        {
            line += ".NotNullable()";
        }

        if (isPrimaryKey)
        {
            line += ".PrimaryKey()";
        }

        if (column.DefaultValue != null)
        {
            line += $".WithDefaultValue({Code.UnknownLiteral(column.DefaultValue)})";
        }

        if (column.IsUnicode == false)
        {
            line += ".AsAnsiString()";
        }

        if (column.MaxLength.HasValue)
        {
            line += $".WithSize({column.MaxLength.Value})";
        }

        builder.AppendLine(line);
    }

    private void GenerateAlterColumnDefinition(AlterColumnOperation column, IndentedStringBuilder builder)
    {
        var line = $".AlterColumn({Code.Literal(column.Name)})";

        line += $".As{GetFluentMigratorType(column.ClrType, column.ColumnType)}()";

        if (column.IsNullable)
        {
            line += ".Nullable()";
        }
        else
        {
            line += ".NotNullable()";
        }

        if (column.DefaultValue != null)
        {
            line += $".WithDefaultValue({Code.UnknownLiteral(column.DefaultValue)})";
        }

        if (column.IsUnicode == false)
        {
            line += ".AsAnsiString()";
        }

        if (column.MaxLength.HasValue)
        {
            line += $".WithSize({column.MaxLength.Value})";
        }

        builder.AppendLine(line);
    }

    private string GetFluentMigratorType(Type clrType, string? columnType)
    {
        var underlyingType = Nullable.GetUnderlyingType(clrType) ?? clrType;

        if (underlyingType == typeof(int)) return "Int32";
        if (underlyingType == typeof(long)) return "Int64";
        if (underlyingType == typeof(short)) return "Int16";
        if (underlyingType == typeof(byte)) return "Byte";
        if (underlyingType == typeof(bool)) return "Boolean";
        if (underlyingType == typeof(string)) return "String";
        if (underlyingType == typeof(DateTime)) return "DateTime";
        if (underlyingType == typeof(decimal)) return "Decimal";
        if (underlyingType == typeof(double)) return "Double";
        if (underlyingType == typeof(float)) return "Float";
        if (underlyingType == typeof(Guid)) return "Guid";
        if (underlyingType == typeof(byte[])) return "Binary";

        return "String"; // fallback
    }
}
