#region License

// Copyright (c) 2007-2018, Sean Chambers <schambers80@gmail.com>
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

namespace FluentMigrator.Runner
{
    /// <summary>
    /// A migration scope that encapsulates database changes in a transaction
    /// </summary>
    public interface IMigrationScope : IDisposable
    {
        /// <summary>
        /// Gets a value indicating whether the migration scope is active?
        /// </summary>
        /// <remarks>
        /// A migration scope is considered active when it is the outmost scope.
        /// </remarks>
        bool IsActive { get; }

        /// <summary>
        /// Marks the scope as complete.
        /// </summary>
        /// <remarks>
        /// This usually involves the commit a transaction.
        /// </remarks>
        void Complete();

        /// <summary>
        /// Marks the scope as cancelled.
        /// </summary>
        /// <remarks>
        /// This usually involves the rollback of a transaction.
        /// </remarks>
        void Cancel();
    }
}
