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
using System.Collections.Generic;
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
using System.Linq;

namespace FluentMigrator.Tests.Unit
{
    [TestFixture]
    public class MigrationRunnerTests
    {
        private MigrationRunner _runner;
        private Mock<IAnnouncer> _announcer;
        private Mock<IStopWatch> _stopWatch;

        private Mock<IMigrationProcessor> _processorMock;
        private Mock<IMigrationInformationLoader> _migrationLoaderMock;
        private Mock<IProfileLoader> _profileLoaderMock;
        private Mock<IRunnerContext> _runnerContextMock;
        private SortedList<long, IMigrationInfo> _migrationList;
        private TestVersionLoader _fakeVersionLoader;
        private int _applicationContext;

        [SetUp]
        public void SetUp()
        {
            _applicationContext = new Random().Next();
            _migrationList = new SortedList<long, IMigrationInfo>();
            _runnerContextMock = new Mock<IRunnerContext>(MockBehavior.Loose);
            _processorMock = new Mock<IMigrationProcessor>(MockBehavior.Loose);
            _migrationLoaderMock = new Mock<IMigrationInformationLoader>(MockBehavior.Loose);
            _profileLoaderMock = new Mock<IProfileLoader>(MockBehavior.Loose);

            _announcer = new Mock<IAnnouncer>();
            _stopWatch = new Mock<IStopWatch>();
            _stopWatch.Setup(x => x.Time(It.IsAny<Action>())).Returns(new TimeSpan(1)).Callback((Action a) => a.Invoke());

            var options = new ProcessorOptions
                            {
                                PreviewOnly = false
                            };

            _processorMock.SetupGet(x => x.Options).Returns(options);
            _processorMock.SetupGet(x => x.ConnectionString).Returns(IntegrationTestOptions.SqlServer2008.ConnectionString);

            _runnerContextMock.SetupGet(x => x.Namespace).Returns("FluentMigrator.Tests.Integration.Migrations");
            _runnerContextMock.SetupGet(x => x.Announcer).Returns(_announcer.Object);
            _runnerContextMock.SetupGet(x => x.StopWatch).Returns(_stopWatch.Object);
            _runnerContextMock.SetupGet(x => x.Target).Returns(Assembly.GetExecutingAssembly().ToString());
            _runnerContextMock.SetupGet(x => x.Connection).Returns(IntegrationTestOptions.SqlServer2008.ConnectionString);
            _runnerContextMock.SetupGet(x => x.Database).Returns("sqlserver");
            _runnerContextMock.SetupGet(x => x.ApplicationContext).Returns(_applicationContext);

            _migrationLoaderMock.Setup(x => x.LoadMigrations()).Returns(()=> _migrationList);

            _runner = new MigrationRunner(Assembly.GetAssembly(typeof(MigrationRunnerTests)), _runnerContextMock.Object, _processorMock.Object)
                        {
                            MigrationLoader = _migrationLoaderMock.Object,
                            ProfileLoader = _profileLoaderMock.Object,
                        };

            _fakeVersionLoader = new TestVersionLoader(_runner, _runner.VersionLoader.VersionTableMetaData);

            _runner.VersionLoader = _fakeVersionLoader;

            _processorMock.Setup(x => x.SchemaExists(It.Is<string>(s => s == _runner.VersionLoader.VersionTableMetaData.SchemaName)))
                          .Returns(true);

            _processorMock.Setup(x => x.TableExists(It.Is<string>(s => s == _runner.VersionLoader.VersionTableMetaData.SchemaName),
                                                    It.Is<string>(t => t == _runner.VersionLoader.VersionTableMetaData.TableName)))
                          .Returns(true);
        }

        private void LoadVersionData(params long[] fakeVersions)
        {
            _fakeVersionLoader.Versions.Clear();
            _migrationList.Clear();

            foreach (var version in fakeVersions)
            {
                _fakeVersionLoader.Versions.Add(version);
                _migrationList.Add(version,new MigrationInfo(version, TransactionBehavior.Default, new TestMigration()));
            }

            _fakeVersionLoader.LoadVersionInfo();
        }

