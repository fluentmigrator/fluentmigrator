#region License
// Copyright (c) 2024, Fluent Migrator Project
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
using System.Collections.Generic;

using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations.Design;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.Extensions.Options;

namespace FluentMigrator.EFCore;

public class FluentMigratorCSharpMigrationsGenerator : CSharpMigrationsGenerator
{
    private readonly FluentMigratorOptions _options;

    public FluentMigratorCSharpMigrationsGenerator(
        MigrationsCodeGeneratorDependencies dependencies,
        CSharpMigrationsGeneratorDependencies csharpDependencies,
        IOptions<FluentMigratorOptions> options)
        : base(dependencies, csharpDependencies)
    {
        _options = options.Value;
    }

    private ICSharpHelper Code => CSharpDependencies.CSharpHelper;

    public override string GenerateMigration(
        string? migrationNamespace,
        string migrationName,
        IReadOnlyList<MigrationOperation> upOperations,
        IReadOnlyList<MigrationOperation> downOperations)
    {
        var builder = new IndentedStringBuilder();
        var timestamp = _options.TimestampProvider(_options.TimestampFormat);

        builder.AppendLine("using FluentMigrator;");

        // Additional usings from options
        foreach (var additionalUsing in _options.AdditionalUsings)
        {
            builder.AppendLine($"using {additionalUsing};");
        }
        builder.AppendLine();

        builder.AppendLine($"namespace {migrationNamespace}");
        builder.AppendLine("{");

        using (builder.Indent())
        {
            builder.AppendLine($"[Migration({timestamp})]");

            // Add Tags attribute if specified
            if (_options.DefaultTags.Count > 0)
            {
                foreach (var tagSet in _options.DefaultTags)
                {
                    var tagsArray = string.Join(", ", tagSet.ConvertAll(tag => Code.Literal(tag)));
                    builder.AppendLine($"[Tags({tagsArray})]");
                }
            }

            builder.AppendLine($"public class {migrationName} : {_options.BaseMigrationClass}");
            builder.AppendLine("{");

            using (builder.Indent())
            {
                // Up method
                builder.AppendLine("public override void Up()");
                builder.AppendLine("{");
                using (builder.Indent())
                {
                    GenerateFluentMigratorOperations(upOperations, builder);
                }
                builder.AppendLine("}");
                builder.AppendLine();

                // Down method
                builder.AppendLine("public override void Down()");
                builder.AppendLine("{");
                using (builder.Indent())
                {
                    GenerateFluentMigratorOperations(downOperations, builder);
                }
                builder.AppendLine("}");
            }

            builder.AppendLine("}");
        }

        builder.AppendLine("}");

        return builder.ToString();
    }

    private void GenerateFluentMigratorOperations(IReadOnlyList<MigrationOperation> operations, IndentedStringBuilder builder)
    {
        // Get our custom operation generator
        if (CSharpDependencies.CSharpMigrationOperationGenerator is not FluentMigratorCSharpMigrationOperationGenerator operationGenerator)
        {
            // Fallback if not our custom generator
            CSharpDependencies.CSharpMigrationOperationGenerator.Generate("", operations, builder);
            return;
        }

        // Call our custom generator methods directly for each operation
        foreach (var operation in operations)
        {
            switch (operation)
            {
                case CreateTableOperation createTableOp:
                    operationGenerator.GenerateCreateTable(createTableOp, builder);
                    break;
                case AddColumnOperation addColumnOp:
                    operationGenerator.GenerateAddColumn(addColumnOp, builder);
                    break;
                case DropTableOperation dropTableOp:
                    operationGenerator.GenerateDropTable(dropTableOp, builder);
                    break;
                case DropColumnOperation dropColumnOp:
                    operationGenerator.GenerateDropColumn(dropColumnOp, builder);
                    break;
                case AlterColumnOperation alterColumnOp:
                    operationGenerator.GenerateAlterColumn(alterColumnOp, builder);
                    break;
                case AddForeignKeyOperation addFkOp:
                    operationGenerator.GenerateAddForeignKey(addFkOp, builder);
                    break;
                case CreateIndexOperation createIndexOp:
                    operationGenerator.GenerateCreateIndex(createIndexOp, builder);
                    break;
                case DropIndexOperation dropIndexOp:
                    operationGenerator.GenerateDropIndex(dropIndexOp, builder);
                    break;
                case InsertDataOperation insertDataOp:
                    operationGenerator.GenerateInsertData(insertDataOp, builder);
                    break;
                default:
                    builder.AppendLine($"// Unsupported operation: {operation.GetType().Name}");
                    break;
            }
        }
    }

    public override string GenerateMetadata(
        string? migrationNamespace,
        Type contextType,
        string migrationName,
        string migrationId,
        IModel targetModel)
    {
        return string.Empty;
    }
}
