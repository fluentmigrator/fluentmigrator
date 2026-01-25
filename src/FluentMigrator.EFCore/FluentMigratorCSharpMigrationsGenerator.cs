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

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations.Design;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace FluentMigrator.EFCore
{
    public class FluentMigratorCSharpMigrationsGenerator : CSharpMigrationsGenerator
    {
        public FluentMigratorCSharpMigrationsGenerator(
            MigrationsCodeGeneratorDependencies dependencies,
            CSharpMigrationsGeneratorDependencies csharpDependencies)
            : base(dependencies, csharpDependencies)
        {
        }

        public override string GenerateMigration(
            string migrationNamespace,
            string migrationName,
            IReadOnlyList<MigrationOperation> upOperations,
            IReadOnlyList<MigrationOperation> downOperations)
        {
            var builder = new IndentedStringBuilder();
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");

            builder.AppendLine("using FluentMigrator;");
            builder.AppendLine();
            builder.AppendLine($"namespace {migrationNamespace}");
            builder.AppendLine("{");

            using (builder.Indent())
            {
                builder.AppendLine($"[Migration({timestamp})]");
                builder.AppendLine($"public class {migrationName} : Migration");
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
            var operationGenerator = CSharpDependencies.CSharpMigrationOperationGenerator as FluentMigratorCSharpMigrationOperationGenerator;

            if (operationGenerator == null)
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
                    default:
                        builder.AppendLine($"// Unsupported operation: {operation.GetType().Name}");
                        break;
                }
            }
        }

        public override string GenerateMetadata(
            string migrationNamespace,
            Type contextType,
            string migrationName,
            string migrationId,
            IModel targetModel)
        {
            // Generate EF Core metadata with a different class name to avoid conflicts
            var metadata = base.GenerateMetadata(migrationNamespace, contextType, migrationName, migrationId, targetModel);

            // Replace the class name to avoid duplicate definition
            metadata = metadata.Replace($"partial class {migrationName}", $"partial class {migrationName}Metadata");

            // Remove the 'override' keyword since we're not inheriting from Migration anymore
            metadata = metadata.Replace("protected override void BuildTargetModel", "protected void BuildTargetModel");

            // Remove the Migration attribute - this is metadata only, not a FluentMigrator migration
            // The attribute line looks like: [Migration("migrationId")]
            var lines = metadata.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            var result = new System.Text.StringBuilder();

            foreach (var line in lines)
            {
                // Skip lines that contain the Migration attribute
                if (!line.Trim().StartsWith("[Migration("))
                {
                    result.AppendLine(line);
                }
            }

            return result.ToString();
        }

        public override string GenerateSnapshot(
            string modelSnapshotNamespace,
            Type contextType,
            string modelSnapshotName,
            IModel model)
        {
            // Generate EF Core snapshot for change detection
            return base.GenerateSnapshot(modelSnapshotNamespace, contextType, modelSnapshotName, model);
        }
    }
}
