#region License
// Copyright (c) 2007-2018, FluentMigrator Project
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
    /// A migration scope encapsulating migrations in a transaction
    /// </summary>
    public class TransactionalMigrationScope : TrackingMigrationScope
    {
        private readonly IMigrationProcessor _migrationProcessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionalMigrationScope"/> class.
        /// </summary>
        /// <param name="processor">The migration processor</param>
        /// <param name="disposalAction">Called after the scope was cancelled</param>
        public TransactionalMigrationScope(IMigrationProcessor processor, Action disposalAction)
            : base(disposalAction)
        {
            _migrationProcessor = processor ?? throw new ArgumentNullException(nameof(processor));
            _migrationProcessor.BeginTransaction();
        }

        /// <inheritdoc />
        protected override void DoComplete()
        {
            _migrationProcessor.CommitTransaction();
        }

        /// <inheritdoc />
        protected override void DoCancel()
        {
            _migrationProcessor.RollbackTransaction();
        }
    }
}
