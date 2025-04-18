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
#if NETFRAMEWORK
using System.Data.OleDb;
#endif
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

#if NETFRAMEWORK
        public static readonly DatabaseConfiguration Jet = CreateJetConfiguration();
        public static readonly DatabaseConfiguration Jet32 = CreateJet32Configuration();
        public static readonly DatabaseConfiguration Jet64 = CreateJet64Configuration();
#endif
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

#if NETFRAMEWORK
        private static DatabaseConfiguration CreateJetConfiguration()
        {
            if (Environment.Is64BitProcess)
                return CreateJet64Configuration();
            return CreateJet32Configuration();
        }

        private static DatabaseConfiguration CreateJet32Configuration()
        {
            // Configure the DB connection
            var dbFileName = Path.Combine(AppContext.BaseDirectory, "test.mdb");
            var csb = new OleDbConnectionStringBuilder
            {
                DataSource = dbFileName,
                Provider = "Microsoft.Jet.OLEDB.4.0",
            };

            var connectionString = csb.ConnectionString;
            EnsureMdbExists(dbFileName, connectionString);

            return new DatabaseConfiguration()
            {
                ProcessorId = "jet",
                ConnectionString = connectionString,
            };
        }

        private static DatabaseConfiguration CreateJet64Configuration()
        {
            // Configure the DB connection
            var dbFileName = Path.Combine(AppContext.BaseDirectory, "test.mdb");
            var csb = new OleDbConnectionStringBuilder
            {
                DataSource = dbFileName,
                Provider = "Microsoft.ACE.OLEDB.12.0",
            };

            var connectionString = csb.ConnectionString;
            EnsureMdbExists(dbFileName, connectionString);

            return new DatabaseConfiguration()
            {
                ProcessorId = "jet",
                ConnectionString = connectionString,
            };
        }

        private static void EnsureMdbExists(string dbFileName, string connectionString)
        {
            if (File.Exists(dbFileName))
                return;

            var catalog = new ADOX.Catalog();
            catalog.Create(connectionString);
        }
#endif
        }
}
