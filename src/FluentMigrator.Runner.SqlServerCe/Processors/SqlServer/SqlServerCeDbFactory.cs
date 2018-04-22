#region License
//
// Copyright (c) 2018, Fluent Migrator Project
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

using System;
using System.Data;

namespace FluentMigrator.Runner.Processors.SqlServer
{
    public class SqlServerCeDbFactory : ReflectionBasedDbFactory
    {
        private static readonly TestEntry[] _entries =
        {
            new TestEntry("System.Data.SqlServerCe", "System.Data.SqlServerCe.SqlCeProviderFactory"),
        };

        [Obsolete]
        public SqlServerCeDbFactory()
            : this(null)
        {
        }

        public SqlServerCeDbFactory(IServiceProvider serviceProvider)
            : base(serviceProvider, _entries)
        {
        }

        [Obsolete]
        public override IDbCommand CreateCommand(string commandText, IDbConnection connection, IDbTransaction transaction, IMigrationProcessorOptions options)
        {
            var command = connection.CreateCommand();
            command.CommandText = commandText;
            // SQL Server CE does not support non-zero command timeout values!! :/
            if (transaction != null) command.Transaction = transaction;
            return command;
        }
    }
}
