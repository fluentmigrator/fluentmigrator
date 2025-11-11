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

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

using FluentMigrator.Exceptions;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Exceptions;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Infrastructure;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Tests.Integration.Migrations;
using FluentMigrator.Tests.Integration.Migrations.Constrained.Constraints;
using FluentMigrator.Tests.Integration.Migrations.Constrained.ConstraintsMultiple;
using FluentMigrator.Tests.Integration.Migrations.Constrained.ConstraintsSuccess;
using FluentMigrator.Tests.Logging;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Moq;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit
{
    [TestFixture]
    [Category("Runner")]
    [Category("MigrationRunner")]
    public class MigrationRunnerTests
    {
        private Mock<IStopWatch> _stopWatch;

        private Mock<IMigrationProcessor> _processorMock;
        private Mock<IMigrationInformationLoader> _migrationLoaderMock;
        private Mock<IProfileLoader> _profileLoaderMock;
        private Mock<IAssemblySource> _assemblySourceMock;
        private Mock<IMigrationScopeManager> _migrationScopeHandlerMock;

        private ICollection<string> _logMessages;
        private SortedList<long, IMigrationInfo> _migrationList;
        private TestVersionLoader _fakeVersionLoader;

        private IServiceCollection _serviceCollection;

        [SetUp]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void SetUp()
        {
            var asm = Assembly.GetExecutingAssembly();

            _migrationList = new SortedList<long, IMigrationInfo>();
            _processorMock = new Mock<IMigrationProcessor>(MockBehavior.Loose);
            _migrationLoaderMock = new Mock<IMigrationInformationLoader>(MockBehavior.Loose);
            _profileLoaderMock = new Mock<IProfileLoader>(MockBehavior.Loose);
            _migrationScopeHandlerMock = new Mock<IMigrationScopeManager>(MockBehavior.Loose);
            _migrationScopeHandlerMock.Setup(x => x.CreateOrWrapMigrationScope(It.IsAny<bool>())).Returns(new NoOpMigrationScope());

            _stopWatch = new Mock<IStopWatch>();
            _stopWatch.Setup(x => x.Time(It.IsAny<Action>())).Returns(new TimeSpan(1)).Callback((Action a) => a.Invoke());

            _assemblySourceMock = new Mock<IAssemblySource>();
            _assemblySourceMock.SetupGet(x => x.Assemblies).Returns(new[] { asm });

            _migrationLoaderMock.Setup(x => x.LoadMigrations()).Returns(()=> _migrationList);

            var generatorAccessorMock = new Mock<IGeneratorAccessor>(MockBehavior.Loose);

            _logMessages = new List<string>();
            var connectionString = IntegrationTestOptions.SqlServer2008.ConnectionString;
            _serviceCollection = ServiceCollectionExtensions.CreateServices()
                .WithProcessor(_processorMock)
                .AddSingleton<ILoggerProvider>(new TextLineLoggerProvider(_logMessages, new FluentMigratorLoggerOptions() { ShowElapsedTime = true }))
                .AddSingleton(_stopWatch.Object)
                .AddSingleton(_assemblySourceMock.Object)
                .AddSingleton(_migrationLoaderMock.Object)
                .AddSingleton(generatorAccessorMock.Object)
                .AddScoped<IConnectionStringReader>(_ => new PassThroughConnectionStringReader(connectionString))
                .AddScoped(_ => _profileLoaderMock.Object)
                .Configure<ProcessorOptions>(
                    opt => opt.ConnectionString = connectionString)
                .Configure<AssemblySourceOptions>(opt => opt.AssemblyNames = new []{ asm.FullName })
                .Configure<TypeFilterOptions>(
                    opt => opt.Namespace = "FluentMigrator.Tests.Integration.Migrations")
                .ConfigureRunner(builder => builder.WithRunnerConventions(new CustomMigrationConventions()));
        }

        private MigrationRunner CreateRunner(Action<IServiceCollection> initAction = null)
        {
            initAction?.Invoke(_serviceCollection);
            var serviceProvider = _serviceCollection
                .BuildServiceProvider();

            var runner = (MigrationRunner) serviceProvider.GetRequiredService<IMigrationRunner>();
            _fakeVersionLoader = new TestVersionLoader(runner, runner.VersionLoader.VersionTableMetaData);
            runner.VersionLoader = _fakeVersionLoader;

            var readTableDataResult = new DataSet();
            readTableDataResult.Tables.Add(new DataTable());

            _processorMock.Setup(x => x.ReadTableData(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(readTableDataResult);
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

        [Test]
        public void CanPassConnectionString()
        {
            var runner = CreateRunner();

            IMigration migration = new TestEmptyMigration();
            runner.Up(migration);

            Assert.That(migration.ConnectionString, Is.EqualTo(IntegrationTestOptions.SqlServer2008.ConnectionString), "The migration does not have the expected connection string.");
        }

        [Test]
        public void CanAnnounceUp()
        {
            var runner = CreateRunner();
            runner.Up(new TestMigration());
            _logMessages.ShouldContain(l => LineContainsAll(l, "Test", "migrating"));
        }

        [Test]
        public void CanAnnounceUpFinish()
        {
            var runner = CreateRunner();
            runner.Up(new TestMigration());
            _logMessages.ShouldContain(l => LineContainsAll(l, "Test", "migrated"));
        }

        [Test]
        public void CanAnnounceDown()
        {
            var runner = CreateRunner();
            runner.Down(new TestMigration());
            _logMessages.ShouldContain(l => LineContainsAll(l, "Test", "reverting"));
        }

        [Test]
        public void CanAnnounceDownFinish()
        {
            var runner = CreateRunner();
            runner.Down(new TestMigration());
            _logMessages.ShouldContain(l => LineContainsAll(l, "Test", "reverted"));
        }

        [Test]
        public void CanAnnounceUpElapsedTime()
        {
            var ts = new TimeSpan(0, 0, 0, 1, 3);

            _stopWatch.Setup(x => x.ElapsedTime()).Returns(ts);

            var runner = CreateRunner();
            runner.Up(new TestMigration());

            _logMessages.ShouldContain(l => l.Equals($"=> {ts.TotalSeconds}s"));
        }

        [Test]
        public void CanAnnounceDownElapsedTime()
        {
            var ts = new TimeSpan(0, 0, 0, 1, 3);

            _stopWatch.Setup(x => x.ElapsedTime()).Returns(ts);

            var runner = CreateRunner();
            runner.Down(new TestMigration());

            _logMessages.ShouldContain(l => l.Equals($"=> {ts.TotalSeconds}s"));
        }

        [Test]
        public void CanReportExceptions()
        {
            var runner = CreateRunner();

            _processorMock.Setup(x => x.Process(It.IsAny<CreateTableExpression>())).Throws(new Exception("Oops"));

            var exception = Assert.Throws<Exception>(() => runner.Up(new TestMigration()));

            Assert.That(exception.Message, Does.Contain("Oops"));
        }

        [Test]
        public void CanSayExpression()
        {
            _stopWatch.Setup(x => x.ElapsedTime()).Returns(new TimeSpan(0, 0, 0, 1, 3));

            var runner = CreateRunner();
            runner.Up(new TestMigration());

            _logMessages.ShouldContain(l => LineContainsAll(l, "CreateTable"));
        }

        [Test]
        public void CanTimeExpression()
        {
            var ts = new TimeSpan(0, 0, 0, 1, 3);

            _stopWatch.Setup(x => x.ElapsedTime()).Returns(ts);

            var runner = CreateRunner();
            runner.Up(new TestMigration());

            _logMessages.ShouldContain(l => l.Equals($"=> {ts.TotalSeconds}s"));
        }

        [Test]
        public void HasMigrationsToApplyUpWhenThereAreMigrations()
        {
            var runner = CreateRunner();

            const long fakeMigrationVersion1 = 2009010101;
            const long fakeMigrationVersion2 = 2009010102;
            LoadVersionData(fakeMigrationVersion1, fakeMigrationVersion2);
            _fakeVersionLoader.Versions.Remove(fakeMigrationVersion2);
            _fakeVersionLoader.LoadVersionInfo();

            runner.HasMigrationsToApplyUp().ShouldBeTrue();
        }

        [Test]
        public void HasMigrationsToApplyUpWhenThereAreNoNewMigrations()
        {
            var runner = CreateRunner();

            const long fakeMigrationVersion1 = 2009010101;
            const long fakeMigrationVersion2 = 2009010102;
            LoadVersionData(fakeMigrationVersion1, fakeMigrationVersion2);

            runner.HasMigrationsToApplyUp().ShouldBeFalse();
        }

        [Test]
        public void HasMigrationsToApplyUpToSpecificVersionWhenTheSpecificHasNotBeenApplied()
        {
            var runner = CreateRunner();

            const long fakeMigrationVersion1 = 2009010101;
            const long fakeMigrationVersion2 = 2009010102;
            LoadVersionData(fakeMigrationVersion1, fakeMigrationVersion2);
            _fakeVersionLoader.Versions.Remove(fakeMigrationVersion2);
            _fakeVersionLoader.LoadVersionInfo();

            runner.HasMigrationsToApplyUp(fakeMigrationVersion2).ShouldBeTrue();
        }

        [Test]
        public void HasMigrationsToApplyUpToSpecificVersionWhenTheSpecificHasBeenApplied()
        {
            var runner = CreateRunner();

            const long fakeMigrationVersion1 = 2009010101;
            const long fakeMigrationVersion2 = 2009010102;
            LoadVersionData(fakeMigrationVersion1, fakeMigrationVersion2);
            _fakeVersionLoader.Versions.Remove(fakeMigrationVersion2);
            _fakeVersionLoader.LoadVersionInfo();

            runner.HasMigrationsToApplyUp(fakeMigrationVersion1).ShouldBeFalse();
        }

        [Test]
        public void HasMigrationsToApplyRollbackWithOneMigrationApplied()
        {
            var runner = CreateRunner();

            const long fakeMigrationVersion1 = 2009010101;
            LoadVersionData(fakeMigrationVersion1);

            runner.HasMigrationsToApplyRollback().ShouldBeTrue();
        }

        [Test]
        public void HasMigrationsToApplyRollbackWithNoMigrationsApplied()
        {
            var runner = CreateRunner();
            LoadVersionData();
            runner.HasMigrationsToApplyRollback().ShouldBeFalse();
        }

        [Test]
        public void HasMigrationsToApplyDownWhenTheVersionHasNotBeenApplied()
        {
            var runner = CreateRunner();

            const long fakeMigrationVersion1 = 2009010101;
            const long fakeMigrationVersion2 = 2009010102;
            LoadVersionData(fakeMigrationVersion1, fakeMigrationVersion2);
            _fakeVersionLoader.Versions.Remove(fakeMigrationVersion2);
            _fakeVersionLoader.LoadVersionInfo();

            runner.HasMigrationsToApplyDown(fakeMigrationVersion1).ShouldBeFalse();
        }

        [Test]
        public void HasMigrationsToApplyDownWhenTheVersionHasBeenApplied()
        {
            var runner = CreateRunner();

            const long fakeMigrationVersion1 = 2009010101;
            const long fakeMigrationVersion2 = 2009010102;
            LoadVersionData(fakeMigrationVersion1, fakeMigrationVersion2);

            runner.HasMigrationsToApplyDown(fakeMigrationVersion1).ShouldBeTrue();
        }

        [Test]
        public void RollbackOnlyOneStepsOfTwoShouldNotDeleteVersionInfoTable()
        {
            const long fakeMigrationVersion = 2009010101;
            const long fakeMigrationVersion2 = 2009010102;

            var runner = CreateRunner();
            Assert.That(runner.VersionLoader.VersionTableMetaData.TableName, Is.Not.Null);

            LoadVersionData(fakeMigrationVersion, fakeMigrationVersion2);

            runner.VersionLoader.LoadVersionInfo();
            runner.Rollback(1);

            _fakeVersionLoader.DidRemoveVersionTableGetCalled.ShouldBeFalse();
        }

        [Test]
        public void RollbackLastVersionShouldDeleteVersionInfoTable()
        {
            var runner = CreateRunner();

            const long fakeMigrationVersion = 2009010101;

            LoadVersionData(fakeMigrationVersion);

            Assert.That(runner.VersionLoader.VersionTableMetaData.TableName, Is.Not.Null);

            runner.Rollback(1);

            _fakeVersionLoader.DidRemoveVersionTableGetCalled.ShouldBeTrue();
        }

        [Test]
        public void RollbackToVersionZeroShouldDeleteVersionInfoTable()
        {
            var runner = CreateRunner();

            Assert.That(runner.VersionLoader.VersionTableMetaData.TableName, Is.Not.Null);

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

            var runner = CreateRunner();
            LoadVersionData(fakeMigration1,fakeMigration3);

            _fakeVersionLoader.Versions.Add(fakeMigration2);
            _fakeVersionLoader.LoadVersionInfo();

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

            var runner = CreateRunner();
            LoadVersionData(fakeMigration1, fakeMigration2, fakeMigration3);

            _migrationList.Remove(fakeMigration1);
            _migrationList.Remove(fakeMigration2);
            _fakeVersionLoader.LoadVersionInfo();

            runner.RollbackToVersion(0);

            _fakeVersionLoader.Versions.ShouldContain(fakeMigration1);
            _fakeVersionLoader.Versions.ShouldContain(fakeMigration2);
            _fakeVersionLoader.Versions.ShouldNotContain(fakeMigration3);
        }

        [Test]
        public void RollbackShouldLimitMigrationsToNamespace()
        {
            var runner = CreateRunner();

            const long fakeMigration1 = 2011010101;
            const long fakeMigration2 = 2011010102;
            const long fakeMigration3 = 2011010103;

            LoadVersionData(fakeMigration1, fakeMigration3);

            _fakeVersionLoader.Versions.Add(fakeMigration2);
            _fakeVersionLoader.LoadVersionInfo();

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

            var runner = CreateRunner();
            LoadVersionData(version1, version2);

            _migrationList.Clear();
            _migrationList.Add(version1,new MigrationInfo(version1, TransactionBehavior.Default, mockMigration1.Object));
            _migrationList.Add(version2, new MigrationInfo(version2, TransactionBehavior.Default, mockMigration2.Object));

            Assert.DoesNotThrow(() => runner.ValidateVersionOrder());

            _logMessages.ShouldContain(l => LineContainsAll(l, "Version ordering valid."));

            _fakeVersionLoader.DidRemoveVersionTableGetCalled.ShouldBeFalse();
        }

        [Test]
        public void ValidateVersionOrderingShouldReturnNothingIfUnappliedMigrationVersionIsGreaterThanLatestAppliedMigration()
        {
            const long version1 = 2011010101;
            const long version2 = 2011010102;

            var mockMigration1 = new Mock<IMigration>();
            var mockMigration2 = new Mock<IMigration>();

            var runner = CreateRunner();
            LoadVersionData(version1);

            _migrationList.Clear();
            _migrationList.Add(version1, new MigrationInfo(version1, TransactionBehavior.Default, mockMigration1.Object));
            _migrationList.Add(version2, new MigrationInfo(version2, TransactionBehavior.Default, mockMigration2.Object));

            Assert.DoesNotThrow(() => runner.ValidateVersionOrder());

            _logMessages.ShouldContain(l => LineContainsAll(l, "Version ordering valid."));

            _fakeVersionLoader.DidRemoveVersionTableGetCalled.ShouldBeFalse();
        }

        [Test]
        public void ValidateVersionOrderingShouldThrowExceptionIfUnappliedMigrationVersionIsLessThanGreatestAppliedMigrationVersion()
        {
            var runner = CreateRunner();

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

            var exception = Assert.Throws<VersionOrderInvalidException>(() => runner.ValidateVersionOrder());

            var invalidMigrations = exception.InvalidMigrations.ToList();
            invalidMigrations.Count.ShouldBe(2);
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

            var runner = CreateRunner();

            LoadVersionData(version1, version3);

            _migrationList.Clear();
            _migrationList.Add(version1, new MigrationInfo(version1, TransactionBehavior.Default, mockMigration1.Object));
            _migrationList.Add(version2, new MigrationInfo(version2, TransactionBehavior.Default, mockMigration2.Object));
            _migrationList.Add(version3, new MigrationInfo(version3, TransactionBehavior.Default, mockMigration3.Object));
            _migrationList.Add(version4, new MigrationInfo(version4, TransactionBehavior.Default, true, mockMigration4.Object));

            runner.ListMigrations();

            _logMessages.ShouldContain(l => LineContainsAll(l, "2011010101: IMigrationProxy"));
            _logMessages.ShouldContain(l => LineContainsAll(l, "2011010102: IMigrationProxy (not applied)"));
            _logMessages.ShouldContain(l => LineContainsAll(l, "2011010103: IMigrationProxy (current)"));
            _logMessages.ShouldContain(l => LineContainsAll(l, "2011010104: IMigrationProxy (not applied, BREAKING)"));
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
            _logMessages.ShouldContain(l => LineContainsAll(l, $"UpdateDataExpression: {expectedErrorMessage}"));
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
            _logMessages.ShouldContain(l => LineContainsAll(l, $"UpdateDataExpression: {expectedErrorMessage}"));
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

            _logMessages.ShouldContain(l => LineContainsAll(l, $"UpdateDataExpression: {ErrorMessages.UpdateDataExpressionMustSpecifyWhereClauseOrAllRows}"));
            _logMessages.ShouldContain(l => LineContainsAll(l, $"CreateColumnExpression: {ErrorMessages.TableNameCannotBeNullOrEmpty}"));
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
            var serviceProvider = ServiceCollectionExtensions.CreateServices(false)
                .WithProcessor(processorMock)
                .BuildServiceProvider();
            var runner = (MigrationRunner) serviceProvider.GetRequiredService<IMigrationRunner>();
            Assert.That(runner.Conventions, Is.TypeOf<DefaultMigrationRunnerConventions>());
        }

        [Test]
        public void CanBlockBreakingChangesByDefault()
        {
            var runner = CreateRunner(sc => sc.Configure<RunnerOptions>(opt => opt.AllowBreakingChange = false));

            InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() =>
                runner.ApplyMigrationUp(
                    new MigrationInfo(7, TransactionBehavior.Default, true, new TestBreakingMigration()), true));

            Assert.That(ex, Is.Not.Null);

            Assert.That(
                ex.Message, Is.EqualTo("The migration 7: TestBreakingMigration is identified as a breaking change, and will not be executed unless the necessary flag (allow-breaking-changes|abc) is passed to the runner."));
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

        [Test]
        public void TestLoadVersionInfoIfRequired()
        {
            var runner = CreateRunner();

            runner.LoadVersionInfoIfRequired().ShouldBeTrue();
            runner.LoadVersionInfoIfRequired().ShouldBeFalse();
        }

        [Test]
        public void CanLoadCustomMigrationScopeHandler()
        {
            _serviceCollection.AddScoped<IMigrationScopeManager>(scoped => { return _migrationScopeHandlerMock.Object; });
            var runner = CreateRunner();
            runner.BeginScope();
            _migrationScopeHandlerMock.Verify(x => x.BeginScope(), Times.Once());
        }

        [Test]
        public void CanLoadDefaultMigrationScopeHandlerIfNoCustomHandlerIsSpecified()
        {
            var runner = CreateRunner();
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.NonPublic;
            FieldInfo field = typeof(MigrationRunner).GetField("_migrationScopeManager", bindFlags);
            Assert.That(field.GetValue(runner), Is.TypeOf<MigrationScopeHandler>());
        }

        [Test]
        public void DoesRunMigrationsThatMeetConstraints()
        {
            _migrationList.Clear();
            _migrationList.Add(1, new MigrationInfo(1, TransactionBehavior.Default, new Step1Migration()));
            _migrationList.Add(2, new MigrationInfo(2, TransactionBehavior.Default, new Step2Migration()));
            _migrationList.Add(3, new MigrationInfo(3, TransactionBehavior.Default, new Step2Migration2()));
            var runner = CreateRunner();
            runner.MigrateUp();
            Assert.That(runner.VersionLoader.VersionInfo.Latest(), Is.EqualTo(1));
        }

        [Test]
        public void DoesRunMigrationsThatDoMeetConstraints()
        {
            _migrationList.Clear();
            _migrationList.Add(1, new MigrationInfo(1, TransactionBehavior.Default, new Step1Migration()));
            _migrationList.Add(2, new MigrationInfo(2, TransactionBehavior.Default, new Step2Migration()));
            _migrationList.Add(3, new MigrationInfo(3, TransactionBehavior.Default, new Step2Migration2()));
            var runner = CreateRunner();
            runner.MigrateUp();
            runner.MigrateUp(); // run migrations second time, this time satisfying constraints
            Assert.That(runner.VersionLoader.VersionInfo.Latest(), Is.EqualTo(3));
        }

        [Test]
        public void MultipleConstraintsEachNeedsToReturnTrue()
        {
            _migrationList.Clear();
            _migrationList.Add(1, new MigrationInfo(1, TransactionBehavior.Default, new MultipleConstraintsMigration()));
            var runner = CreateRunner();
            runner.MigrateUp();
            Assert.That(runner.VersionLoader.VersionInfo.Latest(), Is.EqualTo(0));
        }

        [Test]
        public void DoesRunMigrationWithPositiveConstraint()
        {
            _migrationList.Clear();
            _migrationList.Add(1, new MigrationInfo(1, TransactionBehavior.Default, new ConstrainedMigrationSuccess()));
            var runner = CreateRunner();
            runner.MigrateUp();
            Assert.That(runner.VersionLoader.VersionInfo.Latest(), Is.EqualTo(1));
        }

        [Test]
        public void MigrateUpShouldValidateConnectionAndThrowExceptionForInvalidConnectionString()
        {
            // Arrange
            _migrationList.Clear();
            _migrationList.Add(1, new MigrationInfo(1, TransactionBehavior.Default, new TestMigration()));
            
            var runner = CreateRunner();

            // Setup processor mock to throw exception on BeginTransaction AFTER runner initialization
            // Use SetupSequence to throw on the validation call (which is the first call after initialization)
            _processorMock.Setup(x => x.BeginTransaction())
                .Throws(new ArgumentException("Invalid connection string"));

            // Act & Assert
            var exception = Assert.Throws<UndeterminableConnectionException>(() => runner.MigrateUp());
            Assert.That(exception.Message, Does.Contain("Failed to establish database connection"));
            Assert.That(exception.InnerException, Is.Not.Null);
            Assert.That(exception.InnerException.Message, Does.Contain("Invalid connection string"));
        }

        [Test]
        public void MigrateUpShouldSkipConnectionValidationInPreviewMode()
        {
            // Arrange
            _migrationList.Clear();
            _migrationList.Add(1, new MigrationInfo(1, TransactionBehavior.Default, new TestMigration()));
            
            var runner = CreateRunner(services =>
            {
                services.Configure<ProcessorOptions>(opt => opt.PreviewOnly = true);
            });

            // Setup processor mock to throw exception on BeginTransaction AFTER runner initialization
            _processorMock.Setup(x => x.BeginTransaction())
                .Throws(new ArgumentException("Invalid connection string"));

            // Act & Assert - Should not throw exception in preview mode
            Assert.DoesNotThrow(() => runner.MigrateUp());
        }

        [Test]
        public void MigrateUpShouldValidateConnectionEvenWhenNoMigrationsToApply()
        {
            // Arrange
            _migrationList.Clear(); // No migrations to apply
            
            var runner = CreateRunner();

            // Setup processor mock to throw exception on BeginTransaction AFTER runner initialization
            _processorMock.Setup(x => x.BeginTransaction())
                .Throws(new ArgumentException("Invalid connection string"));

            // Act & Assert - Should still validate connection even with no migrations
            var exception = Assert.Throws<UndeterminableConnectionException>(() => runner.MigrateUp());
            Assert.That(exception.Message, Does.Contain("Failed to establish database connection"));
            Assert.That(exception.InnerException, Is.Not.Null);
            Assert.That(exception.InnerException.Message, Does.Contain("Invalid connection string"));
        }

        private static bool LineContainsAll(string line, params string[] words)
        {
            var pattern = string.Join(".*?", words.Select(Regex.Escape));
            return Regex.IsMatch(line, pattern);
        }

        public class CustomMigrationConventions : MigrationRunnerConventions
        {
        }
    }
}
