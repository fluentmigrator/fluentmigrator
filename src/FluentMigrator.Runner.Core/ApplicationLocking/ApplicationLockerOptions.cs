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

namespace FluentMigrator.Runner
{
    /// <summary>
    /// Options for the application-wide migration lock.
    /// </summary>
    public class ApplicationLockerOptions
    {
        /// <summary>
        /// Gets or sets how long to wait for the application lock before failing with
        /// <see cref="Exceptions.ApplicationLockException"/>.
        /// </summary>
        /// <remarks>
        /// The default, <see cref="Timeout.InfiniteTimeSpan"/>, waits until the lock is acquired. Set a finite
        /// value so that an instance stuck behind an unhealthy lock holder fails, and a host such as a
        /// Kubernetes startup probe can restart it.
        /// </remarks>
        public TimeSpan AcquireTimeout { get; set; } = Timeout.InfiniteTimeSpan;
    }
}
