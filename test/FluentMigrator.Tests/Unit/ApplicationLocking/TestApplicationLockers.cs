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

namespace FluentMigrator.Tests.Unit.ApplicationLocking
{
    /// <summary>
    /// Counts acquisitions and releases. Records the highest number of simultaneous holders, which is
    /// above one only if the same lock was acquired twice without a release in between.
    /// </summary>
    public sealed class TrackingApplicationLocker : IApplicationLocker
    {
        private int _acquisitions;
        private int _releases;
        private int _holders;
        private int _maxHolders;
        private int _asyncReleases;

        public int AcquisitionCount => Volatile.Read(ref _acquisitions);

        public int ReleaseCount => Volatile.Read(ref _releases);

        public int AsyncReleaseCount => Volatile.Read(ref _asyncReleases);

        public int MaxSimultaneousHolders => Volatile.Read(ref _maxHolders);

        public bool IsLocked => Volatile.Read(ref _holders) > 0;

        /// <summary>Gets or sets a value indicating whether <see cref="AcquireLockAsync"/> returns a handle that is only <see cref="IAsyncDisposable"/>.</summary>
        public bool AsyncHandleOnly { get; set; }

        /// <summary>Gets or sets an exception to throw from both acquire methods.</summary>
        public Exception FailWith { get; set; }

        /// <summary>Gets or sets an event that acquisition waits for before taking the lock.</summary>
        public ManualResetEventSlim BlockAcquisitionUntil { get; set; }

        public IDisposable AcquireLock(CancellationToken cancellationToken)
        {
            return Acquire(cancellationToken);
        }

        public async ValueTask<IAsyncDisposable> AcquireLockAsync(CancellationToken cancellationToken)
        {
            await Task.Yield();
            var handle = Acquire(cancellationToken);
            return AsyncHandleOnly ? new AsyncOnlyHandle(handle) : (IAsyncDisposable)handle;
        }

        private Handle Acquire(CancellationToken cancellationToken)
        {
            BlockAcquisitionUntil?.Wait(cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            if (FailWith != null)
            {
                throw FailWith;
            }

            Interlocked.Increment(ref _acquisitions);
            var holders = Interlocked.Increment(ref _holders);
            int max;
            while (holders > (max = Volatile.Read(ref _maxHolders))
                && Interlocked.CompareExchange(ref _maxHolders, holders, max) != max)
            {
            }

            return new Handle(this);
        }

        private sealed class Handle : IDisposable, IAsyncDisposable
        {
            private TrackingApplicationLocker _locker;

            public Handle(TrackingApplicationLocker locker) => _locker = locker;

            public void Dispose()
            {
                var locker = Interlocked.Exchange(ref _locker, null);
                if (locker != null)
                {
                    Interlocked.Increment(ref locker._releases);
                    Interlocked.Decrement(ref locker._holders);
                }
            }

            public ValueTask DisposeAsync()
            {
                var locker = Volatile.Read(ref _locker);
                if (locker != null)
                {
                    Interlocked.Increment(ref locker._asyncReleases);
                }

                Dispose();
                return default;
            }
        }

        private sealed class AsyncOnlyHandle : IAsyncDisposable
        {
            private readonly Handle _handle;

            public AsyncOnlyHandle(Handle handle) => _handle = handle;

            public async ValueTask DisposeAsync()
            {
                await Task.Yield();
                await _handle.DisposeAsync();
            }
        }
    }

    /// <summary>
    /// Non-reentrant, like <c>sp_getapplock</c> on a dedicated connection, a PostgreSQL advisory lock, a
    /// Kubernetes lease or a blob lease. A second acquisition while held fails after two seconds with
    /// "self-deadlock" instead of hanging the test run.
    /// </summary>
    public sealed class SemaphoreApplicationLocker : IApplicationLocker
    {
        private readonly SemaphoreSlim _gate = new SemaphoreSlim(1, 1);

        public TimeSpan SelfDeadlockAfter { get; set; } = TimeSpan.FromSeconds(2);

        public bool IsHeld => _gate.CurrentCount == 0;

        public IDisposable AcquireLock(CancellationToken cancellationToken)
        {
            if (!_gate.Wait(SelfDeadlockAfter, cancellationToken))
            {
                throw new TimeoutException("self-deadlock");
            }

            return new Releaser(_gate);
        }

        public async ValueTask<IAsyncDisposable> AcquireLockAsync(CancellationToken cancellationToken)
        {
            if (!await _gate.WaitAsync(SelfDeadlockAfter, cancellationToken))
            {
                throw new TimeoutException("self-deadlock");
            }

            return new Releaser(_gate);
        }

        private sealed class Releaser : IDisposable, IAsyncDisposable
        {
            private SemaphoreSlim _gate;

            public Releaser(SemaphoreSlim gate) => _gate = gate;

            public void Dispose() => Interlocked.Exchange(ref _gate, null)?.Release();

            public ValueTask DisposeAsync()
            {
                Dispose();
                return default;
            }
        }
    }
}
