#region License
//
// Copyright (c) 2007-2024, Fluent Migrator Project
// Copyright (c) 2010, Nathan Brown
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

using System;
using System.Data;

using FluentMigrator.Runner;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Tests.Integration.Migrations.Computed;
using FluentMigrator.Tests.Integration.TestCases;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Integration
{
    [TestFixture]
    [Category("ComputedColumn")]
    public class ComputedColumnTests : IntegrationTestBase
    {
        private const string RootNamespace = "FluentMigrator.Tests.Integration.Migrations";

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSource))]
        public void CanCreateTableWithColumnComputedStored(
            Type processorType,
            Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteWithProcessor(
                processorType,
                services => services.WithMigrationsIn(RootNamespace),
                (serviceProvider, processor) =>
                {
                    var runner = (MigrationRunner)serviceProvider.GetRequiredService<IMigrationRunner>();

                    runner.Up(new ComputedStoredColumnMigration());

                    InsertAssertValues(runner, processor);

                    runner.Down(new ComputedStoredColumnMigration());
                },
                serverOptions,
                true);
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSource))]
        public void CanCreateTableWithColumnComputedNotStored(
            Type processorType,
            Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteWithProcessor(
                processorType,
                services => services.WithMigrationsIn(RootNamespace),
                (serviceProvider, processor) =>
                {
                    var runner = (MigrationRunner)serviceProvider.GetRequiredService<IMigrationRunner>();

                    runner.Up(new ComputedNotStoredColumnMigration());

                    InsertAssertValues(runner, processor, true);

                    runner.Down(new ComputedNotStoredColumnMigration());
                },
                serverOptions,
                true);
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSource))]
        public void CanCreateColumnComputedNotStored(
            Type processorType,
            Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteWithProcessor(
                processorType,
                services => services.WithMigrationsIn(RootNamespace),
                (serviceProvider, processor) =>
                {
                    var runner = (MigrationRunner)serviceProvider.GetRequiredService<IMigrationRunner>();

                    runner.Up(new ComputedColumnBaseTableMigration());
                    runner.Up(new ComputedNotStoredColumnAlterTableMigration());

                    InsertAssertValues(runner, processor, true);

                    runner.Down(new ComputedNotStoredColumnAlterTableMigration());
                    runner.Down(new ComputedColumnBaseTableMigration());
                },
                serverOptions,
                true);
        }

        private static void InsertAssertValues(MigrationRunner runner, ProcessorBase processor, bool insertNullValues = false)
        {
            runner.Up(new ComputedColumnInsertMigration());

            if (insertNullValues)
            {
                runner.Up(new ComputedColumnInsertNullMigration());
            }

            DataSet ds = processor.ReadTableData(null, "products");

            ds.Tables[0].Rows.Count.ShouldBe(insertNullValues ? 2 : 1);

            ds.Tables[0].Rows[0][3].ShouldBe(200m); // Total = Price * Quantity

            if (insertNullValues)
            {
                ds.Tables[0].Rows[1][3].ShouldBe(DBNull.Value);
            }

            runner.Down(new ComputedColumnInsertMigration());
        }
    }
}
