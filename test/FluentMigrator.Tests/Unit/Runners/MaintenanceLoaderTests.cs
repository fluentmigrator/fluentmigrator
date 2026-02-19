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

using System.Linq;

using FluentMigrator.Infrastructure.Extensions;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Infrastructure;
using FluentMigrator.Runner.Initialization;

using Microsoft.Extensions.DependencyInjection;

using Moq;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Runners
{
    [TestFixture]
    [Category("Maintenance")]
    public class MaintenanceLoaderTests
    {
        public const string Tag1 = "MaintenanceTestTag1";
        public const string Tag2 = "MaintenanceTestTag2";
        private readonly string[] _tags = {Tag1, Tag2};

        private Mock<IMigrationRunnerConventions> _migrationConventions;
        private IMaintenanceLoader _maintenanceLoader;
        private IMaintenanceLoader _maintenanceLoaderNoTags;
        private IMaintenanceLoader _maintenanceLoaderTypeFilter;

        [SetUp]
        public void Setup()
        {
            _migrationConventions = new Mock<IMigrationRunnerConventions>();
            _migrationConventions.Setup(x => x.GetMaintenanceStage).Returns(DefaultMigrationRunnerConventions.Instance.GetMaintenanceStage);
            _migrationConventions.Setup(x => x.TypeIsMigration).Returns(DefaultMigrationRunnerConventions.Instance.TypeIsMigration);
            _migrationConventions.Setup(x => x.TypeHasTags).Returns(DefaultMigrationRunnerConventions.Instance.TypeHasTags);
            _migrationConventions.Setup(x => x.TypeHasMatchingTags).Returns(DefaultMigrationRunnerConventions.Instance.TypeHasMatchingTags);

            _maintenanceLoader = ServiceCollectionExtensions.CreateServices()
                .Configure<RunnerOptions>(opt => opt.Tags = _tags)
                .AddSingleton<IMigrationRunnerConventionsAccessor>(new PassThroughMigrationRunnerConventionsAccessor(_migrationConventions.Object))
                .BuildServiceProvider()
                .GetRequiredService<IMaintenanceLoader>();

            _maintenanceLoaderNoTags = ServiceCollectionExtensions.CreateServices()
                .AddSingleton<IMigrationRunnerConventionsAccessor>(new PassThroughMigrationRunnerConventionsAccessor(_migrationConventions.Object))
                .BuildServiceProvider()
                .GetRequiredService<IMaintenanceLoader>();

            _maintenanceLoaderTypeFilter = ServiceCollectionExtensions.CreateServices()
                .Configure<TypeFilterOptions>(opt =>
                {
                    opt.Namespace = "FluentMigrator.Tests.Unit.Runners.MigrationsOtherNamespace";
                    opt.NestedNamespaces = true;
                })
                .AddSingleton<IMigrationRunnerConventionsAccessor>(new PassThroughMigrationRunnerConventionsAccessor(_migrationConventions.Object))
                .BuildServiceProvider()
                .GetRequiredService<IMaintenanceLoader>();
        }

        [Test]
        public void LoadsMigrationsForCorrectStage()
        {
            var migrationInfos = _maintenanceLoader.LoadMaintenance(MigrationStage.BeforeEach);
            _migrationConventions.Verify(x => x.GetMaintenanceStage, Times.AtLeastOnce());
            Assert.That(migrationInfos, Is.Not.Empty);

            Assert.That(migrationInfos.Select(mi => mi.Migration.GetType()), Is.EquivalentTo(new[]
            {
                typeof(MaintenanceBeforeEach),
                typeof(MaintenanceBeforeEachNoTag),
                typeof(MaintenanceBeforeEachWithNonTransactionBehavior),
                typeof(Runners.MigrationsOtherNamespace.MaintenanceBeforeEachNoTag)
            }));

            foreach (var migrationInfo in migrationInfos)
            {
                migrationInfo.Migration.ShouldNotBeNull();

                var maintenanceAttribute = migrationInfo.Migration.GetType().GetOneAttribute<MaintenanceAttribute>();
                maintenanceAttribute.ShouldNotBeNull();
                maintenanceAttribute.Stage.ShouldBe(MigrationStage.BeforeEach);
            }
        }

        [Test]
        [Category("Tagging")]
        public void LoadsMigrationsFilteredByTag()
        {
            var migrationInfos = _maintenanceLoader.LoadMaintenance(MigrationStage.BeforeEach);
            _migrationConventions.Verify(x => x.TypeHasMatchingTags, Times.AtLeastOnce());
            Assert.That(migrationInfos, Is.Not.Empty);

            Assert.That(migrationInfos.Select(mi => mi.Migration.GetType()), Is.EquivalentTo(new[]
            {
                typeof(MaintenanceBeforeEach),
                typeof(MaintenanceBeforeEachNoTag),
                typeof(MaintenanceBeforeEachWithNonTransactionBehavior),
                typeof(Runners.MigrationsOtherNamespace.MaintenanceBeforeEachNoTag)
            }));

            var excludes = new[] { typeof(MaintenanceBeforeEachNoTag), typeof(Runners.MigrationsOtherNamespace.MaintenanceBeforeEachNoTag) };
            foreach (var migrationInfo in migrationInfos)
            {
                migrationInfo.Migration.ShouldNotBeNull();

                if (!excludes.Contains(migrationInfo.Migration.GetType()))
                    DefaultMigrationRunnerConventions.Instance.TypeHasMatchingTags(migrationInfo.Migration.GetType(), _tags)
                        .ShouldBeTrue();
            }
        }

        [Test]
        public void MigrationInfoIsAttributedIsFalse()
        {
            var migrationInfos = _maintenanceLoader.LoadMaintenance(MigrationStage.BeforeEach);
            Assert.That(migrationInfos, Is.Not.Empty);

            foreach (var migrationInfo in migrationInfos)
            {
                migrationInfo.IsAttributed().ShouldBeFalse();
            }
        }

        [Test]
        public void SetsTransactionBehaviorToSameAsMaintenanceAttribute()
        {
            var migrationInfos = _maintenanceLoader.LoadMaintenance(MigrationStage.BeforeEach);
            Assert.That(migrationInfos, Is.Not.Empty);

            foreach (var migrationInfo in migrationInfos)
            {
                migrationInfo.Migration.ShouldNotBeNull();

                var maintenanceAttribute = migrationInfo.Migration.GetType().GetOneAttribute<MaintenanceAttribute>();
                maintenanceAttribute.ShouldNotBeNull();
                migrationInfo.TransactionBehavior.ShouldBe(maintenanceAttribute.TransactionBehavior);
            }
        }

        [Test]
        public void LoadsMigrationsNoTag()
        {
            var migrationInfos = _maintenanceLoaderNoTags.LoadMaintenance(MigrationStage.BeforeEach);
            _migrationConventions.Verify(x => x.TypeHasMatchingTags, Times.AtLeastOnce());
            Assert.That(migrationInfos, Is.Not.Empty);

            bool foundNoTag = false;
            foreach (var migrationInfo in migrationInfos)
            {
                migrationInfo.Migration.ShouldNotBeNull();

                // Both notag maintenance and tagged maintenance should be found in the notag maintenanceLoader because he doesn't care about tags
                if (migrationInfo.Migration.GetType() == typeof(MaintenanceBeforeEachNoTag))
                {
                    foundNoTag = true;
                } else if (migrationInfo.Migration.GetType() == typeof(Runners.MigrationsOtherNamespace.MaintenanceBeforeEachNoTag))
                {
                    continue;
                }
                else
                {
                    DefaultMigrationRunnerConventions.Instance.TypeHasMatchingTags(migrationInfo.Migration.GetType(), _tags)
                        .ShouldBeTrue();
                }
            }

            Assert.That(foundNoTag);
        }

        [Test]
        public void LoadsMigrationsFilterNamespace()
        {
            var migrationInfos = _maintenanceLoaderTypeFilter.LoadMaintenance(MigrationStage.BeforeEach);
            Assert.That(migrationInfos, Is.Not.Empty);

            bool found = false;
            foreach (var migrationInfo in migrationInfos)
            {
                migrationInfo.Migration.ShouldNotBeNull();

                // Both notag maintenance and tagged maintenance should be found in the notag maintenanceLoader because he doesn't care about tags
                if (migrationInfo.Migration.GetType() == typeof(Runners.MigrationsOtherNamespace.MaintenanceBeforeEachNoTag))
                {
                    found = true;
                }
                else
                {
                    DefaultMigrationRunnerConventions.Instance.TypeHasMatchingTags(migrationInfo.Migration.GetType(), _tags)
                        .ShouldBeTrue();
                }
            }

            Assert.That(found);
        }
    }

    [Tags(MaintenanceLoaderTests.Tag1, MaintenanceLoaderTests.Tag2)]
    [Maintenance(MigrationStage.BeforeEach)]
    public class MaintenanceBeforeEach : Migration
    {
        public override void Up() { }
        public override void Down() { }
    }

    [Tags(MaintenanceLoaderTests.Tag1)]
    [Tags(MaintenanceLoaderTests.Tag2)]
    [Maintenance(MigrationStage.BeforeEach, TransactionBehavior.None)]
    public class MaintenanceBeforeEachWithNonTransactionBehavior : Migration
    {
        public override void Up() { }
        public override void Down() { }
    }

    [Tags("NonSpecifiedMaintenanceTestTag1")]
    [Maintenance(MigrationStage.BeforeEach)]
    public class MaintenanceBeforeEachWithoutTestTag : Migration
    {
        public override void Up() { }
        public override void Down() { }
    }

    [Tags(MaintenanceLoaderTests.Tag1)]
    [Maintenance(MigrationStage.AfterAll, TransactionBehavior.None)]
    public class MaintenanceAfterAllWithNoneTransactionBehavior : Migration
    {
        public override void Up() { }
        public override void Down() { }
    }

    [Maintenance(MigrationStage.BeforeEach)]
    public class MaintenanceBeforeEachNoTag : Migration
    {
        public override void Up() { }
        public override void Down() { }
    }

}

namespace FluentMigrator.Tests.Unit.Runners.MigrationsOtherNamespace
{

    [Maintenance(MigrationStage.BeforeEach)]
    public class MaintenanceBeforeEachNoTag : Migration
    {
        public override void Up() { }
        public override void Down() { }
    }
}
