#region License
// 
// Copyright (c) 2007-2009, Sean Chambers <schambers80@gmail.com>
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

using FluentMigrator.Infrastructure;
using FluentMigrator.Infrastructure.Extensions;
using FluentMigrator.Runner;
using Moq;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Runners
{
    [TestFixture]
    public class MaintenanceLoaderTests
    {
        public const string Tag1 = "MaintenanceTestTag1";
        public const string Tag2 = "MaintenanceTestTag2";
        private string[] _tags = {Tag1, Tag2};

        private Mock<IMigrationConventions> _migrationConventions;
        private MaintenanceLoader _maintenanceLoader;
        private MaintenanceLoader _maintenanceLoaderNoTags;

        [SetUp]
        public void Setup()
        {
            _migrationConventions = new Mock<IMigrationConventions>();
            _migrationConventions.Setup(x => x.GetMaintenanceStage).Returns(DefaultMigrationConventions.GetMaintenanceStage);
            _migrationConventions.Setup(x => x.TypeHasTags).Returns(DefaultMigrationConventions.TypeHasTags);
            _migrationConventions.Setup(x => x.TypeHasMatchingTags).Returns(DefaultMigrationConventions.TypeHasMatchingTags);

            _maintenanceLoader = new MaintenanceLoader(new SingleAssembly(GetType().Assembly), _tags, _migrationConventions.Object);
            _maintenanceLoaderNoTags = new MaintenanceLoader(new SingleAssembly(GetType().Assembly), null, _migrationConventions.Object);
        }

        [Test]
        public void LoadsMigrationsForCorrectStage()
        {
            var migrationInfos = _maintenanceLoader.LoadMaintenance(MigrationStage.BeforeEach);
            _migrationConventions.Verify(x => x.GetMaintenanceStage, Times.AtLeastOnce());
            Assert.IsNotEmpty(migrationInfos);

            foreach (var migrationInfo in migrationInfos)
            {
                migrationInfo.Migration.ShouldNotBeNull();

                // The NoTag maintenance should not be found in the tagged maintenanceLoader because it wants tagged classes
                Assert.IsFalse(migrationInfo.Migration.GetType().Equals(typeof(MaintenanceBeforeEachNoTag)));

                var maintenanceAttribute = migrationInfo.Migration.GetType().GetOneAttribute<MaintenanceAttribute>();
                maintenanceAttribute.ShouldNotBeNull();
                maintenanceAttribute.Stage.ShouldBe(MigrationStage.BeforeEach);
            }
        }

        [Test]
        public void LoadsMigrationsFilteredByTag()
        {
            var migrationInfos = _maintenanceLoader.LoadMaintenance(MigrationStage.BeforeEach);
            _migrationConventions.Verify(x => x.TypeHasMatchingTags, Times.AtLeastOnce());
            Assert.IsNotEmpty(migrationInfos);

            foreach (var migrationInfo in migrationInfos)
            {
                migrationInfo.Migration.ShouldNotBeNull();

                // The NoTag maintenance should not be found in the tagged maintenanceLoader because it wants tagged classes
                Assert.IsFalse(migrationInfo.Migration.GetType().Equals(typeof(MaintenanceBeforeEachNoTag)));

                DefaultMigrationConventions.TypeHasMatchingTags(migrationInfo.Migration.GetType(), _tags)
                    .ShouldBeTrue();
            } 
        }

        [Test]
        public void MigrationInfoIsAttributedIsFalse()
        {
            var migrationInfos = _maintenanceLoader.LoadMaintenance(MigrationStage.BeforeEach);
            Assert.IsNotEmpty(migrationInfos);

            foreach (var migrationInfo in migrationInfos)
            {
                migrationInfo.IsAttributed().ShouldBeFalse();
            }
        }

        [Test]
        public void SetsTransactionBehaviorToSameAsMaintenanceAttribute()
        {
            var migrationInfos = _maintenanceLoader.LoadMaintenance(MigrationStage.BeforeEach);
            Assert.IsNotEmpty(migrationInfos);

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
            Assert.IsNotEmpty(migrationInfos);

            bool foundNoTag = false;
            foreach (var migrationInfo in migrationInfos)
            {
                migrationInfo.Migration.ShouldNotBeNull();

                // Both notag maintenance and tagged maintenance should be found in the notag maintenanceLoader because he doesn't care about tags
                if (migrationInfo.Migration.GetType().Equals(typeof(MaintenanceBeforeEachNoTag)))
                {
                    foundNoTag = true;
                }
                else
                {
                    DefaultMigrationConventions.TypeHasMatchingTags(migrationInfo.Migration.GetType(), _tags)
                        .ShouldBeTrue();
                }
            }

            Assert.IsTrue(foundNoTag);
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
