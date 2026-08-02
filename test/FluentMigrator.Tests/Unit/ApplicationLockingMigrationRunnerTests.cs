#region License
// Copyright (c) 2007-2024, Fluent Migrator Project
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

using System;

using FluentMigrator.Generation;
using FluentMigrator.Infrastructure;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Generators.Generic;
using FluentMigrator.Runner.Infrastructure;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors;

using Microsoft.Extensions.DependencyInjection;

using Moq;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit
{
    [TestFixture]
    [Category("Runner")]
    [Category("ApplicationLocker")]
    public class ApplicationLockingMigrationRunnerTests
    {
        [Test]
        public void RunnerOperationIsExecutedWhileLockIsHeld()
        {
            var locker = new TrackingApplicationLocker();
            var innerRunner = new Mock<IMigrationRunner>();
            innerRunner
                .Setup(x => x.MigrateUp())
                .Callback(() => locker.IsLocked.ShouldBeTrue());
            var runner = new ApplicationLockingMigrationRunner(innerRunner.Object, locker);

            runner.MigrateUp();

            locker.AcquisitionCount.ShouldBe(1);
            locker.DisposalCount.ShouldBe(1);
            locker.IsLocked.ShouldBeFalse();
        }

        [Test]
        public void NestedRunnerOperationDoesNotReacquireLock()
        {
            var locker = new TrackingApplicationLocker();
            var innerRunner = new Mock<IMigrationRunner>();
            var migration = Mock.Of<IMigration>();
            ApplicationLockingMigrationRunner runner = null;
            innerRunner
                .Setup(x => x.MigrateUp())
                .Callback(() => runner.Up(migration));
            innerRunner
                .Setup(x => x.Up(migration))
                .Callback(() => locker.IsLocked.ShouldBeTrue());
            runner = new ApplicationLockingMigrationRunner(innerRunner.Object, locker);

            runner.MigrateUp();

            locker.AcquisitionCount.ShouldBe(1);
            locker.DisposalCount.ShouldBe(1);
        }

        [Test]
        public void LockIsReleasedWhenRunnerOperationThrows()
        {
            var locker = new TrackingApplicationLocker();
            var innerRunner = new Mock<IMigrationRunner>();
            innerRunner
                .Setup(x => x.MigrateUp())
                .Throws<InvalidOperationException>();
            var runner = new ApplicationLockingMigrationRunner(innerRunner.Object, locker);

            Should.Throw<InvalidOperationException>(() => runner.MigrateUp());

            locker.DisposalCount.ShouldBe(1);
            locker.IsLocked.ShouldBeFalse();
        }

        [Test]
        public void BeginScopeHoldsLockUntilScopeIsDisposed()
        {
            var locker = new TrackingApplicationLocker();
            var migrationScope = new Mock<IMigrationScope>();
            var innerRunner = new Mock<IMigrationRunner>();
            innerRunner.Setup(x => x.BeginScope()).Returns(migrationScope.Object);
            var runner = new ApplicationLockingMigrationRunner(innerRunner.Object, locker);

            using (var scope = runner.BeginScope())
            {
                locker.IsLocked.ShouldBeTrue();
                scope.Complete();
            }

            migrationScope.Verify(x => x.Complete(), Times.Once());
            migrationScope.Verify(x => x.Dispose(), Times.Once());
            locker.DisposalCount.ShouldBe(1);
            locker.IsLocked.ShouldBeFalse();
        }

        [Test]
        public void ConfiguredLockerEncompassesVersionLoaderInitialization()
        {
            var processor = new Mock<IMigrationProcessor>(MockBehavior.Loose);
            var generator = new Mock<IMigrationGenerator>(MockBehavior.Loose);
            generator.SetupGet(x => x.Quoter).Returns(new GenericQuoter());
            var generatorAccessor = new Mock<IGeneratorAccessor>(MockBehavior.Loose);
            generatorAccessor.SetupGet(x => x.Generator).Returns(generator.Object);
            var services = ServiceCollectionExtensions.CreateServices(false)
                .WithProcessor(processor)
                .AddSingleton(generatorAccessor.Object)
                .Configure<ProcessorOptions>(x => x.ConnectionString = "test")
                .ConfigureApplicationLocker<TrackingApplicationLocker>();
            using var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var locker = (TrackingApplicationLocker)scope.ServiceProvider.GetRequiredService<IApplicationLocker>();
            processor
                .Setup(x => x.TableExists(It.IsAny<string>(), It.IsAny<string>()))
                .Callback(() => locker.IsLocked.ShouldBeTrue())
                .Returns(false);

            var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
            runner.ShouldBeOfType<ApplicationLockingMigrationRunner>();

            runner.LoadVersionInfoIfRequired();

            locker.AcquisitionCount.ShouldBe(1);
            locker.DisposalCount.ShouldBe(1);
        }

        public sealed class TrackingApplicationLocker : IApplicationLocker
        {
            public int AcquisitionCount { get; private set; }

            public int DisposalCount { get; private set; }

            public bool IsLocked { get; private set; }

            public IDisposable AcquireLock()
            {
                ++AcquisitionCount;
                IsLocked = true;
                return new LockHandle(this);
            }

            private sealed class LockHandle : IDisposable
            {
                private readonly TrackingApplicationLocker _locker;

                public LockHandle(TrackingApplicationLocker locker)
                {
                    _locker = locker;
                }

                public void Dispose()
                {
                    ++_locker.DisposalCount;
                    _locker.IsLocked = false;
                }
            }
        }
    }
}
