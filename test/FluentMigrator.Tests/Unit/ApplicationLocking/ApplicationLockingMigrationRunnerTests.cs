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
using System.Threading;
using System.Threading.Tasks;

using FluentMigrator.Infrastructure;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Generators.Generic;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Moq;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.ApplicationLocking
{
    [TestFixture]
    [Category("Runner")]
    [Category("ApplicationLocker")]
    public class ApplicationLockingMigrationRunnerTests
    {
        private static ApplicationLockingMigrationRunner CreateRunner(
            Mock<IMigrationRunner> innerRunner,
            IApplicationLocker locker)
        {
            innerRunner.Setup(x => x.BeginScope()).Returns(() => Mock.Of<IMigrationScope>());
            var lease = new ApplicationLockLease(locker, Timeout.InfiniteTimeSpan, logger: null);
            return new ApplicationLockingMigrationRunner(innerRunner.Object, lease);
        }

        [Test]
        public void RunnerOperationIsExecutedWhileLockIsHeld()
        {
            var locker = new TrackingApplicationLocker();
            var innerRunner = new Mock<IMigrationRunner>();
            var lockedDuringOperation = false;
            innerRunner.Setup(x => x.MigrateUp()).Callback(() => lockedDuringOperation = locker.IsLocked);
            var runner = CreateRunner(innerRunner, locker);

            runner.MigrateUp();

            lockedDuringOperation.ShouldBeTrue();
            locker.AcquisitionCount.ShouldBe(1);
            locker.ReleaseCount.ShouldBe(1);
        }

        [Test]
        public void NestedRunnerOperationDoesNotReacquireLock()
        {
            var locker = new SemaphoreApplicationLocker();
            var innerRunner = new Mock<IMigrationRunner>();
            var migration = Mock.Of<IMigration>();
            ApplicationLockingMigrationRunner runner = null;
            innerRunner.Setup(x => x.MigrateUp()).Callback(() => runner.Up(migration));
            runner = CreateRunner(innerRunner, locker);

            Should.NotThrow(() => runner.MigrateUp());

            innerRunner.Verify(x => x.Up(migration), Times.Once());
            locker.IsHeld.ShouldBeFalse();
        }

        [Test]
        public void LockIsReleasedWhenRunnerOperationThrows()
        {
            var locker = new TrackingApplicationLocker();
            var innerRunner = new Mock<IMigrationRunner>();
            innerRunner.Setup(x => x.MigrateUp()).Throws<InvalidOperationException>();
            var runner = CreateRunner(innerRunner, locker);

            Should.Throw<InvalidOperationException>(() => runner.MigrateUp());

            locker.ReleaseCount.ShouldBe(1);
            locker.IsLocked.ShouldBeFalse();
        }

        [Test]
        public void BeginScopeHoldsLockUntilScopeIsDisposed()
        {
            var locker = new TrackingApplicationLocker();
            var migrationScope = new Mock<IMigrationScope>();
            var innerRunner = new Mock<IMigrationRunner>();
            var runner = CreateRunner(innerRunner, locker);
            innerRunner.Setup(x => x.BeginScope()).Returns(migrationScope.Object);

            using (var scope = runner.BeginScope())
            {
                locker.IsLocked.ShouldBeTrue();
                runner.MigrateUp();
                scope.Complete();
            }

            migrationScope.Verify(x => x.Complete(), Times.Once());
            migrationScope.Verify(x => x.Dispose(), Times.Once());
            locker.AcquisitionCount.ShouldBe(1);
            locker.ReleaseCount.ShouldBe(1);
        }

        [Test]
        public void BeginScopeLeavesTheLeaseWhenTheInnerScopeCannotBeCreated()
        {
            var locker = new TrackingApplicationLocker();
            var innerRunner = new Mock<IMigrationRunner>();
            var runner = CreateRunner(innerRunner, locker);
            innerRunner.Setup(x => x.BeginScope()).Throws<InvalidOperationException>();

            Should.Throw<InvalidOperationException>(() => runner.BeginScope());

            locker.IsLocked.ShouldBeFalse();
        }

        [Test]
        public void DisposingAScopeTwiceReleasesOnce()
        {
            var locker = new TrackingApplicationLocker();
            var runner = CreateRunner(new Mock<IMigrationRunner>(), locker);

            var outer = runner.BeginScope();
            var inner = runner.BeginScope();
            inner.Dispose();
            inner.Dispose();

            locker.IsLocked.ShouldBeTrue("the outer scope is still open");
            outer.Dispose();
            locker.ReleaseCount.ShouldBe(1);
        }

        [Test]
        public void OperationAfterTheScopeReleasedTheLockReacquiresIt()
        {
            var locker = new TrackingApplicationLocker();
            var runner = CreateRunner(new Mock<IMigrationRunner>(), locker);
            using var childStarted = new ManualResetEventSlim();
            using var runChild = new ManualResetEventSlim();
            Task child;

            using (runner.BeginScope())
            {
                child = Task.Run(
                    () =>
                    {
                        childStarted.Set();
                        runChild.Wait();
                        runner.MigrateUp();
                    });
                childStarted.Wait();
            }

            locker.IsLocked.ShouldBeFalse();
            runChild.Set();
            child.Wait();

            locker.AcquisitionCount.ShouldBe(2);
            locker.ReleaseCount.ShouldBe(2);
            locker.MaxSimultaneousHolders.ShouldBe(1);
        }

        [Test]
        public void OperationOnAnotherThreadInsideAScopeDoesNotReacquireTheLock()
        {
            var locker = new SemaphoreApplicationLocker();
            var runner = CreateRunner(new Mock<IMigrationRunner>(), locker);

            using (runner.BeginScope())
            {
                Exception threadException = null;
                var thread = new Thread(
                    () =>
                    {
                        try
                        {
                            runner.MigrateUp();
                        }
                        catch (Exception ex)
                        {
                            threadException = ex;
                        }
                    });
                thread.Start();
                thread.Join();

                threadException.ShouldBeNull();
                locker.IsHeld.ShouldBeTrue();
            }

            locker.IsHeld.ShouldBeFalse();
        }

        [Test]
        public void ScopeDisposedOnAnotherThreadReleasesTheLock()
        {
            var locker = new TrackingApplicationLocker();
            var runner = CreateRunner(new Mock<IMigrationRunner>(), locker);

            var scope = runner.BeginScope();
            var thread = new Thread(scope.Dispose);
            thread.Start();
            thread.Join();

            locker.IsLocked.ShouldBeFalse();

            runner.MigrateUp();

            locker.AcquisitionCount.ShouldBe(2);
            locker.ReleaseCount.ShouldBe(2);
        }

        /// <summary>
        /// Repro from the review of the first version of this PR (#2371): a flow that is already inside
        /// an operation keeps running after the scope that started it releases. It must still hold the lock.
        /// </summary>
        [Test]
        public void ChildFlowAlreadyInsideOperationKeepsLockUntilItFinishes()
        {
            var locker = new SemaphoreApplicationLocker();
            var innerRunner = new Mock<IMigrationRunner>();
            var runner = CreateRunner(innerRunner, locker);
            using var insideInner = new ManualResetEventSlim();
            using var finishInner = new ManualResetEventSlim();
            var lockHeldWhenChildFinished = false;
            innerRunner
                .Setup(x => x.MigrateUp())
                .Callback(
                    () =>
                    {
                        insideInner.Set();
                        finishInner.Wait();
                        lockHeldWhenChildFinished = locker.IsHeld;
                    });
            Task child;

            using (runner.BeginScope())
            {
                child = Task.Run(() => runner.MigrateUp());
                insideInner.Wait();
            }

            finishInner.Set();
            child.Wait();

            lockHeldWhenChildFinished.ShouldBeTrue();
            locker.IsHeld.ShouldBeFalse();
        }

        /// <summary>
        /// Repro from the review of the first version of this PR (#2371): an <c>AsyncLocal</c> assignment
        /// made inside an async method is reverted when it returns, so a scope begun there was invisible to
        /// the caller, whose next operation self-deadlocked on a non-reentrant lock. This is the shape a
        /// future <c>BeginScopeAsync</c> will have.
        /// </summary>
        [Test]
        public async Task ScopeBegunInsideAsyncHelperIsSeenByCaller()
        {
            var locker = new SemaphoreApplicationLocker();
            var runner = CreateRunner(new Mock<IMigrationRunner>(), locker);

            using (await BeginScopeAsync(runner))
            {
                Should.NotThrow(() => runner.MigrateUp());
                locker.IsHeld.ShouldBeTrue();
            }

            locker.IsHeld.ShouldBeFalse();
        }

        [Test]
        public void ScopeBegunInsideTaskRunIsSeenByCaller()
        {
            var locker = new SemaphoreApplicationLocker();
            var runner = CreateRunner(new Mock<IMigrationRunner>(), locker);

            using (Task.Run(() => runner.BeginScope()).Result)
            {
                Should.NotThrow(() => runner.MigrateUp());
                locker.IsHeld.ShouldBeTrue();
            }

            locker.IsHeld.ShouldBeFalse();
        }

        [Test]
        public void RunnerWithoutLockerIsTheMigrationRunnerItself()
        {
            using var serviceProvider = CreateConnectedServices(new Mock<IMigrationProcessor>()).BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();

            scope.ServiceProvider.GetRequiredService<IMigrationRunner>().ShouldBeOfType<MigrationRunner>();
            scope.ServiceProvider.GetRequiredService<IApplicationLockLease>().IsEnabled.ShouldBeFalse();
        }

        [Test]
        public void ConfiguredLockerEncompassesVersionTableCreationThroughTheRunner()
        {
            var processor = new Mock<IMigrationProcessor>();
            var services = CreateConnectedServices(processor)
                .ConfigureApplicationLocker<TrackingApplicationLocker>();
            using var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var locker = (TrackingApplicationLocker)scope.ServiceProvider.GetRequiredService<IApplicationLocker>();
            var unlockedChecks = 0;
            processor
                .Setup(x => x.TableExists(It.IsAny<string>(), It.IsAny<string>()))
                .Callback(() => unlockedChecks += locker.IsLocked ? 0 : 1)
                .Returns(false);

            var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
            runner.ShouldBeOfType<ApplicationLockingMigrationRunner>();
            runner.LoadVersionInfoIfRequired();

            unlockedChecks.ShouldBe(0);
            locker.AcquisitionCount.ShouldBe(1);
            locker.ReleaseCount.ShouldBe(1);
        }

        /// <summary>
        /// Review of the first version of this PR (#2371): resolving <see cref="IVersionLoader"/> directly ran
        /// its check-then-create outside the lock, with one acquisition per created object. That is the race
        /// from #1949 through a public service.
        /// </summary>
        [Test]
        public void VersionLoaderResolvedDirectlyCreatesTheVersionTableUnderOneAcquisition()
        {
            var processor = new Mock<IMigrationProcessor>();
            var services = CreateConnectedServices(processor)
                .ConfigureApplicationLocker<TrackingApplicationLocker>();
            using var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var locker = (TrackingApplicationLocker)scope.ServiceProvider.GetRequiredService<IApplicationLocker>();
            var checks = 0;
            var unlockedChecks = 0;
            processor
                .Setup(x => x.TableExists(It.IsAny<string>(), It.IsAny<string>()))
                .Callback(
                    () =>
                    {
                        checks++;
                        unlockedChecks += locker.IsLocked ? 0 : 1;
                    })
                .Returns(false);

            scope.ServiceProvider.GetRequiredService<IVersionLoader>();

            checks.ShouldBeGreaterThan(0);
            unlockedChecks.ShouldBe(0);
            locker.AcquisitionCount.ShouldBe(1);
            locker.IsLocked.ShouldBeFalse();
        }

        [Test]
        public void ConnectionlessRunDoesNotTakeTheLock()
        {
            var services = CreateConnectedServices(new Mock<IMigrationProcessor>())
                .Configure<RunnerOptions>(o => o.NoConnection = true)
                .ConfigureApplicationLocker(_ => throw new AssertionException("The locker must not be created without a connection."));
            using var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();

            scope.ServiceProvider.GetRequiredService<IMigrationRunner>().ShouldBeOfType<MigrationRunner>();
            scope.ServiceProvider.GetRequiredService<IApplicationLockLease>().IsEnabled.ShouldBeFalse();
        }

        [Test]
        public void FactoryOverloadRegistersTheLockerAndItsOptions()
        {
            var locker = new TrackingApplicationLocker();
            var services = CreateConnectedServices(new Mock<IMigrationProcessor>())
                .ConfigureApplicationLocker(_ => locker, o => o.AcquireTimeout = TimeSpan.FromSeconds(42));
            using var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();

            serviceProvider.GetRequiredService<IOptions<ApplicationLockerOptions>>().Value.AcquireTimeout
                .ShouldBe(TimeSpan.FromSeconds(42));
            scope.ServiceProvider.GetRequiredService<IApplicationLocker>().ShouldBeSameAs(locker);
            scope.ServiceProvider.GetRequiredService<IMigrationRunner>().ShouldBeOfType<ApplicationLockingMigrationRunner>();
        }

        [Test]
        public void EachDependencyInjectionScopeOwnsItsOwnLease()
        {
            var locker = new SemaphoreApplicationLocker { SelfDeadlockAfter = TimeSpan.FromMilliseconds(200) };
            var services = CreateConnectedServices(new Mock<IMigrationProcessor>())
                .ConfigureApplicationLocker(_ => locker);
            using var serviceProvider = services.BuildServiceProvider();
            using var first = serviceProvider.CreateScope();
            using var second = serviceProvider.CreateScope();

            using (first.ServiceProvider.GetRequiredService<IApplicationLockLease>().Enter())
            {
                // A second scope stands for a second application instance: it must wait for the lock.
                Should.Throw<Runner.Exceptions.ApplicationLockException>(
                    () => second.ServiceProvider.GetRequiredService<IApplicationLockLease>().Enter());
            }

            using (second.ServiceProvider.GetRequiredService<IApplicationLockLease>().Enter())
            {
                locker.IsHeld.ShouldBeTrue();
            }
        }

        private static async Task<IMigrationScope> BeginScopeAsync(IMigrationRunner runner)
        {
            await Task.Yield();
            return runner.BeginScope();
        }

        private static IServiceCollection CreateConnectedServices(Mock<IMigrationProcessor> processor)
        {
            var generator = new Mock<IMigrationGenerator>();
            generator.SetupGet(x => x.Quoter).Returns(new GenericQuoter());
            var generatorAccessor = new Mock<IGeneratorAccessor>();
            generatorAccessor.SetupGet(x => x.Generator).Returns(generator.Object);

            return ServiceCollectionExtensions.CreateServices(false)
                .WithProcessor(processor)
                .AddSingleton(generatorAccessor.Object)
                .Configure<ProcessorOptions>(x => x.ConnectionString = "test");
        }
    }
}
