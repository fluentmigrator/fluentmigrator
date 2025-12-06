#region License
//
// Copyright (c) 2018, Fluent Migrator Project
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
using System.Linq.Expressions;

using FluentMigrator.Exceptions;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Tests.Integration;

using JetBrains.Annotations;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Moq;

using NUnit.Framework;

namespace FluentMigrator.Tests.Unit
{
    [TestFixture]
    [Category("Runner")]
    [Category("TaskExecutor")]
    public class TaskExecutorTests : IntegrationTestBase
    {
        private Mock<IMigrationRunner> _migrationRunner;

        [SetUp]
        public void SetUp()
        {
            _migrationRunner = new Mock<IMigrationRunner>();
        }

        private void Verify(Expression<Action<IMigrationRunner>> func, string task, long version, int steps)
        {
            _migrationRunner.Setup(func).Verifiable();

            var processor = new Mock<IMigrationProcessor>();
            const string profile = "Debug";
            var dataSet = new DataSet();
            dataSet.Tables.Add(new DataTable());
            processor.Setup(x => x.ReadTableData(null, It.IsAny<string>())).Returns(dataSet);

            var stopWatch = new Mock<IStopWatch>();

            var services = ServiceCollectionExtensions.CreateServices()
                .WithProcessor(processor)
                .AddSingleton(stopWatch.Object)
                .AddScoped<IConnectionStringReader>(_ => new PassThroughConnectionStringReader(IntegrationTestOptions.SqlServer2008.ConnectionString))
                .AddScoped(_ => _migrationRunner.Object)
                .Configure<SelectingProcessorAccessorOptions>(opt => opt.ProcessorId = "sqlserver2008")
                .Configure<RunnerOptions>(
                    opt =>
                    {
                        opt.Task = task;
                        opt.Version = version;
                        opt.Steps = steps;
                        opt.Profile = profile;
                    })
                .WithMigrationsIn("FluentMigrator.Tests.Integration.Migrations.Interleaved.Pass3")
                .AddScoped<TaskExecutor, FakeTaskExecutor>();

            var serviceProvider = services
                .BuildServiceProvider();

            var taskExecutor = serviceProvider.GetRequiredService<TaskExecutor>();

            taskExecutor.Execute();

            _migrationRunner.VerifyAll();
        }

        [Test]
        public void InvalidProviderNameShouldThrowArgumentException()
        {
            var services = ServiceCollectionExtensions.CreateServices()
                .ConfigureRunner(r => r.AddSQLite())
                .AddScoped<IConnectionStringReader>(_ => new PassThroughConnectionStringReader(IntegrationTestOptions.SqlServer2008.ConnectionString))
                .Configure<SelectingProcessorAccessorOptions>(opt => opt.ProcessorId = "sqlWRONG")
                .WithMigrationsIn("FluentMigrator.Tests.Integration.Migrations")
                .AddScoped<TaskExecutor, FakeTaskExecutor>();

            var serviceProvider = services
                .BuildServiceProvider();

            var taskExecutor = serviceProvider.GetRequiredService<TaskExecutor>();
            Assert.Throws<ProcessorFactoryNotFoundException>(() => taskExecutor.Execute());
        }

        [Test]
        public void ShouldCallMigrateDownIfSpecified()
        {
            Verify(x => x.MigrateDown(It.Is<long>(c => c == 20)), "migrate:down", 20, 0);
        }

        [Test]
        public void ShouldCallMigrateUpByDefault()
        {
            Verify(x => x.MigrateUp(), null, 0, 0);
            Verify(x => x.MigrateUp(), "", 0, 0);
        }

        [Test]
        public void ShouldCallMigrateUpIfSpecified()
        {
            Verify(x => x.MigrateUp(), "migrate", 0, 0);
            Verify(x => x.MigrateUp(), "migrate:up", 0, 0);
        }

        [Test]
        public void ShouldCallMigrateUpWithVersionIfSpecified()
        {
            Verify(x => x.MigrateUp(It.Is<long>(c => c == 1)), "migrate", 1, 0);
            Verify(x => x.MigrateUp(It.Is<long>(c => c == 1)), "migrate:up", 1, 0);
        }

        [Test]
        public void ShouldCallRollbackIfSpecified()
        {
            Verify(x => x.Rollback(It.Is<int>(c => c == 2)), "rollback", 0, 2);
        }

        [Test]
        public void ShouldCallRollbackIfSpecifiedAndDefaultTo1Step()
        {
            Verify(x => x.Rollback(It.Is<int>(c => c == 1)), "rollback", 0, 0);
        }

        [Test]
        public void ShouldCallValidateVersionOrder()
        {
            Verify(x => x.ValidateVersionOrder(), "validateversionorder", 0, 0);
        }

