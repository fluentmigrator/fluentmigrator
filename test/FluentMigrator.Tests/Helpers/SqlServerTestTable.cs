#region License
//
// Copyright (c) 2007-2024, Fluent Migrator Project
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
using System.Collections.Generic;
using System.Text;

using FluentMigrator.Generation;
using FluentMigrator.Runner.Processors.SqlServer;

using Microsoft.Data.SqlClient;

namespace FluentMigrator.Tests.Helpers
{
    public class SqlServerTestTable : IDisposable
    {
        private readonly IQuoter _quoter;
        private readonly string _schemaName;
        private SqlConnection Connection { get; set; }
        private List<string> indexies = new List<string>();
        public string Name { get; set; }
        private SqlTransaction Transaction { get; set; }

        public SqlServerTestTable(SqlServerProcessor processor, string schemaName, params string[] columnDefinitions)
        {
            _schemaName = schemaName;
            Connection = (SqlConnection)processor.Connection;
            Transaction = (SqlTransaction)processor.Transaction;
            _quoter = processor.Quoter;

            Name = "TestTable";
            Create(processor, columnDefinitions);
        }
        public SqlServerTestTable(string table, SqlServerProcessor processor, string schemaName, params string[] columnDefinitions)
        {
            _schemaName = schemaName;
            Connection = (SqlConnection)processor.Connection;
            Transaction = (SqlTransaction)processor.Transaction;
            _quoter = processor.Quoter;

            Name = table;

            Create(processor, columnDefinitions);
        }

        public void Dispose()
        {
            Drop();
        }

        public void Create(SqlServerProcessor processor, IEnumerable<string> columnDefinitions)
        {
            if (!string.IsNullOrEmpty(_schemaName) && !processor.SchemaExists(_schemaName))
            {
                using (var command = new SqlCommand(string.Format("CREATE SCHEMA {0}", _quoter.QuoteSchemaName(_schemaName)), Connection, Transaction))
                    command.ExecuteNonQuery();
            }

            var quotedObjectName = _quoter.QuoteTableName(Name, _schemaName);

            var sb = new StringBuilder();
            sb.AppendFormat("CREATE TABLE ");
            sb.Append(quotedObjectName);

            sb.Append("(");
            foreach (string definition in columnDefinitions)
            {
                sb.Append(definition);
                sb.Append(", ");
            }

            sb.Remove(sb.Length - 2, 2);
            sb.Append(")");

            using (var command = new SqlCommand(sb.ToString(), Connection, Transaction))
                command.ExecuteNonQuery();
        }

        public void Drop()
        {
            var quotedSchema = _quoter.QuoteSchemaName(_schemaName);

            var quotedObjectName = _quoter.QuoteTableName(Name, _schemaName);

            foreach (var quoteIndexName in indexies)
            {
                using (var command = new SqlCommand(string.Format("DROP INDEX {0} ON {1}", quoteIndexName, quotedObjectName), Connection, Transaction))
                    command.ExecuteNonQuery();
            }

            using (var command = new SqlCommand("DROP TABLE " + quotedObjectName, Connection, Transaction))
                command.ExecuteNonQuery();

            if (!string.IsNullOrEmpty(_schemaName))
            {
                using (var command = new SqlCommand(string.Format("DROP SCHEMA {0}", quotedSchema), Connection, Transaction))
                    command.ExecuteNonQuery();
            }
        }

        public string WithIndexOn(string column)
        {
            var indexName = string.Format("idx_{0}", column);

            var quotedObjectName = _quoter.QuoteTableName(Name, _schemaName);

            var quotedIndexName = _quoter.QuoteIndexName(indexName);

            indexies.Add(quotedIndexName);

            using (var command = new SqlCommand(string.Format("CREATE INDEX {0} ON {1} ({2})", quotedIndexName, quotedObjectName, _quoter.QuoteColumnName(column)), Connection, Transaction))
                command.ExecuteNonQuery();

            return indexName;
        }

        public void WithDefaultValueOn(string column)
        {
            var defaultConstraintName = string.Format("[DF_{0}_{1}]", Name, column);
            const int defaultValue = 1;
            using (var command = new SqlCommand(string.Format(" ALTER TABLE {0} ADD CONSTRAINT {1} DEFAULT ({2}) FOR {3}", _quoter.QuoteTableName(Name, _schemaName), defaultConstraintName, defaultValue, _quoter.QuoteColumnName(column)), Connection, Transaction))
                command.ExecuteNonQuery();
        }
    }
}
