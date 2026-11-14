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

using FluentMigrator.Infrastructure;
using FluentMigrator.Runner.Initialization;

namespace FluentMigrator.Runner
{
    /// <summary>
    /// Enters the scope's <see cref="IApplicationLockLease"/> around every runner operation and for the lifetime
    /// of every migration scope.
    /// </summary>
    /// <remarks>
    /// This decorator holds no lock state of its own. The lease owns the lock, so code that resolves
    /// <see cref="IVersionLoader"/> or the processor directly shares the same acquisition.
    /// </remarks>
    internal sealed class ApplicationLockingMigrationRunner : IMigrationRunner
    {
        private readonly IMigrationRunner _runner;
        private readonly IApplicationLockLease _lease;

        public ApplicationLockingMigrationRunner(IMigrationRunner runner, IApplicationLockLease lease)
        {
            _runner = runner ?? throw new ArgumentNullException(nameof(runner));
            _lease = lease ?? throw new ArgumentNullException(nameof(lease));
        }

        public IMigrationProcessor Processor => _runner.Processor;

        public IMigrationInformationLoader MigrationLoader => _runner.MigrationLoader;

        public void Up(IMigration migration)
        {
            using (_lease.Enter())
            {
                _runner.Up(migration);
            }
        }

        public void Down(IMigration migration)
        {
            using (_lease.Enter())
            {
                _runner.Down(migration);
            }
        }

        public void MigrateUp()
        {
            using (_lease.Enter())
            {
                _runner.MigrateUp();
            }
        }

        public void MigrateUp(long version)
        {
            using (_lease.Enter())
            {
                _runner.MigrateUp(version);
            }
        }

        public void Rollback(int steps)
        {
            using (_lease.Enter())
            {
                _runner.Rollback(steps);
            }
        }

        public void RollbackToVersion(long version)
        {
            using (_lease.Enter())
            {
                _runner.RollbackToVersion(version);
            }
        }

        public void MigrateDown(long version)
        {
            using (_lease.Enter())
            {
                _runner.MigrateDown(version);
            }
        }

        public void ValidateVersionOrder()
        {
            using (_lease.Enter())
            {
                _runner.ValidateVersionOrder();
            }
        }

        public void ListMigrations()
        {
            using (_lease.Enter())
            {
                _runner.ListMigrations();
            }
        }

        public bool HasMigrationsToApplyUp(long? version = null)
        {
            using (_lease.Enter())
            {
                return _runner.HasMigrationsToApplyUp(version);
            }
        }

        public bool HasMigrationsToApplyDown(long version)
        {
            using (_lease.Enter())
            {
                return _runner.HasMigrationsToApplyDown(version);
            }
        }

        public bool HasMigrationsToApplyRollback()
        {
            using (_lease.Enter())
            {
                return _runner.HasMigrationsToApplyRollback();
            }
        }

        public bool LoadVersionInfoIfRequired()
        {
            using (_lease.Enter())
            {
                return _runner.LoadVersionInfoIfRequired();
            }
        }

        public IMigrationScope BeginScope()
        {
            var entry = _lease.Enter();
            try
            {
                return new LeasedMigrationScope(_runner.BeginScope(), entry);
            }
            catch
            {
                entry.Dispose();
                throw;
            }
        }

        private sealed class LeasedMigrationScope : IMigrationScope
        {
            private readonly IMigrationScope _scope;
            private readonly IDisposable _entry;

            public LeasedMigrationScope(IMigrationScope scope, IDisposable entry)
            {
                _scope = scope;
                _entry = entry;
            }

            public bool IsActive => _scope.IsActive;

            public void Complete() => _scope.Complete();

            public void Cancel() => _scope.Cancel();

            public void Dispose()
            {
                // Leave the lease only after the scope has committed or rolled back. The entry is
                // idempotent, so a second Dispose is harmless.
                try
                {
                    _scope.Dispose();
                }
                finally
                {
                    _entry.Dispose();
                }
            }
        }
    }
}
