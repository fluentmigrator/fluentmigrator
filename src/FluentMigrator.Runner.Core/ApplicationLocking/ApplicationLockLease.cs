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
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using FluentMigrator.Runner.Exceptions;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace FluentMigrator.Runner
{
    /// <summary>
    /// Reference-counted, instance-owned share of the application lock. See <see cref="IApplicationLockLease"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// State lives in fields of this instance and changes only while <see cref="_gate"/> is held. No
    /// <see cref="AsyncLocal{T}"/>, <see cref="ThreadLocal{T}"/> or other ambient state is involved, so the
    /// result does not depend on which thread or execution context enters or leaves.
    /// </para>
    /// <para>
    /// The gate is a <see cref="SemaphoreSlim"/> so that <see cref="EnterAsync"/> can wait for it
    /// asynchronously. It is not reentrant, and does not need to be: it is held only while the count changes
    /// and the lock is acquired or released, never while caller code runs.
    /// </para>
    /// </remarks>
    internal sealed class ApplicationLockLease : IApplicationLockLease, IDisposable
    {
        private readonly IApplicationLocker _locker;
        private readonly TimeSpan _acquireTimeout;
        private readonly ILogger _logger;
        private readonly SemaphoreSlim _gate = new SemaphoreSlim(1, 1);

        // Guarded by _gate. The handle is an IDisposable (from AcquireLock) or an IAsyncDisposable
        // (from AcquireLockAsync).
        private object _handle;
        private int _depth;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationLockLease"/> class.
        /// </summary>
        /// <param name="locker">The locker, or <c>null</c> to disable the lease.</param>
        /// <param name="acquireTimeout">How long to wait for the lock.</param>
        /// <param name="logger">The logger.</param>
        public ApplicationLockLease(IApplicationLocker locker, TimeSpan acquireTimeout, ILogger logger)
        {
            _locker = locker;
            _acquireTimeout = acquireTimeout;
            _logger = logger ?? NullLogger.Instance;
        }

        /// <inheritdoc />
        public bool IsEnabled => _locker != null;

        /// <inheritdoc />
        public bool IsHeld => Volatile.Read(ref _handle) != null;

        /// <inheritdoc />
        public IDisposable Enter(CancellationToken cancellationToken = default)
        {
            if (_locker == null)
            {
                return Releaser.None;
            }

            var timeoutSource = CreateTimeoutSource(cancellationToken, out var token);
            try
            {
                _gate.Wait(token);
                try
                {
                    if (_depth == 0)
                    {
                        var stopwatch = LogAcquiring();
                        _handle = _locker.AcquireLock(token)
                         ?? throw new InvalidOperationException($"{_locker.GetType()}.{nameof(IApplicationLocker.AcquireLock)} returned null.");
                        LogAcquired(stopwatch);
                    }

                    _depth++;
                }
                finally
                {
                    _gate.Release();
                }
            }
            catch (Exception ex) when (IsLockFailure(ex, cancellationToken))
            {
                throw CreateLockException(ex, timeoutSource);
            }
            finally
            {
                timeoutSource?.Dispose();
            }

            return new Releaser(this);
        }

        /// <inheritdoc />
        public async ValueTask<IAsyncDisposable> EnterAsync(CancellationToken cancellationToken = default)
        {
            if (_locker == null)
            {
                return Releaser.None;
            }

            var timeoutSource = CreateTimeoutSource(cancellationToken, out var token);
            try
            {
                await _gate.WaitAsync(token).ConfigureAwait(false);
                try
                {
                    if (_depth == 0)
                    {
                        var stopwatch = LogAcquiring();
                        _handle = await _locker.AcquireLockAsync(token).ConfigureAwait(false)
                         ?? throw new InvalidOperationException($"{_locker.GetType()}.{nameof(IApplicationLocker.AcquireLockAsync)} returned null.");
                        LogAcquired(stopwatch);
                    }

                    _depth++;
                }
                finally
                {
                    _gate.Release();
                }
            }
            catch (Exception ex) when (IsLockFailure(ex, cancellationToken))
            {
                throw CreateLockException(ex, timeoutSource);
            }
            finally
            {
                timeoutSource?.Dispose();
            }

            return new Releaser(this);
        }

        /// <summary>
        /// Releases the lock if the dependency-injection scope ends while entries are still outstanding,
        /// for example because a migration scope was never disposed.
        /// </summary>
        public void Dispose()
        {
            _gate.Wait();
            try
            {
                if (_handle != null)
                {
                    _logger.LogWarning("Releasing the application lock because its dependency-injection scope ended with {Count} outstanding entries.", _depth);
                    _depth = 0;
                    ReleaseHandle();
                }
            }
            finally
            {
                _gate.Release();
            }
        }

        private void Leave()
        {
            // Not cancellable: a release must not be skipped.
            _gate.Wait();
            try
            {
                if (_depth > 0 && --_depth == 0)
                {
                    ReleaseHandle();
                }
            }
            finally
            {
                _gate.Release();
            }
        }

        private async ValueTask LeaveAsync()
        {
            await _gate.WaitAsync().ConfigureAwait(false);
            try
            {
                if (_depth > 0 && --_depth == 0)
                {
                    var handle = _handle;
                    _handle = null;
                    if (handle is IAsyncDisposable asyncDisposable)
                    {
                        await asyncDisposable.DisposeAsync().ConfigureAwait(false);
                    }
                    else
                    {
                        ((IDisposable)handle).Dispose();
                    }

                    _logger.LogDebug("Released the application lock.");
                }
            }
            finally
            {
                _gate.Release();
            }
        }

        private void ReleaseHandle()
        {
            var handle = _handle;
            _handle = null;
            if (handle is IDisposable disposable)
            {
                disposable.Dispose();
            }
            else
            {
                // Acquired asynchronously but released on a synchronous path. Block on a pool thread so that
                // a captured SynchronizationContext cannot deadlock the release (AsyncAllTheWay.md, D4).
                var asyncDisposable = (IAsyncDisposable)handle;
                Task.Run(() => asyncDisposable.DisposeAsync().AsTask()).GetAwaiter().GetResult();
            }

            _logger.LogDebug("Released the application lock.");
        }

        private CancellationTokenSource CreateTimeoutSource(CancellationToken cancellationToken, out CancellationToken token)
        {
            if (_acquireTimeout == Timeout.InfiniteTimeSpan)
            {
                token = cancellationToken;
                return null;
            }

            var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            source.CancelAfter(_acquireTimeout);
            token = source.Token;
            return source;
        }

        private static bool IsLockFailure(Exception exception, CancellationToken callerToken)
        {
            // The caller's own cancellation propagates unchanged (AsyncAllTheWay.md, R1). Everything else,
            // including a cancellation caused by the timeout, is a failure to acquire the lock.
            return !(exception is OperationCanceledException && callerToken.IsCancellationRequested);
        }

        private Exception CreateLockException(Exception exception, CancellationTokenSource timeoutSource)
        {
            if (exception is OperationCanceledException && timeoutSource?.IsCancellationRequested == true)
            {
                var message = $"Timed out after {_acquireTimeout} waiting for the application lock.";
                return new ApplicationLockException(message, new TimeoutException(message, exception));
            }

            return new ApplicationLockException($"Could not acquire the application lock: {exception.Message}", exception);
        }

        private Stopwatch LogAcquiring()
        {
            _logger.LogInformation("Waiting for the application lock.");
            return Stopwatch.StartNew();
        }

        private void LogAcquired(Stopwatch stopwatch)
        {
            _logger.LogInformation("Acquired the application lock after {Elapsed}.", stopwatch.Elapsed);
        }

        private sealed class Releaser : IDisposable, IAsyncDisposable
        {
            public static readonly Releaser None = new Releaser(null);

            private ApplicationLockLease _lease;

            public Releaser(ApplicationLockLease lease)
            {
                _lease = lease;
            }

            public void Dispose()
            {
                Interlocked.Exchange(ref _lease, null)?.Leave();
            }

            public ValueTask DisposeAsync()
            {
                var lease = Interlocked.Exchange(ref _lease, null);
                return lease == null ? default : lease.LeaveAsync();
            }
        }
    }
}
