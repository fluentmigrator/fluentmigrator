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

using FluentMigrator.Runner;
using FluentMigrator.Runner.Exceptions;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.ApplicationLocking
{
    [TestFixture]
    [Category("Runner")]
    [Category("ApplicationLocker")]
    public class ApplicationLockLeaseTests
    {
        private static ApplicationLockLease CreateLease(IApplicationLocker locker, TimeSpan? timeout = null)
        {
            return new ApplicationLockLease(locker, timeout ?? Timeout.InfiniteTimeSpan, logger: null);
        }

        [Test]
        public void LeaseWithoutLockerIsDisabledAndEnteringDoesNothing()
        {
            var lease = CreateLease(locker: null);

            lease.IsEnabled.ShouldBeFalse();
            using (lease.Enter())
            {
                lease.IsHeld.ShouldBeFalse();
            }
        }

        [Test]
        public void FirstEntryAcquiresAndLastExitReleases()
        {
            var locker = new TrackingApplicationLocker();
            var lease = CreateLease(locker);

            var outer = lease.Enter();
            var inner = lease.Enter();
            locker.AcquisitionCount.ShouldBe(1);

            outer.Dispose();
            lease.IsHeld.ShouldBeTrue("an entry is still outstanding");
            locker.IsLocked.ShouldBeTrue();

            inner.Dispose();
            lease.IsHeld.ShouldBeFalse();
            locker.ReleaseCount.ShouldBe(1);
        }

        [Test]
        public void DisposingAnEntryTwiceLeavesOnce()
        {
            var locker = new TrackingApplicationLocker();
            var lease = CreateLease(locker);

            var outer = lease.Enter();
            var inner = lease.Enter();
            inner.Dispose();
            inner.Dispose();

            lease.IsHeld.ShouldBeTrue();
            outer.Dispose();
            locker.ReleaseCount.ShouldBe(1);
        }

        [Test]
        public void ConcurrentEntriesShareOneAcquisition()
        {
            using var acquisitionMayProceed = new ManualResetEventSlim();
            var locker = new TrackingApplicationLocker { BlockAcquisitionUntil = acquisitionMayProceed };
            var lease = CreateLease(locker);

            var entries = new Task<IDisposable>[8];
            for (var i = 0; i < entries.Length; i++)
            {
                entries[i] = Task.Run(() => lease.Enter());
            }

            acquisitionMayProceed.Set();
            Task.WaitAll(entries);

            locker.AcquisitionCount.ShouldBe(1);
            locker.MaxSimultaneousHolders.ShouldBe(1);

            foreach (var entry in entries)
            {
                entry.Result.Dispose();
            }

            locker.ReleaseCount.ShouldBe(1);
            lease.IsHeld.ShouldBeFalse();
        }

        [Test]
        public void LockerFailureIsWrappedAndTheNextEntryTriesAgain()
        {
            var failure = new InvalidOperationException("lock store unavailable");
            var locker = new TrackingApplicationLocker { FailWith = failure };
            var lease = CreateLease(locker);

            var exception = Should.Throw<ApplicationLockException>(() => lease.Enter());
            exception.InnerException.ShouldBeSameAs(failure);
            lease.IsHeld.ShouldBeFalse();

            locker.FailWith = null;
            using (lease.Enter())
            {
                locker.AcquisitionCount.ShouldBe(1);
            }

            locker.ReleaseCount.ShouldBe(1);
        }

        [Test]
        public void NullHandleIsALockFailure()
        {
            var lease = CreateLease(new NullHandleLocker());

            Should.Throw<ApplicationLockException>(() => lease.Enter())
                .InnerException.ShouldBeOfType<InvalidOperationException>();
            lease.IsHeld.ShouldBeFalse();
        }

        [Test]
        public void TimeoutFailsWithApplicationLockExceptionWrappingTimeoutException()
        {
            var locker = new SemaphoreApplicationLocker { SelfDeadlockAfter = TimeSpan.FromSeconds(30) };
            var otherInstance = CreateLease(locker);
            var lease = CreateLease(locker, TimeSpan.FromMilliseconds(100));

            using (otherInstance.Enter())
            {
                var exception = Should.Throw<ApplicationLockException>(() => lease.Enter());
                exception.InnerException.ShouldBeOfType<TimeoutException>();
                lease.IsHeld.ShouldBeFalse();
            }
        }

        [Test]
        public void TimeoutAppliesToTheAsyncPath()
        {
            var locker = new SemaphoreApplicationLocker { SelfDeadlockAfter = TimeSpan.FromSeconds(30) };
            var otherInstance = CreateLease(locker);
            var lease = CreateLease(locker, TimeSpan.FromMilliseconds(100));

            using (otherInstance.Enter())
            {
                Should.Throw<ApplicationLockException>(async () => await lease.EnterAsync())
                    .InnerException.ShouldBeOfType<TimeoutException>();
            }
        }

        [Test]
        public void CallerCancellationPropagatesUnwrapped()
        {
            var locker = new SemaphoreApplicationLocker { SelfDeadlockAfter = TimeSpan.FromSeconds(30) };
            var otherInstance = CreateLease(locker);
            var lease = CreateLease(locker, TimeSpan.FromSeconds(30));
            using var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));

            using (otherInstance.Enter())
            {
                Should.Throw<OperationCanceledException>(() => lease.Enter(cancellation.Token));
                Should.Throw<OperationCanceledException>(async () => await lease.EnterAsync(cancellation.Token));
            }

            lease.IsHeld.ShouldBeFalse();
        }

        [Test]
        public async Task AsyncEntryAcquiresAsynchronouslyAndSharesWithSyncEntries()
        {
            var locker = new TrackingApplicationLocker();
            var lease = CreateLease(locker);

            await using (await lease.EnterAsync())
            {
                using (lease.Enter())
                {
                    locker.AcquisitionCount.ShouldBe(1);
                }

                lease.IsHeld.ShouldBeTrue();
            }

            lease.IsHeld.ShouldBeFalse();
            locker.AsyncReleaseCount.ShouldBe(1);
        }

        [Test]
        public async Task AsyncOnlyHandleIsReleasedOnASynchronousExit()
        {
            var locker = new TrackingApplicationLocker { AsyncHandleOnly = true };
            var lease = CreateLease(locker);

            var asyncEntry = await lease.EnterAsync();
            var syncEntry = lease.Enter();
            await asyncEntry.DisposeAsync();

            // The last exit is synchronous, but the handle is IAsyncDisposable only.
            syncEntry.Dispose();

            locker.IsLocked.ShouldBeFalse();
            locker.AsyncReleaseCount.ShouldBe(1);
        }

        [Test]
        public void EntryCanBeDisposedOnAnotherThread()
        {
            var locker = new TrackingApplicationLocker();
            var lease = CreateLease(locker);

            var entry = lease.Enter();
            var thread = new Thread(entry.Dispose);
            thread.Start();
            thread.Join();

            lease.IsHeld.ShouldBeFalse();
            locker.ReleaseCount.ShouldBe(1);
        }

        [Test]
        public void DisposingTheLeaseReleasesAnOutstandingLock()
        {
            var locker = new TrackingApplicationLocker();
            var lease = CreateLease(locker);

            var forgotten = lease.Enter();
            lease.Dispose();

            locker.IsLocked.ShouldBeFalse();
            locker.ReleaseCount.ShouldBe(1);

            // Disposing the forgotten entry afterwards must not release anything twice.
            forgotten.Dispose();
            locker.ReleaseCount.ShouldBe(1);
        }

        private sealed class NullHandleLocker : IApplicationLocker
        {
            public IDisposable AcquireLock(CancellationToken cancellationToken) => null;

            public ValueTask<IAsyncDisposable> AcquireLockAsync(CancellationToken cancellationToken) => default;
        }
    }
}
