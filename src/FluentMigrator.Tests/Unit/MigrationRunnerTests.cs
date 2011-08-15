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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Tests.Integration.Migrations;
using Moq;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit
{
    [TestFixture]
    public class MigrationRunnerTests
    {
        private MigrationRunner _runner;
        private Mock<IAnnouncer> _announcer;
        private Mock<IStopWatch> _stopWatch;

        private Mock<IMigrationProcessor> _processorMock;
        private Mock<IMigrationLoader> _migrationLoaderMock;
        private Mock<IProfileLoader> _profileLoaderMock;
        private Mock<IRunnerContext> _runnerContextMock;
        private Mock<IVersionLoader> _versionLoaderMock;
        private SortedList<long, IMigration> _migrationList;

        [SetUp]
        public void SetUp()
        {
            _migrationList = new SortedList<long, IMigration>();
            _runnerContextMock = new Mock<IRunnerContext>(MockBehavior.Loose);
            _processorMock = new Mock<IMigrationProcessor>(MockBehavior.Loose);
            _migrationLoaderMock = new Mock<IMigrationLoader>(MockBehavior.Loose);
            _profileLoaderMock = new Mock<IProfileLoader>(MockBehavior.Loose);
            _versionLoaderMock = new Mock<IVersionLoader>(MockBehavior.Loose);

            _announcer = new Mock<IAnnouncer>();
            _stopWatch = new Mock<IStopWatch>();

            var options = new ProcessorOptions
                            {
                                PreviewOnly = true
                            };
            _processorMock.SetupGet(x => x.Options).Returns(options);

            _runnerContextMock.SetupGet(x => x.Namespace).Returns("FluentMigrator.Tests.Integration.Migrations");
            _runnerContextMock.SetupGet(x => x.Announcer).Returns(_announcer.Object);
            _runnerContextMock.SetupGet(x => x.StopWatch).Returns(_stopWatch.Object);
            _runnerContextMock.SetupGet(x => x.Target).Returns(Assembly.GetExecutingAssembly().ToString());
            _runnerContextMock.SetupGet(x => x.Connection).Returns(IntegrationTestOptions.SqlServer2008.ConnectionString);
            _runnerContextMock.SetupGet(x => x.Database).Returns("sqlserver");

            _migrationLoaderMock.SetupGet(x => x.Migrations).Returns(_migrationList);
            _versionLoaderMock.SetupGet(x => x.AlreadyCreatedVersionSchema).Returns(true);
            _versionLoaderMock.SetupGet(x => x.AlreadyCreatedVersionTable).Returns(true);

            _runner = new MigrationRunner(Assembly.GetAssembly(typeof(MigrationRunnerTests)), _runnerContextMock.Object, _processorMock.Object)
                        {
                            MigrationLoader = _migrationLoaderMock.Object,
                            ProfileLoader = _profileLoaderMock.Object,
                            VersionLoader = _versionLoaderMock.Object
                        };
        }

        [Test]
        public void CanAnnounceUp()
        {
            _announcer.Setup(x => x.Heading(It.IsRegex(containsAll("Test", "migrating"))));
            _runner.Up(new TestMigration());
            _announcer.VerifyAll();
        }

        [Test]
        public void CanAnnounceUpFinish()
        {
            _announcer.Setup(x => x.Say(It.IsRegex(containsAll("Test", "migrated"))));
            _runner.Up(new TestMigration());
            _announcer.VerifyAll();
        }

        [Test]
        public void CanAnnounceDown()
        {
            _announcer.Setup(x => x.Heading(It.IsRegex(containsAll("Test", "reverting"))));
            _runner.Down(new TestMigration());
            _announcer.VerifyAll();
        }

        [Test]
        public void CanAnnounceDownFinish()
        {
            _announcer.Setup(x => x.Say(It.IsRegex(containsAll("Test", "reverted"))));
            _runner.Down(new TestMigration());
            _announcer.VerifyAll();
        }

        [Test]
        public void CanAnnounceUpElapsedTime()
        {
            var ts = new TimeSpan(0, 0, 0, 1, 3);
            _announcer.Setup(x => x.ElapsedTime(It.Is<TimeSpan>(y => y == ts)));

            _stopWatch.Setup(x => x.ElapsedTime()).Returns(ts);

            _runner.Up(new TestMigration());

            _announcer.VerifyAll();
        }

        [Test]
        public void CanAnnounceDownElapsedTime()
        {
            var ts = new TimeSpan(0, 0, 0, 1, 3);
            _announcer.Setup(x => x.ElapsedTime(It.Is<TimeSpan>(y => y == ts)));

            _stopWatch.Setup(x => x.ElapsedTime()).Returns(ts);

            _runner.Down(new TestMigration());

            _announcer.VerifyAll();
        }

        [Test]
        public void CanReportExceptions()
        {
            _processorMock.Setup(x => x.Process(It.IsAny<CreateTableExpression>())).Throws(new Exception("Oops"));

            _announcer.Setup(x => x.Error(It.IsRegex(containsAll("Oops"))));

            try
            {
                _runner.Up(new TestMigration());
            }
            catch (Exception)
            {
            }

            _announcer.VerifyAll();
        }

        [Test]
        public void CanSayExpression()
        {
            _announcer.Setup(x => x.Say(It.IsRegex(containsAll("CreateTable"))));

            _stopWatch.Setup(x => x.ElapsedTime()).Returns(new TimeSpan(0, 0, 0, 1, 3));

            _runner.Up(new TestMigration());

            _announcer.VerifyAll();
        }

        [Test]
        public void CanTimeExpression()
        {
            var ts = new TimeSpan(0, 0, 0, 1, 3);
            _announcer.Setup(x => x.ElapsedTime(It.Is<TimeSpan>(y => y == ts)));

            _stopWatch.Setup(x => x.ElapsedTime()).Returns(ts);

            _runner.Up(new TestMigration());

            _announcer.VerifyAll();
        }

        private string containsAll(params string[] words)
        {
            return ".*?" + string.Join(".*?", words) + ".*?";
        }

        [Test]
        public void LoadsCorrectCallingAssembly()
        {
            _runner.MigrationAssembly.ShouldBe(Assembly.GetAssembly(typeof(MigrationRunnerTests)));
        }

        [Test]
        public void RollbackOnlyOneStepsOfTwoShouldNotDeleteVersionInfoTable()
        {
            long fakeMigrationVersion = 2009010101;
            long fakeMigrationVersion2 = 2009010102;

            _runner.MigrationLoader.Migrations.Add(fakeMigrationVersion, new TestMigration());
            _runner.MigrationLoader.Migrations.Add(fakeMigrationVersion2, new TestMigration());

            _runner.VersionLoader.VersionInfo.AddAppliedMigration(fakeMigrationVersion);
            _runner.VersionLoader.VersionInfo.AddAppliedMigration(fakeMigrationVersion2);


            var versionInfoTableName = _runner.VersionLoader.VersionTableMetaData.TableName;

            _runner.Rollback(1);

            _runner.VersionLoader.LoadVersionInfo();

            _runner.VersionLoader.VersionInfo.AppliedMigrations().Count().ShouldBe(1);
        }

        [Test]
        public void RollbackLastVersionShouldDeleteVersionInfoTable()
        {
            long fakeMigrationVersion = 2009010101;
            _runner.MigrationLoader.Migrations.Add(fakeMigrationVersion, new TestMigration());
            _runner.VersionLoader.VersionInfo.AddAppliedMigration(fakeMigrationVersion);

            var versionInfoTableName = _runner.VersionLoader.VersionTableMetaData.TableName;

            _runner.Rollback(1);

            _processorMock.Verify(
                pm => pm.Process(It.Is<DeleteTableExpression>(
                    dte => dte.TableName == versionInfoTableName)
                    )
                );
        }

        [Test]
        public void RollbackToVersionZeroShouldDeleteVersionInfoTable()
        {
            var versionInfoTableName = _runner.VersionLoader.VersionTableMetaData.TableName;

            _runner.RollbackToVersion(0);

            _processorMock.Verify(
                pm => pm.Process(It.Is<DeleteTableExpression>(
                    dte => dte.TableName == versionInfoTableName)
                    )
                );


        }

        [Test]
        public void RollbackToVersionZeroShouldNotCreateVersionInfoTableAfterRemoval()
        {
            var versionInfoTableName = _runner.VersionLoader.VersionTableMetaData.TableName;

            _runner.RollbackToVersion(0);

            //Should only be called once in setup
            _processorMock.Verify(
                pm => pm.Process(It.Is<CreateTableExpression>(
                    dte => dte.TableName == versionInfoTableName)
                    ),
                    Times.Once()
                );
        }

        [Test]
        public void RollbackToVersionShouldLoadVersionInfoIfVersionGreaterThanZero()
        {
            var versionInfoTableName = _runner.VersionLoader.VersionTableMetaData.TableName;

            _runner.RollbackToVersion(1);

            _processorMock.Verify(
                pm => pm.Process(It.Is<DeleteTableExpression>(
                    dte => dte.TableName == versionInfoTableName)
                    ),
                    Times.Never()
                );

            //Once in setup, once after rollback
            _processorMock.Verify(
                pm => pm.Process(It.Is<CreateTableExpression>(
                    dte => dte.TableName == versionInfoTableName)
                    ),
                    Times.Exactly(2)
                );
        }

        [Test, Ignore("Move to MigrationLoader tests")]
        public void HandlesNullMigrationList()
        {
            //set migrations to return empty list
            //			var asm = Assembly.GetAssembly(typeof(MigrationVersionRunnerUnitTests));
            //			_migrationLoaderMock.Setup(x => x.FindMigrations(asm, null)).Returns<IEnumerable<Migration>>(null);
            //
            //			_runner.Migrations.Count.ShouldBe(0);
            //
            //			_vrunner.MigrateUp();
            //
            //			_migrationLoaderMock.VerifyAll();
        }

        [Test, ExpectedException(typeof(Exception))]
        [Ignore("Move to migrationloader tests")]
        public void ShouldThrowExceptionIfDuplicateVersionNumbersAreLoaded()
        {
            //			_migrationLoaderMock.Setup(x => x.FindMigrationsIn(It.IsAny<Assembly>(), null)).Returns(new List<MigrationMetadata>
            //			                                                                         	{
            //			                                                                         		new MigrationMetadata {Version = 1, Type = typeof(UserToRole)},
            //			                                                                         		new MigrationMetadata {Version = 2, Type = typeof(FluentMigrator.Tests.Integration.Migrations.Interleaved.Pass2.UserToRole)},
            //			                                                                         		new MigrationMetadata {Version = 2, Type = typeof(FluentMigrator.Tests.Integration.Migrations.Interleaved.Pass2.UserToRole)}
            //			                                                                         	});
            //
            //			_vrunner.MigrateUp();
        }

        [Test]
        [Ignore("Move to migrationloader tests")]
        public void HandlesMigrationThatDoesNotInheritFromMigrationBaseClass()
        {
            //			_migrationLoaderMock.Setup(x => x.FindMigrationsIn(It.IsAny<Assembly>(), null)).Returns(new List<MigrationMetadata>
            //			                                                                         	{
            //			                                                                         		new MigrationMetadata {Version = 1, Type = typeof(MigrationThatDoesNotInheritFromMigrationBaseClass)},
            //			                                                                         	});
            //
            //			_vrunner.Migrations[1].ShouldNotBeNull();
            //			_vrunner.Migrations[1].ShouldBeOfType<MigrationThatDoesNotInheritFromMigrationBaseClass>();
        }

        private class MigrationThatDoesNotInheritFromMigrationBaseClass : IMigration
        {
            public void GetUpExpressions(IMigrationContext context)
            {
                throw new NotImplementedException();
            }

            public void GetDownExpressions(IMigrationContext context)
            {
                throw new NotImplementedException();
            }
        }
    }
}
