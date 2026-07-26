#region License
//
// Copyright (c) 2007-2024, Fluent Migrator Project
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

using FluentMigrator.Runner;
using FluentMigrator.Runner.Infrastructure;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FluentMigrator.Tests.Aot;

/// <summary>
/// End-to-end AOT smoke test that runs a real migration against an in-memory SQLite database.
/// Exercises the full pipeline: DI → runner → validation → SQL generation → execution.
/// </summary>
public class MigrationRunnerTests
{
    [Test]
    [DisplayName("Full migration pipeline works in AOT (Up + Down)")]
    public async Task CanRunMigrationUpAndDown()
    {
        var serviceProvider = new ServiceCollection()
            .AddLogging(lb => lb.SetMinimumLevel(LogLevel.Warning))
            .AddFluentMigratorSlim()
            .ConfigureRunner(rb => rb
                .AddSQLite()
                .WithGlobalConnectionString("Data Source=:memory:")
                .WithTypes(typeof(CreateUsersTable)))
            .BuildServiceProvider();

        var runner = serviceProvider.GetRequiredService<IMigrationRunner>();

        runner.MigrateUp();
        await Assert.That(runner.HasMigrationsToApplyUp()).IsFalse();

        runner.MigrateDown(0);
        await Assert.That(runner.HasMigrationsToApplyUp()).IsTrue();
    }
}

[Migration(1)]
public class CreateUsersTable : Migration
{
    public override void Up()
    {
        Create.Table("Users")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("Name").AsString(100).NotNullable()
            .WithColumn("Email").AsString(255).Nullable();
    }

    public override void Down()
    {
        Delete.Table("Users");
    }
}
