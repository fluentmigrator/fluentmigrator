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
using System.Text;
using System.Collections.Generic;

namespace FluentMigrator.Runner.Processors.SqlServer
{
    public sealed class SqlServerCeProcessor : GenericProcessorBase
    {
        public override string DatabaseType
        {
            get { return "SqlServerCe"; }
        }

        public override bool SupportsTransactions
        {
            get
            {
                return true;
            }
        }

        public SqlServerCeProcessor(IDbConnection connection, IMigrationGenerator generator, IAnnouncer announcer, IMigrationProcessorOptions options, IDbFactory factory)
            : base(connection, factory, generator, announcer, options)
        {
        }

        public override bool SchemaExists(string schemaName)
        {
            return true; // SqlServerCe has no schemas
        }

        public override bool TableExists(string schemaName, string tableName)
        {
            return Exists("SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{0}'", FormatHelper.FormatSqlEscape(tableName));
        }

        public override bool ColumnExists(string schemaName, string tableName, string columnName)
        {
            return Exists("SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{0}' AND COLUMN_NAME = '{1}'",
                FormatHelper.FormatSqlEscape(tableName), FormatHelper.FormatSqlEscape(columnName));
        }

        public override bool ConstraintExists(string schemaName, string tableName, string constraintName)
        {
            return Exists("SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE TABLE_NAME = '{0}' AND CONSTRAINT_NAME = '{1}'",
                FormatHelper.FormatSqlEscape(tableName), FormatHelper.FormatSqlEscape(constraintName));
        }

        public override bool IndexExists(string schemaName, string tableName, string indexName)
        {
            return Exists("SELECT NULL FROM INFORMATION_SCHEMA.INDEXES WHERE INDEX_NAME = '{0}'", FormatHelper.FormatSqlEscape(indexName));
        }

        public override bool SequenceExists(string schemaName, string sequenceName)
        {
            return false;
        }

        public override bool DefaultValueExists(string schemaName, string tableName, string columnName, object defaultValue)
        {
            return false;
        }

        public override void Execute(string template, params object[] args)
        {
            Process(String.Format(template, args));
        }

        public override bool Exists(string template, params object[] args)
        {
            EnsureConnectionIsOpen();

            using (var command = Factory.CreateCommand(String.Format(template, args), Connection, Transaction))
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

            using (var command = Factory.CreateCommand("", Connection, Transaction))
            {
                foreach (string statement in SplitIntoSingleStatements(sql))
                {
                    try
                    {
                        command.CommandText = statement;
                        command.CommandTimeout = 0; // SQL Server CE does not support non-zero command timeout values!! :/
                        command.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        using (var message = new StringWriter())
                        {
                            message.WriteLine("An error occurred executing the following sql:");
                            message.WriteLine(statement);
                            message.WriteLine("The error was {0}", ex.Message);

                            throw new Exception(message.ToString(), ex);
                        }
                    }
                }
            }
        }

        private IEnumerable<string> SplitIntoSingleStatements(string sql)
        {
            StringBuilder builder = null;
            foreach (string line in sql.Split(new string[] { Environment.NewLine }, StringSplitOptions.None))
            {
                if (!string.IsNullOrEmpty(line.Trim()) && !(line.TrimStart().StartsWith("--")) && (!line.ToUpper().Equals("GO")))
                {
                    if (builder == null)
                    {
                        builder = new StringBuilder();
                    }
                    builder.AppendLine(line);

                    if (line.TrimEnd().EndsWith(";"))
                    {
                        yield return builder.ToString();
                        builder = null;
                    }
                }
            }
            if (builder != null)
            {
                yield return builder.ToString();
            }
        }

        public override void Process(PerformDBOperationExpression expression)
        {
            EnsureConnectionIsOpen();

            if (expression.Operation != null)
                expression.Operation(Connection, Transaction);
        }
    }
}
