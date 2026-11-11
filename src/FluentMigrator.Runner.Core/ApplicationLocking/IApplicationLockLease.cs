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
    /// The dependency-injection scope's share of the application-wide lock.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The lease is registered <b>scoped</b>, so its owner is the dependency-injection scope. That is the same
    /// boundary as the scope's migration processor and connection, which is FluentMigrator's unit of
    /// non-concurrency. Every participant in the scope enters the same lease: the runner, the version loader
    /// while it creates the version table, and application code that uses <see cref="IVersionLoader"/> or the
    /// processor directly.
    /// </para>
    /// <para>
    /// The lease counts entries. The first entry acquires the lock through <see cref="IApplicationLocker"/>,
    /// and disposing the last outstanding handle releases it. Ownership is tracked on the lease instance, not on
    /// the calling thread or <see cref="System.Threading.ExecutionContext"/>. A handle may therefore be disposed
    /// on any thread, and a scope begun inside an <c>async</c> method or <c>Task.Run</c> is honoured by the caller.
    /// </para>
    /// <para>
    /// When no <see cref="IApplicationLocker"/> is registered, or the runner has no connection (for example in
    /// preview mode), entering the lease does nothing.
    /// </para>
    /// </remarks>
    public interface IApplicationLockLease
    {
        /// <summary>
        /// Gets a value indicating whether this lease takes the application lock at all.
        /// </summary>
        bool IsEnabled { get; }

        /// <summary>
        /// Gets a value indicating whether this lease currently holds the application lock.
        /// </summary>
        bool IsHeld { get; }

        /// <summary>
        /// Enters the lease, acquiring the application lock if this is the first outstanding entry.
        /// </summary>
        /// <param name="cancellationToken">Cancels the wait for the lock.</param>
        /// <returns>A handle that leaves the lease when disposed. Disposing it more than once is harmless.</returns>
        /// <exception cref="Exceptions.ApplicationLockException">The lock could not be acquired, or the wait timed out.</exception>
        /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> was cancelled.</exception>
        IDisposable Enter(CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously enters the lease, acquiring the application lock if this is the first outstanding entry.
        /// </summary>
        /// <param name="cancellationToken">Cancels the wait for the lock.</param>
        /// <returns>A handle that leaves the lease when disposed. Disposing it more than once is harmless.</returns>
        /// <exception cref="Exceptions.ApplicationLockException">The lock could not be acquired, or the wait timed out.</exception>
        /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> was cancelled.</exception>
        ValueTask<IAsyncDisposable> EnterAsync(CancellationToken cancellationToken = default);
    }
}
