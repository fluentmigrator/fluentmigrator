#region License
// Copyright (c) 2018, FluentMigrator Project
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

using System.Collections.Generic;
using System.Linq;

using FluentMigrator.Infrastructure;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.VersionTableInfo;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.Initialization
{
    [TestFixture]
    public class RunnerBuilderConfigurationTests
    {
        [Test]
        public void TestScanIn()
        {
            var serviceProvider = new ServiceCollection()
                .AddFluentMigratorCore()
                .ConfigureRunner(rb => rb.AddSQLite().ScanIn(typeof(RunnerBuilderConfigurationTests).Assembly))
                .BuildServiceProvider();
            using (var scope = serviceProvider.CreateScope())
            {
                var items = scope.ServiceProvider.GetRequiredService<IEnumerable<IAssemblySourceItem>>().ToList();
                Assert.AreEqual(1, items.Count);
                var migrationSourceItems = scope.ServiceProvider.GetRequiredService<IEnumerable<IMigrationSourceItem>>().ToList();
                Assert.AreEqual(0, migrationSourceItems.Count);
                var versionTableMetaDataSourceItems = scope.ServiceProvider.GetRequiredService<IEnumerable<IVersionTableMetaDataSourceItem>>().ToList();
                Assert.AreEqual(0, versionTableMetaDataSourceItems.Count);
                var embeddedResourceProviders = scope.ServiceProvider.GetRequiredService<IEnumerable<IEmbeddedResourceProvider>>().ToList();
                Assert.AreEqual(1, embeddedResourceProviders.Count);

                var vtmd = scope.ServiceProvider.GetRequiredService<IVersionTableMetaData>();
                Assert.IsInstanceOf<TestVersionTableMetaData>(vtmd);

                var embeddedResources = embeddedResourceProviders
                    .SelectMany(x => x.GetEmbeddedResources())
                    .Distinct().ToList();
                Assert.AreNotEqual(0, embeddedResources.Count);
            }
        }

        [Test]
        public void TestScanInForAll()
        {
            var serviceProvider = new ServiceCollection()
                .AddFluentMigratorCore()
                .ConfigureRunner(rb => rb.AddSQLite().ScanIn(typeof(RunnerBuilderConfigurationTests).Assembly).For.All())
                .BuildServiceProvider();
            using (var scope = serviceProvider.CreateScope())
            {
                var items = scope.ServiceProvider.GetRequiredService<IEnumerable<IAssemblySourceItem>>().ToList();
                Assert.AreEqual(1, items.Count);
                var migrationSourceItems = scope.ServiceProvider.GetRequiredService<IEnumerable<IMigrationSourceItem>>().ToList();
                Assert.AreEqual(0, migrationSourceItems.Count);
                var versionTableMetaDataSourceItems = scope.ServiceProvider.GetRequiredService<IEnumerable<IVersionTableMetaDataSourceItem>>().ToList();
                Assert.AreEqual(0, versionTableMetaDataSourceItems.Count);
                var embeddedResourceProviders = scope.ServiceProvider.GetRequiredService<IEnumerable<IEmbeddedResourceProvider>>().ToList();
                Assert.AreEqual(1, embeddedResourceProviders.Count);

                var vtmd = scope.ServiceProvider.GetRequiredService<IVersionTableMetaData>();
                Assert.IsInstanceOf<TestVersionTableMetaData>(vtmd);

                var embeddedResources = embeddedResourceProviders
                    .SelectMany(x => x.GetEmbeddedResources())
                    .Distinct().ToList();
                Assert.AreNotEqual(0, embeddedResources.Count);
            }
        }

        [Test]
        public void TestScanInForMigration()
        {
            var serviceProvider = new ServiceCollection()
                .AddFluentMigratorCore()
                .ConfigureRunner(rb => rb
                    .AddSQLite()
                    .ScanIn(typeof(RunnerBuilderConfigurationTests).Assembly).For.Migrations())
                .BuildServiceProvider();
            using (var scope = serviceProvider.CreateScope())
            {
                var assemblySourceItems = scope.ServiceProvider.GetRequiredService<IEnumerable<IAssemblySourceItem>>().ToList();
                Assert.AreEqual(0, assemblySourceItems.Count);
                var migrationSourceItems = scope.ServiceProvider.GetRequiredService<IEnumerable<IMigrationSourceItem>>().ToList();
                Assert.AreEqual(1, migrationSourceItems.Count);
                var versionTableMetaDataSourceItems = scope.ServiceProvider.GetRequiredService<IEnumerable<IVersionTableMetaDataSourceItem>>().ToList();
                Assert.AreEqual(0, versionTableMetaDataSourceItems.Count);
                var embeddedResourceProviders = scope.ServiceProvider.GetRequiredService<IEnumerable<IEmbeddedResourceProvider>>().ToList();
                Assert.AreEqual(1, embeddedResourceProviders.Count);

                var vtmd = scope.ServiceProvider.GetRequiredService<IVersionTableMetaData>();
                Assert.IsInstanceOf<DefaultVersionTableMetaData>(vtmd);

                var embeddedResources = embeddedResourceProviders
                    .SelectMany(x => x.GetEmbeddedResources())
                    .Distinct().ToList();
                Assert.AreEqual(0, embeddedResources.Count);
            }
        }

        [Test]
        public void TestScanInForVersionTableMetaData()
        {
            var serviceProvider = new ServiceCollection()
                .AddFluentMigratorCore()
                .ConfigureRunner(rb => rb
                    .AddSQLite()
                    .ScanIn(typeof(RunnerBuilderConfigurationTests).Assembly).For.VersionTableMetaData())
                .BuildServiceProvider();
            using (var scope = serviceProvider.CreateScope())
            {
                var assemblySourceItems = scope.ServiceProvider.GetRequiredService<IEnumerable<IAssemblySourceItem>>().ToList();
                Assert.AreEqual(0, assemblySourceItems.Count);
                var migrationSourceItems = scope.ServiceProvider.GetRequiredService<IEnumerable<IMigrationSourceItem>>().ToList();
                Assert.AreEqual(0, migrationSourceItems.Count);
                var versionTableMetaDataSourceItems = scope.ServiceProvider.GetRequiredService<IEnumerable<IVersionTableMetaDataSourceItem>>().ToList();
                Assert.AreEqual(1, versionTableMetaDataSourceItems.Count);
                var embeddedResourceProviders = scope.ServiceProvider.GetRequiredService<IEnumerable<IEmbeddedResourceProvider>>().ToList();
                Assert.AreEqual(1, embeddedResourceProviders.Count);

                var vtmd = scope.ServiceProvider.GetRequiredService<IVersionTableMetaData>();
                Assert.IsInstanceOf<TestVersionTableMetaData>(vtmd);

                var embeddedResources = embeddedResourceProviders
                    .SelectMany(x => x.GetEmbeddedResources())
                    .Distinct().ToList();
                Assert.AreEqual(0, embeddedResources.Count);
            }
        }

        [Test]
        public void TestScanInForEmbeddedResources()
        {
            var serviceProvider = new ServiceCollection()
                .AddFluentMigratorCore()
                .ConfigureRunner(rb => rb
                    .AddSQLite()
                    .ScanIn(typeof(RunnerBuilderConfigurationTests).Assembly).For.EmbeddedResources())
                .BuildServiceProvider();
            using (var scope = serviceProvider.CreateScope())
            {
                var assemblySourceItems = scope.ServiceProvider.GetRequiredService<IEnumerable<IAssemblySourceItem>>().ToList();
                Assert.AreEqual(0, assemblySourceItems.Count);
                var migrationSourceItems = scope.ServiceProvider.GetRequiredService<IEnumerable<IMigrationSourceItem>>().ToList();
                Assert.AreEqual(0, migrationSourceItems.Count);
                var versionTableMetaDataSourceItems = scope.ServiceProvider.GetRequiredService<IEnumerable<IVersionTableMetaDataSourceItem>>().ToList();
                Assert.AreEqual(0, versionTableMetaDataSourceItems.Count);
                var embeddedResourceProviders = scope.ServiceProvider.GetRequiredService<IEnumerable<IEmbeddedResourceProvider>>().ToList();
                Assert.AreEqual(2, embeddedResourceProviders.Count);

                var vtmd = scope.ServiceProvider.GetRequiredService<IVersionTableMetaData>();
                Assert.IsInstanceOf<DefaultVersionTableMetaData>(vtmd);

                var embeddedResources = embeddedResourceProviders
                    .SelectMany(x => x.GetEmbeddedResources())
                    .Distinct().ToList();
                Assert.AreNotEqual(0, embeddedResources.Count);
            }
        }

        [Test]
        public void TestScanInForMigrationsAndVersionTableMetaData()
        {
            var serviceProvider = new ServiceCollection()
                .AddFluentMigratorCore()
                .ConfigureRunner(rb => rb
                    .AddSQLite()
                    .ScanIn(typeof(RunnerBuilderConfigurationTests).Assembly).For.VersionTableMetaData().For.Migrations())
                .BuildServiceProvider();
            using (var scope = serviceProvider.CreateScope())
            {
                var assemblySourceItems = scope.ServiceProvider.GetRequiredService<IEnumerable<IAssemblySourceItem>>().ToList();
                Assert.AreEqual(0, assemblySourceItems.Count);
                var migrationSourceItems = scope.ServiceProvider.GetRequiredService<IEnumerable<IMigrationSourceItem>>().ToList();
                Assert.AreEqual(1, migrationSourceItems.Count);
                var versionTableMetaDataSourceItems = scope.ServiceProvider.GetRequiredService<IEnumerable<IVersionTableMetaDataSourceItem>>().ToList();
                Assert.AreEqual(1, versionTableMetaDataSourceItems.Count);
                var embeddedResourceProviders = scope.ServiceProvider.GetRequiredService<IEnumerable<IEmbeddedResourceProvider>>().ToList();
                Assert.AreEqual(1, embeddedResourceProviders.Count);

                var vtmd = scope.ServiceProvider.GetRequiredService<IVersionTableMetaData>();
                Assert.IsInstanceOf<TestVersionTableMetaData>(vtmd);

                var embeddedResources = embeddedResourceProviders
                    .SelectMany(x => x.GetEmbeddedResources())
                    .Distinct().ToList();
                Assert.AreEqual(0, embeddedResources.Count);
            }
        }
    }
}
