#region License
// Copyright (c) 2018, Fluent Migrator Project
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

using FluentMigrator.Runner.Initialization;
using FluentMigrator.Tests.Integration.Migrations.SqlServer.UniqueConstraintInclude;

using JetBrains.Annotations;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Integration.Processors.SqlServer.SqlServer2016;

[TestFixture]
public class SqlServerUniqueConstraintIncludeTests : SqlServerIntegrationTests
{
    /// <summary>
    /// Runs a migration that creates a unique constraint with an included column via
    /// <c>Create.UniqueConstraint(...).Column(...).Include(...)</c> and reads the resulting index back
    /// from <c>sys.indexes</c>/<c>sys.index_columns</c>. It proves a unique index is created with the
    /// key column as a key column and the included column as a non-key (INCLUDE) column. Before the fix
    /// the include column was silently dropped and only a plain UNIQUE constraint was created.
    /// </summary>
    [Test]
    public void UniqueConstraintIncludeCreatesCoveringUniqueIndex()
    {
        var migrationsNamespace = typeof(AddUniqueConstraintWithInclude).Namespace;

        try
        {
            Execute(
                services => services
                    .Configure<RunnerOptions>(opt => opt.Task = "migrate")
                    .WithMigrationsIn(migrationsNamespace),
                sp =>
                {
                    var task = sp.GetRequiredService<TaskExecutor>();
                    task.Execute();
                });

            // The key column must be present as a key column (is_included_column = 0) of a UNIQUE index
            // that carries the constraint name.
            var keyColumnIsKey = Processor.Exists(
                IndexColumnQuery,
                AddUniqueConstraintWithInclude.TableName,
                AddUniqueConstraintWithInclude.ConstraintName,
                AddUniqueConstraintWithInclude.KeyColumn,
                0);

            keyColumnIsKey.ShouldBeTrue(
                $"Expected column {AddUniqueConstraintWithInclude.KeyColumn} to be a key column of the unique index " +
                $"{AddUniqueConstraintWithInclude.ConstraintName}.");

            // The included column must be present as a non-key INCLUDE column (is_included_column = 1).
            // This is the assertion that fails before the fix, because the include is silently dropped.
            var includedColumnIsIncluded = Processor.Exists(
                IndexColumnQuery,
                AddUniqueConstraintWithInclude.TableName,
                AddUniqueConstraintWithInclude.ConstraintName,
                AddUniqueConstraintWithInclude.IncludedColumn,
                1);

            includedColumnIsIncluded.ShouldBeTrue(
                $"Expected column {AddUniqueConstraintWithInclude.IncludedColumn} to be an INCLUDE (non-key) column of " +
                $"the unique index {AddUniqueConstraintWithInclude.ConstraintName}.");
        }
        finally
        {
            Execute(
                services => services.AddScoped<TaskExecutor>()
                    .Configure<RunnerOptions>(opt => opt.Task = "rollback:all")
                    .WithMigrationsIn(migrationsNamespace),
                sp =>
                {
                    var task = sp.GetRequiredService<TaskExecutor>();
                    task.Execute();
                });
        }
    }

    private const string IndexColumnQuery =
        """
        SELECT 1
        FROM sys.indexes i
        INNER JOIN sys.objects o ON i.object_id = o.object_id
        INNER JOIN sys.index_columns ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id
        INNER JOIN sys.columns c ON c.object_id = i.object_id AND c.column_id = ic.column_id
        WHERE o.name = '{0}' AND i.name = '{1}' AND i.is_unique = 1
        AND c.name = '{2}' AND ic.is_included_column = {3}
        """;

    private void Execute(
        [CanBeNull] Action<IServiceCollection> initAction,
        [NotNull] Action<IServiceProvider> executeAction)
    {
        using var serviceProvider = CreateProcessorServices(initAction);
        using var scope = serviceProvider.CreateScope();

        executeAction(scope.ServiceProvider);
    }
}
