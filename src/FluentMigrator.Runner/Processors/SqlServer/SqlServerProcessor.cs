using FluentMigrator.Runner.Helpers;

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
    public sealed class SqlServerProcessor : GenericProcessorBase
    {
        private const string SCHEMA_EXISTS = "SELECT 1 WHERE EXISTS (SELECT * FROM sys.schemas WHERE NAME = '{0}') ";
        private const string TABLE_EXISTS = "SELECT 1 WHERE EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = '{0}' AND TABLE_NAME = '{1}')";
        private const string COLUMN_EXISTS = "SELECT 1 WHERE EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = '{0}' AND TABLE_NAME = '{1}' AND COLUMN_NAME = '{2}')";
        private const string CONSTRAINT_EXISTS = "SELECT 1 WHERE EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE CONSTRAINT_CATALOG = DB_NAME() AND TABLE_SCHEMA = '{0}' AND TABLE_NAME = '{1}' AND CONSTRAINT_NAME = '{2}')";
        private const string INDEX_EXISTS = "SELECT 1 WHERE EXISTS (SELECT * FROM sys.indexes WHERE name = '{0}' and object_id=OBJECT_ID('{1}.{2}'))";
        private const string SEQUENCES_EXISTS = "SELECT 1 WHERE EXISTS (SELECT * FROM INFORMATION_SCHEMA.SEQUENCES WHERE SEQUENCE_SCHEMA = '{0}' AND SEQUENCE_NAME = '{1}' )";
        private const string DEFAULTVALUE_EXISTS = "SELECT 1 WHERE EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = '{0}' AND TABLE_NAME = '{1}' AND COLUMN_NAME = '{2}' AND COLUMN_DEFAULT LIKE '{3}')";

        public override string DatabaseType
        {
            get { return "SqlServer"; }
        }

        public override bool SupportsTransactions
        {
            get
            {
                return true;
            }
        }

        public SqlServerProcessor(IDbConnection connection, IMigrationGenerator generator, IAnnouncer announcer, IMigrationProcessorOptions options, IDbFactory factory)
            : base(connection, factory, generator, announcer, options)
        {
        }

        private static string SafeSchemaName(string schemaName)
        {
            return string.IsNullOrEmpty(schemaName) ? "dbo" : FormatHelper.FormatSqlEscape(schemaName);
        }

        public override bool SchemaExists(string schemaName)
        {
            return Exists(SCHEMA_EXISTS, SafeSchemaName(schemaName));
        }

        public override bool TableExists(string schemaName, string tableName)
        {
            try
            {
                return Exists(TABLE_EXISTS, SafeSchemaName(schemaName),
                    FormatHelper.FormatSqlEscape(tableName));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return false;
        }

        public override bool ColumnExists(string schemaName, string tableName, string columnName)
        {
            return Exists(COLUMN_EXISTS, SafeSchemaName(schemaName),
                FormatHelper.FormatSqlEscape(tableName), FormatHelper.FormatSqlEscape(columnName));
        }

        public override bool ConstraintExists(string schemaName, string tableName, string constraintName)
        {
            return Exists(CONSTRAINT_EXISTS, SafeSchemaName(schemaName),
                FormatHelper.FormatSqlEscape(tableName), FormatHelper.FormatSqlEscape(constraintName));
        }

        public override bool IndexExists(string schemaName, string tableName, string indexName)
        {
            return Exists(INDEX_EXISTS,
                FormatHelper.FormatSqlEscape(indexName), SafeSchemaName(schemaName), FormatHelper.FormatSqlEscape(tableName));
        }

        public override bool SequenceExists(string schemaName, string sequenceName)
        {
            return Exists(SEQUENCES_EXISTS, SafeSchemaName(schemaName),
                FormatHelper.FormatSqlEscape(sequenceName));
        }

        public override bool DefaultValueExists(string schemaName, string tableName, string columnName, object defaultValue)
        {
            string defaultValueAsString = string.Format("%{0}%", FormatHelper.FormatSqlEscape(defaultValue.ToString()));
            return Exists(DEFAULTVALUE_EXISTS, SafeSchemaName(schemaName),
                FormatHelper.FormatSqlEscape(tableName),
                FormatHelper.FormatSqlEscape(columnName), defaultValueAsString);
        }

        public override void Execute(string template, params object[] args)
        {
            Process(String.Format(template, args));
        }

        public override bool Exists(string template, params object[] args)
        {
            EnsureConnectionIsOpen();

            using (var command = Factory.CreateCommand(String.Format(template, args), Connection, Transaction))
            {
                var result = command.ExecuteScalar();
                return DBNull.Value != result && Convert.ToInt32(result) == 1;
            }
        }

        public override DataSet ReadTableData(string schemaName, string tableName)
        {
            return Read("SELECT * FROM [{0}].[{1}]", SafeSchemaName(schemaName), tableName);
        }

        public override DataSet Read(string template, params object[] args)
        {
            EnsureConnectionIsOpen();

            var ds = new DataSet();
            using (var command = Factory.CreateCommand(String.Format(template, args), Connection, Transaction))
            {
                var adapter = Factory.CreateDataAdapter(command);
                adapter.Fill(ds);
                return ds;
            }
        }

        protected override void Process(string sql)
        {
            Announcer.Sql(sql);

            if (Options.PreviewOnly || string.IsNullOrEmpty(sql))
                return;

            EnsureConnectionIsOpen();

            if (sql.IndexOf("GO", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                ExecuteBatchNonQuery(sql);

            }
            else
            {
                ExecuteNonQuery(sql);
            }
        }

        private void ExecuteNonQuery(string sql)
        {
            using (var command = Factory.CreateCommand(sql, Connection, Transaction))
            {
                try
                {
                    command.CommandTimeout = Options.Timeout;
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    using (var message = new StringWriter())
                    {
                        message.WriteLine("An error occured executing the following sql:");
                        message.WriteLine(sql);
                        message.WriteLine("The error was {0}", ex.Message);

                        throw new Exception(message.ToString(), ex);
                    }
                }
            }
        }

        private void ExecuteBatchNonQuery(string sql)
        {
            sql += "\nGO";   // make sure last batch is executed.
            string sqlBatch = string.Empty;

            using (var command = Factory.CreateCommand(string.Empty, Connection, Transaction))
            {
                try
                {
                    foreach (string line in sql.Split(new[] { "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (line.ToUpperInvariant().Trim() == "GO")
                        {
                            if (!string.IsNullOrEmpty(sqlBatch))
                            {
                                command.CommandText = sqlBatch;
                                command.CommandTimeout = Options.Timeout;
                                command.ExecuteNonQuery();
                                sqlBatch = string.Empty;
                            }
                        }
                        else
                        {
                            sqlBatch += line + "\n";
                        }
                    }
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
            Announcer.Say("Performing DB Operation");

            if (Options.PreviewOnly)
                return;

            EnsureConnectionIsOpen();

            if (expression.Operation != null)
                expression.Operation(Connection, Transaction);
        }
    }
}
