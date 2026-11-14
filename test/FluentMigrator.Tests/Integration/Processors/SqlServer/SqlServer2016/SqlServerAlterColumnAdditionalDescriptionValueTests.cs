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
using FluentMigrator.Tests.Integration.Migrations.SqlServer.AlterColumnAdditionalDescriptionValue;

using JetBrains.Annotations;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Integration.Processors.SqlServer.SqlServer2016
{
    [TestFixture]
    public class SqlServerAlterColumnAdditionalDescriptionValueTests : SqlServerIntegrationTests
    {
        /// <summary>
        /// Regression test for the SQL Server <c>Alter.Column</c> defect where each additional description's
        /// extended property was written with the column's MAIN description as its value instead of the per-key value.
        /// </summary>
        [Test]
        public void AlterColumnAdditionalDescriptionsArePersistedWithTheirOwnValues()
        {
            var migrationsNamespace = typeof(AlterColumnAdditionalDescriptionValueMigration).Namespace;

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

                AssertExtendedPropertyValue("MS_Description", AlterColumnAdditionalDescriptionValueMigration.MainDescription);

                AssertExtendedPropertyValue(
                    "MS_" + AlterColumnAdditionalDescriptionValueMigration.Key1,
                    AlterColumnAdditionalDescriptionValueMigration.Value1);
                AssertExtendedPropertyValue(
                    "MS_" + AlterColumnAdditionalDescriptionValueMigration.Key2,
                    AlterColumnAdditionalDescriptionValueMigration.Value2);
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

        private void AssertExtendedPropertyValue(string propertyName, string expectedValue)
        {
            var exists = Processor.Exists(
                """
                SELECT 1
                FROM sys.extended_properties ep
                INNER JOIN sys.objects o ON ep.major_id = o.object_id
                INNER JOIN sys.columns c ON c.object_id = o.object_id AND c.column_id = ep.minor_id
                WHERE o.name = '{0}' AND c.name = '{1}' AND ep.name = '{2}' AND CAST(ep.value AS nvarchar(max)) = '{3}'
                """,
                AlterColumnAdditionalDescriptionValueMigration.TableName,
                AlterColumnAdditionalDescriptionValueMigration.ColumnName,
                propertyName,
                expectedValue);

            exists.ShouldBeTrue(
                $"Expected extended property '{propertyName}' on " +
                $"{AlterColumnAdditionalDescriptionValueMigration.TableName}.{AlterColumnAdditionalDescriptionValueMigration.ColumnName} " +
                $"to have value '{expectedValue}'.");
        }

        private void Execute(
            [CanBeNull] Action<IServiceCollection> initAction,
            [NotNull] Action<IServiceProvider> executeAction)
        {
            using (var serviceProvider = CreateProcessorServices(initAction))
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    executeAction(scope.ServiceProvider);
                }
            }
        }
    }
}
