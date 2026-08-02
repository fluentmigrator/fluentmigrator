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

namespace FluentMigrator.Infrastructure
{
    /// <summary>
    /// Acquires an application-wide lock before migrations are inspected or executed.
    /// </summary>
    /// <remarks>
    /// Implementations may coordinate through a database, distributed store, or platform-specific
    /// leader-election service. The lock is held until the returned handle is disposed.
    /// </remarks>
    /// <example>
    /// <code>
    /// public IDisposable AcquireLock()
    /// {
    ///     return distributedLock.Acquire("migrations");
    /// }
    /// </code>
    /// </example>
    public interface IApplicationLocker
    {
        /// <summary>
        /// Acquires the application-wide migration lock.
        /// </summary>
        /// <returns>A handle that releases the lock when disposed.</returns>
        IDisposable AcquireLock();
    }
}