        /// <summary>Unit test which ensures that the application context is correctly propagated down to each migration class.</summary>
        [Test(Description = "Ensure that the application context is correctly propagated down to each migration class.")]
        public void CanPassApplicationContext()
        {
            IMigration migration = new TestEmptyMigration();
            _runner.Up(migration);

            Assert.AreEqual(_applicationContext, _runnerContextMock.Object.ApplicationContext, "The runner context does not have the expected application context.");
            Assert.AreEqual(_applicationContext, _runner.ApplicationContext, "The MigrationRunner does not have the expected application context.");
            Assert.AreEqual(_applicationContext, migration.ApplicationContext, "The migration does not have the expected application context.");
            _announcer.VerifyAll();
        }

        [Test]
        public void CanPassConnectionString()
        {
            IMigration migration = new TestEmptyMigration();
            _runner.Up(migration);

            Assert.AreEqual(IntegrationTestOptions.SqlServer2008.ConnectionString, migration.ConnectionString, "The migration does not have the expected connection string.");
            _announcer.VerifyAll();
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

            var exception = Assert.Throws<Exception>(() => _runner.Up(new TestMigration()));
            
            Assert.That(exception.Message, Is.StringContaining("Oops"));
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

            var versionInfoTableName = _runner.VersionLoader.VersionTableMetaData.TableName;

            LoadVersionData(fakeMigrationVersion, fakeMigrationVersion2);

            _runner.VersionLoader.LoadVersionInfo();
            _runner.Rollback(1);

            _fakeVersionLoader.DidRemoveVersionTableGetCalled.ShouldBeFalse();

        }

        [Test]
        public void RollbackLastVersionShouldDeleteVersionInfoTable()
        {
            long fakeMigrationVersion = 2009010101;

            LoadVersionData(fakeMigrationVersion);

            var versionInfoTableName = _runner.VersionLoader.VersionTableMetaData.TableName;

            _runner.Rollback(1);

            _fakeVersionLoader.DidRemoveVersionTableGetCalled.ShouldBeTrue();
        }

