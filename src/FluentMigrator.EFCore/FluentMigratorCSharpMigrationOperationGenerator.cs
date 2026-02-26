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

    private string WriteTableName(string name)
    {
        return _options.TableNameWriter.Invoke(Code, name);
    }

    public void GenerateCreateTable(CreateTableOperation operation, IndentedStringBuilder builder)
    {
        var tableName = WriteTableName(operation.Name);

        builder.AppendLine($"Create.Table({tableName})");
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
                var columns = string.Join(", ", foreignKey.Columns.Select(c => Code.Literal(c)));
                var principalColumns = string.Join(", ", (foreignKey.PrincipalColumns ?? []).Select(c => Code.Literal(c)));
                var principalTable = WriteTableName(foreignKey.PrincipalTable);

                builder.AppendLine($"Create.ForeignKey({Code.Literal(foreignKey.Name)})");
                using (builder.Indent())
                {
                    builder.AppendLine($".FromTable({tableName}).ForeignColumns({columns})");
                    builder.AppendLine($".ToTable({principalTable}).PrimaryColumns({principalColumns});");
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
        var tableName = WriteTableName(operation.Name);

        builder.AppendLine($"Alter.Table({tableName})");
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

        builder.AppendLine($"Delete.Table({tableName});");
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
        var tableName = WriteTableName(operation.Name);

        builder.AppendLine($"Alter.Table({tableName})");
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
        var tableName = WriteTableName(operation.Table);

        builder.Append($"Create.Index({Code.Literal(operation.Name)})");
        builder.Append($".OnTable({tableName})");

        for (var i = 0; i < operation.Columns.Length; i++)
        {
            var columnName = operation.Columns[i];
            builder.Append($".OnColumn({Code.Literal(columnName)})");

            if (operation.IsUnique)
            {
                builder.Append(".Unique()");
            }
            else
            {
                var direction = operation.IsDescending?[i] == true ? "Descending" : "Ascending";
                builder.Append($".{direction}()");
            }
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

        var tableName = WriteTableName(operation.Table);

        builder.AppendLine($"Delete.Index({Code.Literal(operation.Name)}).OnTable({tableName});");
    }

    public void GenerateInsertData(InsertDataOperation operation, IndentedStringBuilder builder)
    {
        var tableName = WriteTableName(operation.Table);

        builder.AppendLine($"Insert.IntoTable({tableName})");
        using (builder.Indent())
        {
            builder.AppendLine(".Rows([");

            using (builder.Indent())
            {
                foreach (var valueSet in operation.Values)
                {
                    builder.AppendLine("new");
                    builder.AppendLine("{");

                    using (builder.Indent())
                    {
                        foreach (var t in operation.Columns)
                        {
                            var value = Code.UnknownLiteral(valueSet);
                            builder.AppendLine($"{t} = {value},");
                        }
                    }

                    builder.AppendLine("},");
                }
            }

            builder.AppendLine("])");
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

        builder.AppendLine(line);
    }

    private void GenerateAlterColumnDefinition(AlterColumnOperation column, IndentedStringBuilder builder)
    {
        var line = $".AlterColumn({Code.Literal(column.Name)})";

        line += GetFluentMigratorType(column);

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

        builder.AppendLine(line);
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
