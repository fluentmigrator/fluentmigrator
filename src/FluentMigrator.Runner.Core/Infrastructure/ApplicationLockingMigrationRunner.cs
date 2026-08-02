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

using FluentMigrator.Infrastructure;
using FluentMigrator.Runner.Initialization;

namespace FluentMigrator.Runner.Infrastructure
{
    internal sealed class ApplicationLockingMigrationRunner : IMigrationRunner
    {
        private readonly IMigrationRunner _runner;
        private readonly IApplicationLocker _applicationLocker;
        private readonly AsyncLocal<int> _lockDepth = new AsyncLocal<int>();

        public ApplicationLockingMigrationRunner(
            IMigrationRunner runner,
            IApplicationLocker applicationLocker)
        {
            _runner = runner;
            _applicationLocker = applicationLocker;
        }

        public IMigrationProcessor Processor => _runner.Processor;

        public IMigrationInformationLoader MigrationLoader => _runner.MigrationLoader;

        public void Up(IMigration migration)
        {
            ExecuteWithLock(() => _runner.Up(migration));
        }

        public void Down(IMigration migration)
        {
            ExecuteWithLock(() => _runner.Down(migration));
        }

        public void MigrateUp()
        {
            ExecuteWithLock(_runner.MigrateUp);
        }

        public void MigrateUp(long version)
        {
            ExecuteWithLock(() => _runner.MigrateUp(version));
        }

        public void Rollback(int steps)
        {
            ExecuteWithLock(() => _runner.Rollback(steps));
        }

        public void RollbackToVersion(long version)
        {
            ExecuteWithLock(() => _runner.RollbackToVersion(version));
        }

        public void MigrateDown(long version)
        {
            ExecuteWithLock(() => _runner.MigrateDown(version));
        }

        public void ValidateVersionOrder()
        {
            ExecuteWithLock(_runner.ValidateVersionOrder);
        }

        public void ListMigrations()
        {
            ExecuteWithLock(_runner.ListMigrations);
        }

        public bool HasMigrationsToApplyUp(long? version = null)
        {
            return ExecuteWithLock(() => _runner.HasMigrationsToApplyUp(version));
        }

        public bool HasMigrationsToApplyDown(long version)
        {
            return ExecuteWithLock(() => _runner.HasMigrationsToApplyDown(version));
        }

        public bool HasMigrationsToApplyRollback()
        {
            return ExecuteWithLock(_runner.HasMigrationsToApplyRollback);
        }

        public bool LoadVersionInfoIfRequired()
        {
            return ExecuteWithLock(_runner.LoadVersionInfoIfRequired);
        }

        public IMigrationScope BeginScope()
        {
            if (_lockDepth.Value != 0)
            {
                return _runner.BeginScope();
            }

            var applicationLock = _applicationLocker.AcquireLock();
            ++_lockDepth.Value;
            try
            {
                return new ApplicationLockingMigrationScope(
                    _runner.BeginScope(),
                    applicationLock,
                    () => --_lockDepth.Value);
            }
            catch
            {
                --_lockDepth.Value;
                applicationLock.Dispose();
                throw;
            }
        }

        private void ExecuteWithLock(Action action)
        {
            ExecuteWithLock(
                () =>
                {
                    action();
                    return true;
                });
        }

        private T ExecuteWithLock<T>(Func<T> action)
        {
            if (_lockDepth.Value != 0)
            {
                return action();
            }

            using (_applicationLocker.AcquireLock())
            {
                ++_lockDepth.Value;
                try
                {
                    return action();
                }
                finally
                {
                    --_lockDepth.Value;
                }
            }
        }

        private sealed class ApplicationLockingMigrationScope : IMigrationScope
        {
            private readonly IMigrationScope _scope;
            private readonly IDisposable _applicationLock;
            private readonly Action _onDispose;
            private bool _disposed;

            public ApplicationLockingMigrationScope(
                IMigrationScope scope,
                IDisposable applicationLock,
                Action onDispose)
            {
                _scope = scope;
                _applicationLock = applicationLock;
                _onDispose = onDispose;
            }

            public bool IsActive => _scope.IsActive;

            public void Complete()
            {
                _scope.Complete();
            }

            public void Cancel()
            {
                _scope.Cancel();
            }

            public void Dispose()
            {
                if (_disposed)
                {
                    return;
                }

                _disposed = true;
                try
                {
                    _scope.Dispose();
                }
                finally
                {
                    try
                    {
                        _onDispose();
                    }
                    finally
                    {
                        _applicationLock.Dispose();
                    }
                }
            }
        }
    }
}
