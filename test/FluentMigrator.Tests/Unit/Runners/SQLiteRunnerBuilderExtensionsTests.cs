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

using FluentMigrator.Runner;
using FluentMigrator.Runner.Generators.SQLite;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Runners
{
    [TestFixture]
    [Category("Runner")]
    [Category("SQLite")]
    // ReSharper disable once InconsistentNaming
    public class SQLiteRunnerBuilderExtensionsTests
    {
        [Test]
        public void AddSQLiteWithoutCompatibilityModeShouldUseDefaultStrict()
        {
            var serviceProvider = new ServiceCollection()
                .AddFluentMigratorCore()
                .ConfigureRunner(rb => rb.AddSQLite())
                .BuildServiceProvider();

            using (var scope = serviceProvider.CreateScope())
            {
                var generator = scope.ServiceProvider.GetRequiredService<SQLiteGenerator>();
                generator.CompatibilityMode.ShouldBe(CompatibilityMode.STRICT);
            }
        }

        [Test]
        public void AddSQLiteWithStrictCompatibilityModeShouldUseStrict()
        {
            var serviceProvider = new ServiceCollection()
                .AddFluentMigratorCore()
                .ConfigureRunner(rb => rb.AddSQLite(compatibilityMode: CompatibilityMode.STRICT))
                .BuildServiceProvider();

            using (var scope = serviceProvider.CreateScope())
            {
                var generator = scope.ServiceProvider.GetRequiredService<SQLiteGenerator>();
                generator.CompatibilityMode.ShouldBe(CompatibilityMode.STRICT);
            }
        }

        [Test]
        public void AddSQLiteWithLooseCompatibilityModeShouldUseLoose()
        {
            var serviceProvider = new ServiceCollection()
                .AddFluentMigratorCore()
                .ConfigureRunner(rb => rb.AddSQLite(compatibilityMode: CompatibilityMode.LOOSE))
                .BuildServiceProvider();

            using (var scope = serviceProvider.CreateScope())
            {
                var generator = scope.ServiceProvider.GetRequiredService<SQLiteGenerator>();
                generator.CompatibilityMode.ShouldBe(CompatibilityMode.LOOSE);
            }
        }

        [Test]
        public void AddSQLiteWithBinaryGuidAndCompatibilityMode()
        {
            var serviceProvider = new ServiceCollection()
                .AddFluentMigratorCore()
                .ConfigureRunner(rb => rb.AddSQLite(binaryGuid: true, compatibilityMode: CompatibilityMode.STRICT))
                .BuildServiceProvider();

            using (var scope = serviceProvider.CreateScope())
            {
                var generator = scope.ServiceProvider.GetRequiredService<SQLiteGenerator>();
                generator.CompatibilityMode.ShouldBe(CompatibilityMode.STRICT);
            }
        }

        [Test]
        public void AddSQLiteWithUseStrictTablesAndCompatibilityMode()
        {
            var serviceProvider = new ServiceCollection()
                .AddFluentMigratorCore()
                .ConfigureRunner(rb => rb.AddSQLite(useStrictTables: true, compatibilityMode: CompatibilityMode.LOOSE))
                .BuildServiceProvider();

            using (var scope = serviceProvider.CreateScope())
            {
                var generator = scope.ServiceProvider.GetRequiredService<SQLiteGenerator>();
                generator.CompatibilityMode.ShouldBe(CompatibilityMode.LOOSE);
            }
        }

        [Test]
        public void AddSQLiteWithAllParameters()
        {
            var serviceProvider = new ServiceCollection()
                .AddFluentMigratorCore()
                .ConfigureRunner(rb => rb.AddSQLite(binaryGuid: true, useStrictTables: true, compatibilityMode: CompatibilityMode.STRICT))
                .BuildServiceProvider();

            using (var scope = serviceProvider.CreateScope())
            {
                var generator = scope.ServiceProvider.GetRequiredService<SQLiteGenerator>();
                generator.CompatibilityMode.ShouldBe(CompatibilityMode.STRICT);
            }
        }
    }
}
