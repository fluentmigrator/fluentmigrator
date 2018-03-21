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
using FluentMigrator.Infrastructure;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Tests.Integration.Migrations;
using Moq;
using NUnit.Framework;

namespace FluentMigrator.Tests.Unit
{
    [TestFixture]
    public class MigrationRunnerWithGateTests {

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
            _runnerContextMock.SetupGet(x => x.Targets).Returns(new string[] { Assembly.GetExecutingAssembly().ToString()});
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
                _migrationList.Add(version, new MigrationInfo(version, TransactionBehavior.Default, new TestMigration()));
            }

            _fakeVersionLoader.LoadVersionInfo();
        }

        [Test]
        public void CanListVersionsWithGate()
        {
            const long version1 = 2011010101;
            const long version2 = 2011010102;

            var mockMigration1 = new Mock<IMigration>();
            var mockMigration2 = new Mock<IMigration>();
            
            LoadVersionData(version1, version2);

            _migrationList.Clear();

            var migrationInfo1 = new MigrationInfo(version1, TransactionBehavior.Default, mockMigration1.Object);
            migrationInfo1.Gate.SetGate(DateTime.Now, DateTime.Now);

            _migrationList.Add(version1, migrationInfo1);
            _migrationList.Add(version2, new MigrationInfo(version2, TransactionBehavior.Default, mockMigration2.Object));

            _runner.ListMigrations();

            _announcer.Verify(a => a.Say(string.Format("2011010101: IMigrationProxy [{0}]", migrationInfo1.Gate)));
            _announcer.Verify(a => a.Emphasize("2011010102: IMigrationProxy (current)"));
        }

        [Test]
        public void CanMigratingUpOnlyTheMigrationGateOpened() {
            const long version1 = 2011010115;

            var mockMigration1 = new Mock<IMigration>();

            ClearAllVersionData();

            _migrationList.Clear();

            var migrationInfo1 = new MigrationInfo(version1, TransactionBehavior.Default, mockMigration1.Object);
            migrationInfo1.Gate.SetGate(DateTime.Now.AddDays(-1), DateTime.Now.AddDays(1));

            _migrationList.Add(version1, migrationInfo1);

            _announcer.Setup(x => x.Heading(It.IsRegex(containsAll(string.Format("2011010115: IMigrationProxy[{0}]", migrationInfo1.Gate), 
                                                                                "migrating"))));
            _runner.MigrateUp();
            _announcer.VerifyAll();
        }

        [Test]
        public void CanNotMigratingUpIfTheMigrationGateNotOpened() {
            const long version1 = 2011010116;

            var mockMigration1 = new Mock<IMigration>();

            ClearAllVersionData();

            _migrationList.Clear();

            var migrationInfo1 = new MigrationInfo(version1, TransactionBehavior.Default, mockMigration1.Object);
            migrationInfo1.Gate.SetGate(DateTime.Now.AddDays(-2), DateTime.Now.AddDays(-1));

            _migrationList.Add(version1, migrationInfo1);
            
            _runner.MigrateUp();

            _announcer.Verify(x => x.Heading(It.IsRegex(containsAll(string.Format("2011010116: IMigrationProxy[{0}]", migrationInfo1.Gate),
                                                                                  "migrating"))), Times.Never());
        }

        private void ClearAllVersionData() {
            _fakeVersionLoader.Versions.Clear();
            _fakeVersionLoader.LoadVersionInfo();
        }

        private string containsAll(params string[] words) {
            return ".*?" + string.Join(".*?", words) + ".*?";
        }
    }
}
