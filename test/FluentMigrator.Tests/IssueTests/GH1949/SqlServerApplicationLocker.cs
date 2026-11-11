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
using System.Data;
using System.Threading;
using System.Threading.Tasks;

using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace FluentMigrator.Tests.IssueTests.GH1949
{
    /// <summary>
    /// The <c>sp_getapplock</c> locker from the multi-server deployments FAQ, with a tracker that records how
    /// many instances believe they hold the lock at once.
    /// </summary>
    public sealed class SqlServerApplicationLocker : IApplicationLocker
    {
        private readonly string _connectionString;
        private readonly string _resource;
        private readonly LockHolderTracker _tracker;

        public SqlServerApplicationLocker(
            IConnectionStringAccessor connectionStringAccessor,
            IOptions<TestMigrationOptions> options,
            LockHolderTracker tracker)
        {
            _connectionString = connectionStringAccessor.ConnectionString;
            _resource = options.Value.LockName;
            _tracker = tracker;
        }

        public IDisposable AcquireLock(CancellationToken cancellationToken)
        {
            var connection = new SqlConnection(_connectionString);
            try
            {
                connection.Open();
                using var command = CreateGetAppLock(connection);
                do
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    command.ExecuteNonQuery();
                }
                while (!IsGranted(command));

                _tracker.Enter();
                return new Handle(connection, _resource, _tracker);
            }
            catch
            {
                connection.Dispose();
                throw;
            }
        }

        public async ValueTask<IAsyncDisposable> AcquireLockAsync(CancellationToken cancellationToken)
        {
            var connection = new SqlConnection(_connectionString);
            try
            {
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                using var command = CreateGetAppLock(connection);
                do
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
                while (!IsGranted(command));

                _tracker.Enter();
                return new Handle(connection, _resource, _tracker);
            }
            catch
            {
                connection.Dispose();
                throw;
            }
        }

        private SqlCommand CreateGetAppLock(SqlConnection connection)
        {
            var command = new SqlCommand("sp_getapplock", connection) { CommandType = CommandType.StoredProcedure };
            command.Parameters.AddWithValue("@Resource", _resource);
            command.Parameters.AddWithValue("@LockMode", "Exclusive");
            command.Parameters.AddWithValue("@LockOwner", "Session");
            command.Parameters.AddWithValue("@LockTimeout", 1000);
            command.Parameters.Add(new SqlParameter("@Result", SqlDbType.Int) { Direction = ParameterDirection.ReturnValue });
            return command;
        }

        private static bool IsGranted(SqlCommand command)
        {
            var result = (int) command.Parameters["@Result"].Value;
            if (result == -1)
            {
                return false;
            }

            if (result < 0)
            {
                throw new InvalidOperationException($"sp_getapplock returned {result}.");
            }

            return true;
        }

        private sealed class Handle : IDisposable, IAsyncDisposable
        {
            private readonly string _resource;
            private readonly LockHolderTracker _tracker;
            private SqlConnection _connection;

            public Handle(SqlConnection connection, string resource, LockHolderTracker tracker)
            {
                _connection = connection;
                _resource = resource;
                _tracker = tracker;
            }

            public void Dispose()
            {
                var connection = Interlocked.Exchange(ref _connection, null);
                if (connection == null)
                {
                    return;
                }

                // Leave the tracker while the database lock is still held, so the count never overlaps.
                _tracker.Leave();

                using (connection)
                using (var release = new SqlCommand("sp_releaseapplock", connection) { CommandType = CommandType.StoredProcedure })
                {
                    release.Parameters.AddWithValue("@Resource", _resource);
                    release.Parameters.AddWithValue("@LockOwner", "Session");
                    release.ExecuteNonQuery();
                }
            }

            public ValueTask DisposeAsync()
            {
                Dispose();
                return default;
            }
        }
    }

    /// <summary>
    /// Counts the instances inside the lock, shared by every service provider in one test.
    /// </summary>
    public sealed class LockHolderTracker
    {
        private int _holders;
        private int _maxHolders;
        private int _acquisitions;

        public int MaxHolders => Volatile.Read(ref _maxHolders);

        public int Acquisitions => Volatile.Read(ref _acquisitions);

        public void Enter()
        {
            Interlocked.Increment(ref _acquisitions);
            var holders = Interlocked.Increment(ref _holders);
            int max;
            while (holders > (max = Volatile.Read(ref _maxHolders))
                && Interlocked.CompareExchange(ref _maxHolders, holders, max) != max)
            {
            }
        }

        public void Leave() => Interlocked.Decrement(ref _holders);
    }
}