        [Test]
        public void ShouldCallHasMigrationsToApplyUpWithNullVersionOnNoTask()
        {
            VerifyHasMigrationsToApply(x => x.HasMigrationsToApplyUp(It.Is<long?>(version => !version.HasValue)), "", 0);
        }

        [Test]
        public void ShouldCallHasMigrationsToApplyUpWithVersionOnNoTask()
        {
            VerifyHasMigrationsToApply(x => x.HasMigrationsToApplyUp(It.Is<long?>(version => version.GetValueOrDefault() == 1)), "", 1);
        }

        [Test]
        public void ShouldCallHasMigrationsToApplyUpWithNullVersionOnMigrate()
        {
            VerifyHasMigrationsToApply(x => x.HasMigrationsToApplyUp(It.Is<long?>(version => !version.HasValue)), "migrate", 0);
        }

        [Test]
        public void ShouldCallHasMigrationsToApplyUpWithVersionOnMigrate()
        {
            VerifyHasMigrationsToApply(x => x.HasMigrationsToApplyUp(It.Is<long?>(version => version.GetValueOrDefault() == 1)), "migrate", 1);
        }

        [Test]
        public void ShouldCallHasMigrationsToApplyUpWithNullVersionOnMigrateUp()
        {
            VerifyHasMigrationsToApply(x => x.HasMigrationsToApplyUp(It.Is<long?>(version => !version.HasValue)), "migrate:up", 0);
        }

        [Test]
        public void ShouldCallHasMigrationsToApplyUpWithVersionOnMigrateUp()
        {
            VerifyHasMigrationsToApply(x => x.HasMigrationsToApplyUp(It.Is<long?>(version => version.GetValueOrDefault() == 1)), "migrate:up", 1);
        }

        [Test]
        public void ShouldCallHasMigrationsToApplyRollbackOnRollback()
        {
            VerifyHasMigrationsToApply(x => x.HasMigrationsToApplyRollback(), "rollback", 0);
        }

        [Test]
        public void ShouldCallHasMigrationsToApplyRollbackOnRollbackAll()
        {
            VerifyHasMigrationsToApply(x => x.HasMigrationsToApplyRollback(), "rollback:all", 0);
        }

        [Test]
        public void ShouldCallHasMigrationsToApplyDownOnRollbackToVersion()
        {
            VerifyHasMigrationsToApply(x => x.HasMigrationsToApplyDown(It.Is<long>(version => version == 2)), "rollback:toversion", 2);
        }

        [Test]
        public void ShouldCallHasMigrationsToApplyDownOnMigrateDown()
        {
            VerifyHasMigrationsToApply(x => x.HasMigrationsToApplyDown(It.Is<long>(version => version == 2)), "migrate:down", 2);
        }

        private void VerifyHasMigrationsToApply(Expression<Func<IMigrationRunner, bool>> func, string task, long version)
        {
            _migrationRunner.Setup(func).Verifiable();

            var processor = new Mock<IMigrationProcessor>();
            var dataSet = new DataSet();
            dataSet.Tables.Add(new DataTable());
            processor.Setup(x => x.ReadTableData(null, It.IsAny<string>())).Returns(dataSet);
            _migrationRunner.SetupGet(x => x.Processor).Returns(processor.Object);

            var services = ServiceCollectionExtensions.CreateServices()
                .WithProcessor(processor)
                .AddScoped<IConnectionStringReader>(_ => new PassThroughConnectionStringReader(IntegrationTestOptions.SqlServer2008.ConnectionString))
                .AddScoped(_ => _migrationRunner.Object)
                .Configure<SelectingProcessorAccessorOptions>(opt => opt.ProcessorId = "sqlserver2008")
                .Configure<RunnerOptions>(
                    opt =>
                    {
                        opt.Task = task;
                        opt.Version = version;
                    })
                .WithMigrationsIn("FluentMigrator.Tests.Integration.Migrations")
                .AddScoped<TaskExecutor, FakeTaskExecutor>();

            var serviceProvider = services
                .BuildServiceProvider();

            var taskExecutor = serviceProvider.GetRequiredService<TaskExecutor>();
            taskExecutor.HasMigrationsToApply();
            _migrationRunner.Verify(func, Times.Once());
        }

        internal class FakeTaskExecutor : TaskExecutor
        {
            public FakeTaskExecutor(
                [NotNull] ILogger<TaskExecutor> logger,
                [NotNull] IOptions<RunnerOptions> runnerOptions,
                [NotNull] IServiceProvider serviceProvider)
                : base(logger, runnerOptions, serviceProvider)
            {
            }
        }
    }
}
