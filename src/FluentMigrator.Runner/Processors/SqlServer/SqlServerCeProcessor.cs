#region License
// 
// Copyright (c) 2007-2009, Sean Chambers <schambers80@gmail.com>
// Copyright (c) 2010, Nathan Brown
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
using System.IO;
using FluentMigrator.Builders.Execute;

namespace FluentMigrator.Runner.Processors.SqlServer
{
    public sealed class SqlServerCeProcessor : ProcessorBase
    {
        private readonly IDbFactory factory;
        private readonly IDbConnection connection;
        private IDbTransaction transaction;

        public override string DatabaseType
        {
            get { return "SqlServerCe"; }
        }

        public SqlServerCeProcessor(IDbConnection connection, IMigrationGenerator generator, IAnnouncer announcer, IMigrationProcessorOptions options, IDbFactory factory)
            : base(generator, announcer, options)
        {
            this.factory = factory;
            this.connection = connection;
            connection.Open();
            BeginTransaction();
        }

        public override bool SchemaExists(string schemaName)
        {
            return Exists("SELECT * FROM SYS.SCHEMAS WHERE NAME = '{0}'", FormatSqlEscape(schemaName));
        }

        public override bool TableExists(string schemaName, string tableName)
        {
            return Exists("SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{0}'", FormatSqlEscape(tableName));
        }

        public override bool ColumnExists(string schemaName, string tableName, string columnName)
        {
            return Exists("SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{0}' AND COLUMN_NAME = '{1}'", FormatSqlEscape(tableName), FormatSqlEscape(columnName));
        }

        public override bool ConstraintExists(string schemaName, string tableName, string constraintName)
        {
            return Exists("SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE CONSTRAINT_CATALOG = DB_NAME() AND TABLE_NAME = '{0}' AND CONSTRAINT_NAME = '{1}'", FormatSqlEscape(tableName), FormatSqlEscape(constraintName));
        }

        public override bool IndexExists(string schemaName, string tableName, string indexName)
        {
            return Exists("SELECT NULL FROM sysindexes WHERE name = '{0}'", FormatSqlEscape(indexName));
        }

        public override void Execute(string template, params object[] args)
        {
            Process(String.Format(template, args));
        }

        public override bool Exists(string template, params object[] args)
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();

            using (var command = factory.CreateCommand(String.Format(template, args), connection, transaction))
            using (var reader = command.ExecuteReader())
            {
                return reader.Read();
            }
        }

        public override DataSet ReadTableData(string schemaName, string tableName)
        {
            return Read("SELECT * FROM [{0}]", tableName);
        }

        public override DataSet Read(string template, params object[] args)
        {
            if (connection.State != ConnectionState.Open) connection.Open();

            var ds = new DataSet();
            using (var command = factory.CreateCommand(String.Format(template, args), connection, transaction))
            {
                var adapter = factory.CreateDataAdapter(command);
                adapter.Fill(ds);
                return ds;
            }
        }

        public override void BeginTransaction()
        {
            Announcer.Say("Beginning Transaction");
            transaction = connection.BeginTransaction();
        }

        public override void CommitTransaction()
        {
            Announcer.Say("Committing Transaction");

            if (transaction != null)
            {
                transaction.Commit();
                transaction = null;
            }

            if (connection.State != ConnectionState.Closed)
            {
                connection.Close();
            }
        }

        public override void RollbackTransaction()
        {
            if (transaction == null)
            {
                Announcer.Say("No transaction was available to rollback!");
                return;
            }

            Announcer.Say("Rolling back transaction");

            transaction.Rollback();

            if (connection.State != ConnectionState.Closed)
            {
                connection.Close();
            }
        }

        protected override void Process(string sql)
        {
            Announcer.Sql(sql);

            if (Options.PreviewOnly || string.IsNullOrEmpty(sql))
                return;

            if (connection.State != ConnectionState.Open)
                connection.Open();

            if (transaction == null)
                BeginTransaction();

            using (var command = factory.CreateCommand(sql, connection, transaction))
            {
                try
                {
                    command.CommandTimeout = 0; // SQL Server CE does not support non-zero command timeout values!! :/
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    using (var message = new StringWriter())
                    {
                        message.WriteLine("An error occurred executing the following sql:");
                        message.WriteLine(sql);
                        message.WriteLine("The error was {0}", ex.Message);

                        throw new Exception(message.ToString(), ex);
                    }
                }
            }
        }

        public override void Process(PerformDBOperationExpression expression)
        {
            if (connection.State != ConnectionState.Open) connection.Open();

            if (expression.Operation != null)
                expression.Operation(connection, transaction);
        }

        private static string FormatSqlEscape(string sql)
        {
            return sql.Replace("'", "''");
        }
    }
}