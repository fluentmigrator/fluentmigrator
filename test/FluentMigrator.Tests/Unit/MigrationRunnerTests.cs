#region License
//
// Copyright (c) 2007-2018, Sean Chambers <schambers80@gmail.com>
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
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Exceptions;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Tests.Integration.Migrations;

using Microsoft.Extensions.DependencyInjection;

using Moq;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit
{
    [TestFixture]
    public class MigrationRunnerTests
    {
        private Mock<IAnnouncer> _announcer;
        private Mock<IStopWatch> _stopWatch;

        private Mock<IMigrationProcessor> _processorMock;
        private Mock<IMigrationInformationLoader> _migrationLoaderMock;
        private Mock<IProfileLoader> _profileLoaderMock;
        private Mock<IAssemblySource> _assemblySourceMock;

        private SortedList<long, IMigrationInfo> _migrationList;
        private TestVersionLoader _fakeVersionLoader;
        private int _applicationContext;

        private IServiceCollection _serviceCollection;

        [SetUp]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void SetUp()
        {
            var asm = Assembly.GetExecutingAssembly();

            _applicationContext = new Random().Next();
            _migrationList = new SortedList<long, IMigrationInfo>();
            _processorMock = new Mock<IMigrationProcessor>(MockBehavior.Loose);
            _migrationLoaderMock = new Mock<IMigrationInformationLoader>(MockBehavior.Loose);
            _profileLoaderMock = new Mock<IProfileLoader>(MockBehavior.Loose);

            _announcer = new Mock<IAnnouncer>();
            _stopWatch = new Mock<IStopWatch>();
            _stopWatch.Setup(x => x.Time(It.IsAny<Action>())).Returns(new TimeSpan(1)).Callback((Action a) => a.Invoke());

            _assemblySourceMock = new Mock<IAssemblySource>();
            _assemblySourceMock.SetupGet(x => x.Assemblies).Returns(new[] { asm });

            _migrationLoaderMock.Setup(x => x.LoadMigrations()).Returns(()=> _migrationList);

            var connectionString = IntegrationTestOptions.SqlServer2008.ConnectionString;
            _serviceCollection = _processorMock.Object.CreateServices()
                .AddSingleton(_announcer.Object)
                .AddSingleton(_stopWatch.Object)
                .AddSingleton(_assemblySourceMock.Object)
                .AddSingleton(_migrationLoaderMock.Object)
                .AddScoped<IConnectionStringReader>(sp => new PassThroughConnectionStringReader(connectionString))
#pragma warning disable 612
                .Configure<RunnerOptions>(opt => opt.ApplicationContext = _applicationContext)
#pragma warning restore 612
                .Configure<ProcessorOptions>(
                    opt => { opt.ConnectionString = connectionString; })
                .Configure<AssemblySourceOptions>(opt => opt.AssemblyNames = new []{ asm.FullName })
                .Configure<TypeFilterOptions>(
                    opt => { opt.Namespace = "FluentMigrator.Tests.Integration.Migrations"; })
                .ConfigureRunner(builder => builder.WithRunnerConventions(new CustomMigrationConventions()));
        }

        private MigrationRunner CreateRunner()
        {
            var serviceProvider = _serviceCollection
                .BuildServiceProvider();

            var runner = (MigrationRunner) serviceProvider.GetRequiredService<IMigrationRunner>();
            runner.ProfileLoader = _profileLoaderMock.Object;

            _fakeVersionLoader = new TestVersionLoader(runner, runner.VersionLoader.VersionTableMetaData);
            runner.VersionLoader = _fakeVersionLoader;

            _processorMock.Setup(x => x.SchemaExists(It.Is<string>(s => s == runner.VersionLoader.VersionTableMetaData.SchemaName)))
                .Returns(true);

            _processorMock.Setup(x => x.TableExists(It.Is<string>(s => s == runner.VersionLoader.VersionTableMetaData.SchemaName),
                    It.Is<string>(t => t == runner.VersionLoader.VersionTableMetaData.TableName)))
                .Returns(true);

            return runner;
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

        [Test]
        public void ProfilesAreAppliedWhenMigrateUpIsCalledWithNoVersion()
        {
            var runner = CreateRunner();
            runner.MigrateUp();
            _profileLoaderMock.Verify(x => x.ApplyProfiles(runner), Times.Once());
        }

        [Test]
        public void ProfilesAreAppliedWhenMigrateUpIsCalledWithVersionParameter()
        {
            var runner = CreateRunner();
            runner.MigrateUp(2009010101);
            _profileLoaderMock.Verify(x => x.ApplyProfiles(runner), Times.Once());
        }

        [Test]
        public void ProfilesAreAppliedWhenMigrateDownIsCalled()
        {
            var runner = CreateRunner();
            runner.MigrateDown(2009010101);
            _profileLoaderMock.Verify(x => x.ApplyProfiles(runner), Times.Once());
        }

        /// <summary>Unit test which ensures that the application context is correctly propagated down to each migration class.</summary>
        [Test(Description = "Ensure that the application context is correctly propagated down to each migration class.")]
        public void CanPassApplicationContext()
        {
            var runner = CreateRunner();

            IMigration migration = new TestEmptyMigration();
            runner.Up(migration);

            Assert.AreEqual(_applicationContext, migration.ApplicationContext, "The migration does not have the expected application context.");
            _announcer.VerifyAll();
        }

        [Test]
        public void CanPassConnectionString()
        {
            var runner = CreateRunner();

            IMigration migration = new TestEmptyMigration();
            runner.Up(migration);

            Assert.AreEqual(IntegrationTestOptions.SqlServer2008.ConnectionString, migration.ConnectionString, "The migration does not have the expected connection string.");
            _announcer.VerifyAll();
        }

        [Test]
        public void CanAnnounceUp()
        {
            _announcer.Setup(x => x.Heading(It.IsRegex(containsAll("Test", "migrating"))));
            var runner = CreateRunner();
            runner.Up(new TestMigration());
            _announcer.VerifyAll();
        }

        [Test]
        public void CanAnnounceUpFinish()
        {
            _announcer.Setup(x => x.Say(It.IsRegex(containsAll("Test", "migrated"))));
            var runner = CreateRunner();
            runner.Up(new TestMigration());
            _announcer.VerifyAll();
        }

        [Test]
        public void CanAnnounceDown()
        {
            _announcer.Setup(x => x.Heading(It.IsRegex(containsAll("Test", "reverting"))));
            var runner = CreateRunner();
            runner.Down(new TestMigration());
            _announcer.VerifyAll();
        }

        [Test]
        public void CanAnnounceDownFinish()
        {
            _announcer.Setup(x => x.Say(It.IsRegex(containsAll("Test", "reverted"))));
            var runner = CreateRunner();
            runner.Down(new TestMigration());
            _announcer.VerifyAll();
        }

        [Test]
        public void CanAnnounceUpElapsedTime()
        {
            var ts = new TimeSpan(0, 0, 0, 1, 3);
            _announcer.Setup(x => x.ElapsedTime(It.Is<TimeSpan>(y => y == ts)));

            _stopWatch.Setup(x => x.ElapsedTime()).Returns(ts);

            var runner = CreateRunner();
            runner.Up(new TestMigration());

            _announcer.VerifyAll();
        }

        [Test]
        public void CanAnnounceDownElapsedTime()
        {
            var ts = new TimeSpan(0, 0, 0, 1, 3);
            _announcer.Setup(x => x.ElapsedTime(It.Is<TimeSpan>(y => y == ts)));

            _stopWatch.Setup(x => x.ElapsedTime()).Returns(ts);

            var runner = CreateRunner();
            runner.Down(new TestMigration());

            _announcer.VerifyAll();
        }

        [Test]
        public void CanReportExceptions()
        {
            _processorMock.Setup(x => x.Process(It.IsAny<CreateTableExpression>())).Throws(new Exception("Oops"));

            var runner = CreateRunner();
            var exception = Assert.Throws<Exception>(() => runner.Up(new TestMigration()));

            Assert.That(exception.Message, Does.Contain("Oops"));
        }

        [Test]
        public void CanSayExpression()
        {
            _announcer.Setup(x => x.Say(It.IsRegex(containsAll("CreateTable"))));

            _stopWatch.Setup(x => x.ElapsedTime()).Returns(new TimeSpan(0, 0, 0, 1, 3));

            var runner = CreateRunner();
            runner.Up(new TestMigration());

            _announcer.VerifyAll();
        }

        [Test]
        public void CanTimeExpression()
        {
            var ts = new TimeSpan(0, 0, 0, 1, 3);
            _announcer.Setup(x => x.ElapsedTime(It.Is<TimeSpan>(y => y == ts)));

            _stopWatch.Setup(x => x.ElapsedTime()).Returns(ts);

            var runner = CreateRunner();
            runner.Up(new TestMigration());

            _announcer.VerifyAll();
        }

        private string containsAll(params string[] words)
        {
            return ".*?" + string.Join(".*?", words) + ".*?";
        }

        [Test]
        public void HasMigrationsToApplyUpWhenThereAreMigrations()
        {
            long fakeMigrationVersion1 = 2009010101;
            long fakeMigrationVersion2 = 2009010102;
            LoadVersionData(fakeMigrationVersion1, fakeMigrationVersion2);
            _fakeVersionLoader.Versions.Remove(fakeMigrationVersion2);
            _fakeVersionLoader.LoadVersionInfo();

            var runner = CreateRunner();
            runner.HasMigrationsToApplyUp().ShouldBeTrue();
        }

        [Test]
        public void HasMigrationsToApplyUpWhenThereAreNoNewMigrations()
        {
            long fakeMigrationVersion1 = 2009010101;
            long fakeMigrationVersion2 = 2009010102;
            LoadVersionData(fakeMigrationVersion1, fakeMigrationVersion2);

            var runner = CreateRunner();
            runner.HasMigrationsToApplyUp().ShouldBeFalse();
        }

        [Test]
        public void HasMigrationsToApplyUpToSpecificVersionWhenTheSpecificHasNotBeenApplied()
        {
            long fakeMigrationVersion1 = 2009010101;
            long fakeMigrationVersion2 = 2009010102;
            LoadVersionData(fakeMigrationVersion1, fakeMigrationVersion2);
            _fakeVersionLoader.Versions.Remove(fakeMigrationVersion2);
            _fakeVersionLoader.LoadVersionInfo();

            var runner = CreateRunner();
            runner.HasMigrationsToApplyUp(fakeMigrationVersion2).ShouldBeTrue();
        }

        [Test]
        public void HasMigrationsToApplyUpToSpecificVersionWhenTheSpecificHasBeenApplied()
        {
            long fakeMigrationVersion1 = 2009010101;
            long fakeMigrationVersion2 = 2009010102;
            LoadVersionData(fakeMigrationVersion1, fakeMigrationVersion2);
            _fakeVersionLoader.Versions.Remove(fakeMigrationVersion2);
            _fakeVersionLoader.LoadVersionInfo();

            var runner = CreateRunner();
            runner.HasMigrationsToApplyUp(fakeMigrationVersion1).ShouldBeFalse();
        }

        [Test]
        public void HasMigrationsToApplyRollbackWithOneMigrationApplied()
        {
            long fakeMigrationVersion1 = 2009010101;
            LoadVersionData(fakeMigrationVersion1);

            var runner = CreateRunner();
            runner.HasMigrationsToApplyRollback().ShouldBeTrue();
        }

        [Test]
        public void HasMigrationsToApplyRollbackWithNoMigrationsApplied()
        {
            LoadVersionData();

            var runner = CreateRunner();
            runner.HasMigrationsToApplyRollback().ShouldBeFalse();
        }


        [Test]
        public void HasMigrationsToApplyDownWhenTheVersionHasNotBeenApplied()
        {
            long fakeMigrationVersion1 = 2009010101;
            long fakeMigrationVersion2 = 2009010102;
            LoadVersionData(fakeMigrationVersion1, fakeMigrationVersion2);
            _fakeVersionLoader.Versions.Remove(fakeMigrationVersion2);
            _fakeVersionLoader.LoadVersionInfo();

            var runner = CreateRunner();
            runner.HasMigrationsToApplyDown(fakeMigrationVersion1).ShouldBeFalse();
        }

        [Test]
        public void HasMigrationsToApplyDownWhenTheVersionHasBeenApplied()
        {
            long fakeMigrationVersion1 = 2009010101;
            long fakeMigrationVersion2 = 2009010102;
            LoadVersionData(fakeMigrationVersion1, fakeMigrationVersion2);

            var runner = CreateRunner();
            runner.HasMigrationsToApplyDown(fakeMigrationVersion1).ShouldBeTrue();
        }

        [Test]
        public void RollbackOnlyOneStepsOfTwoShouldNotDeleteVersionInfoTable()
        {
            long fakeMigrationVersion = 2009010101;
            long fakeMigrationVersion2 = 2009010102;

            var runner = CreateRunner();
            Assert.NotNull(runner.VersionLoader.VersionTableMetaData.TableName);

            LoadVersionData(fakeMigrationVersion, fakeMigrationVersion2);

            runner.VersionLoader.LoadVersionInfo();
            runner.Rollback(1);

            _fakeVersionLoader.DidRemoveVersionTableGetCalled.ShouldBeFalse();

        }

        [Test]
        public void RollbackLastVersionShouldDeleteVersionInfoTable()
        {
            long fakeMigrationVersion = 2009010101;

            LoadVersionData(fakeMigrationVersion);

            var runner = CreateRunner();
            Assert.NotNull(runner.VersionLoader.VersionTableMetaData.TableName);

            runner.Rollback(1);

            _fakeVersionLoader.DidRemoveVersionTableGetCalled.ShouldBeTrue();
        }

        [Test]
        public void RollbackToVersionZeroShouldDeleteVersionInfoTable()
        {
            var runner = CreateRunner();

            Assert.NotNull(runner.VersionLoader.VersionTableMetaData.TableName);

            runner.RollbackToVersion(0);

            _fakeVersionLoader.DidRemoveVersionTableGetCalled.ShouldBeTrue();

        }

        [Test]
        public void RollbackToVersionZeroShouldNotCreateVersionInfoTableAfterRemoval()
        {
            var runner = CreateRunner();

            var versionInfoTableName = runner.VersionLoader.VersionTableMetaData.TableName;

            runner.RollbackToVersion(0);

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

            var runner = CreateRunner();
            runner.RollbackToVersion(2011010101);

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

            var runner = CreateRunner();
            runner.RollbackToVersion(0);

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

            var runner = CreateRunner();
            runner.Rollback(2);

            _fakeVersionLoader.Versions.ShouldNotContain(fakeMigration1);
            _fakeVersionLoader.Versions.ShouldContain(fakeMigration2);
            _fakeVersionLoader.Versions.ShouldNotContain(fakeMigration3);

            _fakeVersionLoader.DidRemoveVersionTableGetCalled.ShouldBeFalse();
        }

        [Test]
        public void RollbackToVersionShouldLoadVersionInfoIfVersionGreaterThanZero()
        {
            var runner = CreateRunner();

            var versionInfoTableName = runner.VersionLoader.VersionTableMetaData.TableName;

            runner.RollbackToVersion(1);

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

            var runner = CreateRunner();
            Assert.DoesNotThrow(() => runner.ValidateVersionOrder());

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

            var runner = CreateRunner();
            Assert.DoesNotThrow(() => runner.ValidateVersionOrder());

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

            var runner = CreateRunner();
            var exception = Assert.Throws<VersionOrderInvalidException>(() => runner.ValidateVersionOrder());

            var invalidMigrations = exception.InvalidMigrations.ToList();
            invalidMigrations.Count().ShouldBe(2);
            invalidMigrations.Select(x => x.Key).ShouldContain(version2);
            invalidMigrations.Select(x => x.Key).ShouldContain(version3);

            _fakeVersionLoader.DidRemoveVersionTableGetCalled.ShouldBeFalse();
        }

        [Test]
        public void CanListVersions()
        {
            const long version1 = 2011010101;
            const long version2 = 2011010102;
            const long version3 = 2011010103;
            const long version4 = 2011010104;

            var mockMigration1 = new Mock<IMigration>();
            var mockMigration2 = new Mock<IMigration>();
            var mockMigration3 = new Mock<IMigration>();
            var mockMigration4 = new Mock<IMigration>();

            LoadVersionData(version1, version3);

            _migrationList.Clear();
            _migrationList.Add(version1, new MigrationInfo(version1, TransactionBehavior.Default, mockMigration1.Object));
            _migrationList.Add(version2, new MigrationInfo(version2, TransactionBehavior.Default, mockMigration2.Object));
            _migrationList.Add(version3, new MigrationInfo(version3, TransactionBehavior.Default, mockMigration3.Object));
            _migrationList.Add(version4, new MigrationInfo(version4, TransactionBehavior.Default, true, mockMigration4.Object));

            var runner = CreateRunner();
            runner.ListMigrations();

            _announcer.Verify(a => a.Say("2011010101: IMigrationProxy"));
            _announcer.Verify(a => a.Say("2011010102: IMigrationProxy (not applied)"));
            _announcer.Verify(a => a.Emphasize("2011010103: IMigrationProxy (current)"));
            _announcer.Verify(a => a.Emphasize("2011010104: IMigrationProxy (not applied, BREAKING)"));
        }

        [Test]
        public void IfMigrationHasAnInvalidExpressionDuringUpActionShouldThrowAnExceptionAndAnnounceTheError()
        {
            var invalidMigration = new Mock<IMigration>();
            var invalidExpression = new UpdateDataExpression {TableName = "Test"};
            invalidMigration.Setup(m => m.GetUpExpressions(It.IsAny<IMigrationContext>())).Callback((IMigrationContext mc) => mc.Expressions.Add(invalidExpression));

            var runner = CreateRunner();
            Assert.Throws<InvalidMigrationException>(() => runner.Up(invalidMigration.Object));

            var expectedErrorMessage = ErrorMessages.UpdateDataExpressionMustSpecifyWhereClauseOrAllRows;
            _announcer.Verify(a => a.Error(It.Is<string>(s => s.Contains($"UpdateDataExpression: {expectedErrorMessage}"))));
        }

        [Test]
        public void IfMigrationHasAnInvalidExpressionDuringDownActionShouldThrowAnExceptionAndAnnounceTheError()
        {
            var invalidMigration = new Mock<IMigration>();
            var invalidExpression = new UpdateDataExpression { TableName = "Test" };
            invalidMigration.Setup(m => m.GetDownExpressions(It.IsAny<IMigrationContext>())).Callback((IMigrationContext mc) => mc.Expressions.Add(invalidExpression));

            var runner = CreateRunner();
            Assert.Throws<InvalidMigrationException>(() => runner.Down(invalidMigration.Object));

            var expectedErrorMessage = ErrorMessages.UpdateDataExpressionMustSpecifyWhereClauseOrAllRows;
            _announcer.Verify(a => a.Error(It.Is<string>(s => s.Contains($"UpdateDataExpression: {expectedErrorMessage}"))));
        }

        [Test]
        public void IfMigrationHasTwoInvalidExpressionsShouldAnnounceBothErrors()
        {
            var invalidMigration = new Mock<IMigration>();
            var invalidExpression = new UpdateDataExpression { TableName = "Test" };
            var secondInvalidExpression = new CreateColumnExpression();
            invalidMigration.Setup(m => m.GetUpExpressions(It.IsAny<IMigrationContext>()))
                .Callback((IMigrationContext mc) => { mc.Expressions.Add(invalidExpression); mc.Expressions.Add(secondInvalidExpression); });

            var runner = CreateRunner();
            Assert.Throws<InvalidMigrationException>(() => runner.Up(invalidMigration.Object));

            _announcer.Verify(a => a.Error(It.Is<string>(s => s.Contains($"UpdateDataExpression: {ErrorMessages.UpdateDataExpressionMustSpecifyWhereClauseOrAllRows}"))));
            _announcer.Verify(a => a.Error(It.Is<string>(s => s.Contains($"CreateColumnExpression: {ErrorMessages.TableNameCannotBeNullOrEmpty} {ErrorMessages.ColumnNameCannotBeNullOrEmpty} {ErrorMessages.ColumnTypeMustBeDefined}"))));
        }

        [Test]
        public void CanLoadCustomMigrationConventions()
        {
            var runner = CreateRunner();
            Assert.That(runner.Conventions, Is.TypeOf<CustomMigrationConventions>());
        }

        [Test]
        public void CanLoadDefaultMigrationConventionsIfNoCustomConventionsAreSpecified()
        {
            var processorMock = new Mock<IMigrationProcessor>(MockBehavior.Loose);
            var runner = (MigrationRunner) processorMock.Object.CreateServices()
                .BuildServiceProvider()
                .GetRequiredService<IMigrationRunner>();
            Assert.That(runner.Conventions, Is.TypeOf<MigrationRunnerConventions>());
        }

        [Test]
        public void CanBlockBreakingChangesByDefault()
        {
            var runner = CreateRunner();

            InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() =>
                runner.ApplyMigrationUp(
                    new MigrationInfo(7, TransactionBehavior.Default, true, new TestBreakingMigration()), true));

            Assert.NotNull(ex);

            Assert.AreEqual(
                "The migration 7: TestBreakingMigration is identified as a breaking change, and will not be executed unless the necessary flag (allow-breaking-changes|abc) is passed to the runner.",
                ex.Message);
        }

        [Test]
        public void CanRunBreakingChangesIfSpecified()
        {
            _serviceCollection
                .Configure<RunnerOptions>(opt => opt.AllowBreakingChange = true);

            var runner = CreateRunner();

            Assert.DoesNotThrow(() =>
                runner.ApplyMigrationUp(
                    new MigrationInfo(7, TransactionBehavior.Default, true, new TestBreakingMigration()), true));
        }

        [Test]
        public void CanRunBreakingChangesInPreview()
        {
            _serviceCollection
                .Configure<RunnerOptions>(opt => opt.AllowBreakingChange = true)
                .Configure<ProcessorOptions>(opt => opt.PreviewOnly = true);

            var runner = CreateRunner();

            Assert.DoesNotThrow(() =>
                runner.ApplyMigrationUp(
                    new MigrationInfo(7, TransactionBehavior.Default, true, new TestBreakingMigration()), true));
        }

        public class CustomMigrationConventions : MigrationRunnerConventions
        {
        }
    }
}
