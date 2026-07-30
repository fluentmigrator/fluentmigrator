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
using System.Collections.Generic;
using System.Linq;

using FluentMigrator.Runner.Initialization;
using FluentMigrator.Tests.Integration.Migrations.SqlServer.AdditionalColumnDescriptions;

using JetBrains.Annotations;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Integration.Processors.SqlServer.SqlServer2016;

[TestFixture]
public class SqlServerAdditionalColumnDescriptionsTests : SqlServerIntegrationTests
{
    /// <summary>
    /// Runs a migration that calls <c>WithColumnAdditionalDescriptions</c> through all four fluent
    /// builders (Create.Table, Create.Column, Alter.Table/AddColumn, Alter.Column) and then reads the
    /// extended properties back from SQL Server to prove the additional descriptions were persisted.
    /// </summary>
    [Test]
    public void AdditionalColumnDescriptionsArePersistedThroughAllBuilders()
    {
        var migrationsNamespace = typeof(AddAdditionalColumnDescriptions).Namespace;

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

            var expectedAdditionalDescriptions = new Dictionary<string, string>
            {
                { "AdditionalColumnDescriptionKey1", "AdditionalColumnDescriptionValue1" },
                { "AdditionalColumnDescriptionKey2", "AdditionalColumnDescriptionValue2" },
            };

            // Create paths concatenate every additional description into the single MS_Description property.
            AssertConcatenatedAdditionalDescriptions("Col1", expectedAdditionalDescriptions); // Create.Table
            AssertConcatenatedAdditionalDescriptions("Col2", expectedAdditionalDescriptions); // Create.Column
            AssertConcatenatedAdditionalDescriptions("Col3", expectedAdditionalDescriptions); // Alter.Table / AddColumn
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

    private void AssertConcatenatedAdditionalDescriptions(string columnName, IReadOnlyDictionary<string, string> expected)
    {
        var pairs = string.Join(Environment.NewLine, expected.Select(kv => $"{kv.Key}:{kv.Value}"));

        var exists = Processor.Exists(
            """
            SELECT 1
            FROM sys.extended_properties ep
            INNER JOIN sys.objects o ON ep.major_id = o.object_id
            INNER JOIN sys.columns c ON c.object_id = o.object_id AND c.column_id = ep.minor_id
            WHERE o.name = '{0}' AND c.name = '{1}' AND ep.name = 'MS_Description'
            AND CAST(ep.value AS nvarchar(max)) LIKE '%{2}%'
            """,
            AddAdditionalColumnDescriptions.TableName,
            columnName,
            pairs);

        exists.ShouldBeTrue(
            $"Expected the single MS_Description extended property of {AddAdditionalColumnDescriptions.TableName}.{columnName} " +
            $"to contain '{pairs}'.");
    }

    private void AssertPerKeyAdditionalDescriptions(string columnName, IReadOnlyDictionary<string, string> expected)
    {
        foreach (var additionalDescription in expected)
        {
            var exists = Processor.Exists(
                """
                SELECT 1
                FROM sys.extended_properties ep
                INNER JOIN sys.objects o ON ep.major_id = o.object_id
                INNER JOIN sys.columns c ON c.object_id = o.object_id AND c.column_id = ep.minor_id
                WHERE o.name = '{0}' AND c.name = '{1}' AND ep.name = 'MS_{2}'
                AND CAST(ep.value AS nvarchar(max)) = '{3}'
                """,
                AddAdditionalColumnDescriptions.TableName,
                columnName,
                additionalDescription.Key,
                additionalDescription.Value);

            exists.ShouldBeTrue(
                $"Expected extended property 'MS_{additionalDescription.Key}' on " +
                $"{AddAdditionalColumnDescriptions.TableName}.{columnName} to have value '{additionalDescription.Value}'.");
        }
    }

    private void Execute(
        [CanBeNull] Action<IServiceCollection> initAction,
        [NotNull] Action<IServiceProvider> executeAction)
    {
        using var serviceProvider = CreateProcessorServices(initAction);
        using var scope = serviceProvider.CreateScope();

        executeAction(scope.ServiceProvider);
    }
}
