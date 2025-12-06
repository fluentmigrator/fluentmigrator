#region License
// Copyright (c) 2018, Fluent Migrator Project
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
using System.Diagnostics.CodeAnalysis;
using System.IO;

using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;

namespace FluentMigrator.Example.Migrator
{
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Fields are used via reflection")]
    public static class DefaultDatabaseConfigurations
    {
        public static readonly DatabaseConfiguration Sqlite = CreateSqliteConfiguration();
        public static readonly DatabaseConfiguration SqlServer = CreateSqlServerConfiguration();

        private static DatabaseConfiguration CreateSqliteConfiguration()
        {
            // Configure the DB connection
            var dbFileName = Path.Combine(AppContext.BaseDirectory, "test.db");
            var csb = new SqliteConnectionStringBuilder
            {
                DataSource = dbFileName,
                Mode = SqliteOpenMode.ReadWriteCreate
            };

            return new DatabaseConfiguration
            {
                ProcessorId = ProcessorIdConstants.SQLite,
                ConnectionString = csb.ConnectionString,
            };
        }

        private static DatabaseConfiguration CreateSqlServerConfiguration()
        {
            var scsb = new SqlConnectionStringBuilder();
            scsb.DataSource = ".";
            scsb.IntegratedSecurity = true;
            scsb.InitialCatalog = "FluentMigrator";
            scsb.TrustServerCertificate = true;

            return new DatabaseConfiguration
            {
                ProcessorId = ProcessorIdConstants.SqlServer,
                ConnectionString = scsb.ToString()
            };
        }
    }
}
