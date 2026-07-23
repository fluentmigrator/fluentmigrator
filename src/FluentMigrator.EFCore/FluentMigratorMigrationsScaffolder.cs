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

using Microsoft.EntityFrameworkCore.Migrations.Design;

namespace FluentMigrator.EFCore;

public class FluentMigratorMigrationsScaffolder : MigrationsScaffolder
{
    /// <inheritdoc />
    public FluentMigratorMigrationsScaffolder(MigrationsScaffolderDependencies dependencies) : base(dependencies)
    {
    }

    /// <inheritdoc />
    public override MigrationFiles Save(string projectDir, ScaffoldedMigration migration, string? outputDir, bool dryRun)
    {
        var result = base.Save(projectDir, migration, outputDir, dryRun);

        // Remove metadata file as it's not needed for FluentMigrator
        if (string.IsNullOrEmpty(result.MetadataFile))
        {
            return result;
        }

        System.IO.File.Delete(result.MetadataFile);
        result.MetadataFile = null;

        return result;
    }
}