        [Test]
        public void RollbackToVersionZeroShouldDeleteVersionInfoTable()
        {
            var versionInfoTableName = _runner.VersionLoader.VersionTableMetaData.TableName;

            _runner.RollbackToVersion(0);

            _fakeVersionLoader.DidRemoveVersionTableGetCalled.ShouldBeTrue();

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
        public void RollbackToVersionShouldShouldLimitMigrationsToNamespace()
        {
            const long fakeMigration1 = 2011010101;
            const long fakeMigration2 = 2011010102;
            const long fakeMigration3 = 2011010103;

            LoadVersionData(fakeMigration1,fakeMigration3);

            _fakeVersionLoader.Versions.Add(fakeMigration2);
            _fakeVersionLoader.LoadVersionInfo();

            _runner.RollbackToVersion(2011010101);
            
            _fakeVersionLoader.Versions.ShouldContain(fakeMigration1);
            _fakeVersionLoader.Versions.ShouldContain(fakeMigration2);
            _fakeVersionLoader.Versions.ShouldNotContain(fakeMigration3);
        }

        [Test]
        public void RollbackToVersionZeroShouldShouldLimitMigrationsToNamespace()
        {
            const long fakeMigration1 = 2011010101;
            const long fakeMigration2 = 2011010102;
            const long fakeMigration3 = 2011010103;

            LoadVersionData(fakeMigration1, fakeMigration2, fakeMigration3);

            _migrationList.Remove(fakeMigration1);
            _migrationList.Remove(fakeMigration2);
            _fakeVersionLoader.LoadVersionInfo();

            _runner.RollbackToVersion(0);

            _fakeVersionLoader.Versions.ShouldContain(fakeMigration1);
            _fakeVersionLoader.Versions.ShouldContain(fakeMigration2);
            _fakeVersionLoader.Versions.ShouldNotContain(fakeMigration3);
        }

        [Test]
        public void RollbackShouldLimitMigrationsToNamespace()
        {
            const long fakeMigration1 = 2011010101;
            const long fakeMigration2 = 2011010102;
            const long fakeMigration3 = 2011010103;

            LoadVersionData(fakeMigration1, fakeMigration3);

            _fakeVersionLoader.Versions.Add(fakeMigration2);
            _fakeVersionLoader.LoadVersionInfo();

            _runner.Rollback(2);

            _fakeVersionLoader.Versions.ShouldNotContain(fakeMigration1);
            _fakeVersionLoader.Versions.ShouldContain(fakeMigration2);
            _fakeVersionLoader.Versions.ShouldNotContain(fakeMigration3);

            _fakeVersionLoader.DidRemoveVersionTableGetCalled.ShouldBeFalse();
        }

        [Test]
        public void RollbackToVersionShouldLoadVersionInfoIfVersionGreaterThanZero()
        {
            var versionInfoTableName = _runner.VersionLoader.VersionTableMetaData.TableName;

            _runner.RollbackToVersion(1);

            _fakeVersionLoader.DidRemoveVersionTableGetCalled.ShouldBeFalse();

            //Once in setup
            _processorMock.Verify(
                pm => pm.Process(It.Is<CreateTableExpression>(
                    dte => dte.TableName == versionInfoTableName)
                    ),
                    Times.Exactly(1)
                );

            //After setup is done, fake version loader owns the proccess
            _fakeVersionLoader.DidLoadVersionInfoGetCalled.ShouldBe(true);
        }

        [Test]
        public void ValidateVersionOrderingShouldReturnNothingIfNoUnappliedMigrations()
        {
            const long version1 = 2011010101;
            const long version2 = 2011010102;

            var mockMigration1 = new Mock<IMigration>();
            var mockMigration2 = new Mock<IMigration>();

            LoadVersionData(version1, version2);

            _migrationList.Clear();
            _migrationList.Add(version1,new MigrationInfo(version1, TransactionBehavior.Default, mockMigration1.Object));
            _migrationList.Add(version2, new MigrationInfo(version2, TransactionBehavior.Default, mockMigration2.Object));

            Assert.DoesNotThrow(() => _runner.ValidateVersionOrder());

            _announcer.Verify(a => a.Say("Version ordering valid."));

            _fakeVersionLoader.DidRemoveVersionTableGetCalled.ShouldBeFalse();		
        }

        [Test]
        public void ValidateVersionOrderingShouldReturnNothingIfUnappliedMigrationVersionIsGreaterThanLatestAppliedMigration()
        {
            const long version1 = 2011010101;
            const long version2 = 2011010102;

            var mockMigration1 = new Mock<IMigration>();
            var mockMigration2 = new Mock<IMigration>();

            LoadVersionData(version1);

            _migrationList.Clear();
            _migrationList.Add(version1, new MigrationInfo(version1, TransactionBehavior.Default, mockMigration1.Object));
            _migrationList.Add(version2, new MigrationInfo(version2, TransactionBehavior.Default, mockMigration2.Object));

            Assert.DoesNotThrow(() => _runner.ValidateVersionOrder());

            _announcer.Verify(a => a.Say("Version ordering valid."));

            _fakeVersionLoader.DidRemoveVersionTableGetCalled.ShouldBeFalse();		
        }

        [Test]
        public void ValidateVersionOrderingShouldThrowExceptionIfUnappliedMigrationVersionIsLessThanGreatestAppliedMigrationVersion()
        {
            const long version1 = 2011010101;
            const long version2 = 2011010102;
            const long version3 = 2011010103;
            const long version4 = 2011010104;

            var mockMigration1 = new Mock<IMigration>();
            var mockMigration2 = new Mock<IMigration>();
            var mockMigration3 = new Mock<IMigration>();
            var mockMigration4 = new Mock<IMigration>();
            
            LoadVersionData(version1, version4);

            _migrationList.Clear();
            _migrationList.Add(version1, new MigrationInfo(version1, TransactionBehavior.Default, mockMigration1.Object));
            _migrationList.Add(version2, new MigrationInfo(version2, TransactionBehavior.Default, mockMigration2.Object));
            _migrationList.Add(version3, new MigrationInfo(version3, TransactionBehavior.Default, mockMigration3.Object));
            _migrationList.Add(version4, new MigrationInfo(version4, TransactionBehavior.Default, mockMigration4.Object));

            var exception = Assert.Throws<VersionOrderInvalidException>(() => _runner.ValidateVersionOrder());

            exception.InvalidMigrations.Count().ShouldBe(2);
            exception.InvalidMigrations.Any(p => p.Key == version2);
            exception.InvalidMigrations.Any(p => p.Key == version3);

            _fakeVersionLoader.DidRemoveVersionTableGetCalled.ShouldBeFalse();
        }

        [Test]
        public void CanListVersions()
        {
            const long version1 = 2011010101;
            const long version2 = 2011010102;

            var mockMigration1 = new Mock<IMigration>();
            var mockMigration2 = new Mock<IMigration>();
            
            LoadVersionData(version1, version2);

            _migrationList.Clear();
            _migrationList.Add(version1, new MigrationInfo(version1, TransactionBehavior.Default, mockMigration1.Object));
            _migrationList.Add(version2, new MigrationInfo(version2, TransactionBehavior.Default, mockMigration2.Object));

            _runner.ListMigrations();

            _announcer.Verify(a => a.Say("2011010101: IMigrationProxy"));
            _announcer.Verify(a => a.Emphasize("2011010102: IMigrationProxy (current)"));
        }

        [Test]
        public void IfMigrationHasAnInvalidExpressionDuringUpActionShouldThrowAnExceptionAndAnnounceTheError()
        {
            var invalidMigration = new Mock<IMigration>();
            var invalidExpression = new UpdateDataExpression {TableName = "Test"};
            invalidMigration.Setup(m => m.GetUpExpressions(It.IsAny<IMigrationContext>())).Callback((IMigrationContext mc) => mc.Expressions.Add(invalidExpression));

            Assert.Throws<InvalidMigrationException>(() => _runner.Up(invalidMigration.Object));

            _announcer.Verify(a => a.Error(It.Is<string>(s => s.Contains("UpdateDataExpression: Update statement is missing a condition. Specify one by calling .Where() or target all rows by calling .AllRows()."))));
        }

        [Test]
        public void IfMigrationHasAnInvalidExpressionDuringDownActionShouldThrowAnExceptionAndAnnounceTheError()
        {
            var invalidMigration = new Mock<IMigration>();
            var invalidExpression = new UpdateDataExpression { TableName = "Test" };
            invalidMigration.Setup(m => m.GetDownExpressions(It.IsAny<IMigrationContext>())).Callback((IMigrationContext mc) => mc.Expressions.Add(invalidExpression));

            Assert.Throws<InvalidMigrationException>(() => _runner.Down(invalidMigration.Object));

            _announcer.Verify(a => a.Error(It.Is<string>(s => s.Contains("UpdateDataExpression: Update statement is missing a condition. Specify one by calling .Where() or target all rows by calling .AllRows()."))));
        }

        [Test]
        public void IfMigrationHasTwoInvalidExpressionsShouldAnnounceBothErrors()
        {
            var invalidMigration = new Mock<IMigration>();
            var invalidExpression = new UpdateDataExpression { TableName = "Test" };
            var secondInvalidExpression = new CreateColumnExpression();
            invalidMigration.Setup(m => m.GetUpExpressions(It.IsAny<IMigrationContext>()))
                .Callback((IMigrationContext mc) => { mc.Expressions.Add(invalidExpression); mc.Expressions.Add(secondInvalidExpression); });

            Assert.Throws<InvalidMigrationException>(() => _runner.Up(invalidMigration.Object));

            _announcer.Verify(a => a.Error(It.Is<string>(s => s.Contains("UpdateDataExpression: Update statement is missing a condition. Specify one by calling .Where() or target all rows by calling .AllRows()."))));
            _announcer.Verify(a => a.Error(It.Is<string>(s => s.Contains("CreateColumnExpression: The table's name cannot be null or an empty string. The column's name cannot be null or an empty string. The column does not have a type defined."))));
        }

    }
}
