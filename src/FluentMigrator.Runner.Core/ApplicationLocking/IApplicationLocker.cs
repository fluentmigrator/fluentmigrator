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

namespace FluentMigrator.Runner
{
    /// <summary>
    /// Acquires an application-wide lock so that only one application instance migrates a database at a time.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Register an implementation with
    /// <c>ConfigureApplicationLocker&lt;TApplicationLocker&gt;()</c>. The runner then holds the lock for
    /// every runner operation and every migration scope, and while the version table is being created.
    /// </para>
    /// <para><b>Contract.</b></para>
    /// <list type="bullet">
    /// <item><description>
    /// <b>Blocking.</b> Both methods return only once the lock is held. The returned handle releases the
    /// lock when it is disposed. Disposing it more than once must be harmless.
    /// </description></item>
    /// <item><description>
    /// <b>Reentrancy is not required.</b> FluentMigrator calls an implementation at most once per
    /// <see cref="IApplicationLockLease"/> (one per dependency-injection scope) until the handle is
    /// disposed. Nested runner operations, migration scopes, and work running on other threads in the same
    /// scope share that one acquisition. A non-reentrant lock, such as <c>sp_getapplock</c> on a dedicated
    /// connection, a PostgreSQL advisory lock, or a lease in a distributed store, is therefore safe.
    /// </description></item>
    /// <item><description>
    /// <b>Cancellation.</b> When the cancellation token is cancelled while the lock is being
    /// waited for, throw <see cref="OperationCanceledException"/> and do not hold the lock afterwards.
    /// FluentMigrator applies <see cref="ApplicationLockerOptions.AcquireTimeout"/> by cancelling this token.
    /// </description></item>
    /// <item><description>
    /// <b>Failure.</b> Any other exception means the lock could not be acquired. FluentMigrator wraps it in
    /// <see cref="Exceptions.ApplicationLockException"/>, so callers can tell a locking failure apart from a
    /// migration failure.
    /// </description></item>
    /// <item><description>
    /// <b>Prefer database-side locks.</b> When the lock lives in the database the migrations run against,
    /// the database's deadlock detector sees every wait. See <c>adr/proposed/ParallelAsyncAllTheWay.md</c>, rule DI-5.
    /// Use a connection dedicated to the lock: a lock owned by a migration transaction is released when
    /// that transaction commits.
    /// </description></item>
    /// </list>
    /// <para>
    /// The synchronous runner calls <see cref="AcquireLock"/>. <see cref="AcquireLockAsync"/> is the entry
    /// point for asynchronous callers, such as <see cref="IApplicationLockLease.EnterAsync"/>. An implementation
    /// whose underlying lock is synchronous can return
    /// <c>new ValueTask&lt;IAsyncDisposable&gt;(...)</c> from <see cref="AcquireLockAsync"/>. An
    /// implementation whose underlying lock is asynchronous decides itself how <see cref="AcquireLock"/> waits for it.
    /// </para>
    /// </remarks>
    public interface IApplicationLocker
    {
        /// <summary>
        /// Waits until the application-wide lock is acquired.
        /// </summary>
        /// <param name="cancellationToken">Cancels the wait.</param>
        /// <returns>A handle that releases the lock when disposed.</returns>
        /// <exception cref="OperationCanceledException">The wait was cancelled. The lock is not held.</exception>
        IDisposable AcquireLock(CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously waits until the application-wide lock is acquired.
        /// </summary>
        /// <param name="cancellationToken">Cancels the wait.</param>
        /// <returns>A handle that releases the lock when disposed.</returns>
        /// <exception cref="OperationCanceledException">The wait was cancelled. The lock is not held.</exception>
        ValueTask<IAsyncDisposable> AcquireLockAsync(CancellationToken cancellationToken);
    }
}
