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

namespace FluentMigrator.Runner.Exceptions
{
    /// <summary>
    /// The application-wide migration lock could not be acquired.
    /// </summary>
    /// <remarks>
    /// Thrown when <see cref="IApplicationLocker"/> fails, or when <see cref="ApplicationLockerOptions.AcquireTimeout"/>
    /// elapses first (the inner exception is then a <see cref="TimeoutException"/>). No migration has been attempted
    /// by the operation that threw it. Cancellation requested by the caller is reported as
    /// <see cref="OperationCanceledException"/>, not as this exception.
    /// </remarks>
    public class ApplicationLockException : RunnerException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationLockException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The reason the lock could not be acquired.</param>
        public ApplicationLockException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
