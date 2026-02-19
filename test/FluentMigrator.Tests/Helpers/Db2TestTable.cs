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
using System.Text;

using FluentMigrator.Generation;
using FluentMigrator.Runner.Generators.DB2;
using FluentMigrator.Runner.Processors.DB2;

namespace FluentMigrator.Tests.Helpers
{
    public class Db2TestTable : IDisposable
    {
        private readonly IQuoter _quoter = new Db2Quoter();

        private readonly string _schema;

        public Db2TestTable(Db2Processor processor, string schema, params string[] columnDefinitions)
        {
            Processor = processor;
            _schema = schema;

            if (Connection.State != ConnectionState.Open)
                Connection.Open();

            Name = "TestTable";
            NameWithSchema = _quoter.QuoteTableName(Name, _schema);
            Create(columnDefinitions);
        }

        public Db2TestTable(string table, Db2Processor processor, string schema, params string[] columnDefinitions)
        {
            Processor = processor;
            _schema = schema;

            if (Connection.State != ConnectionState.Open)
                Connection.Open();

            Name = _quoter.UnQuote(table);
            NameWithSchema = _quoter.QuoteTableName(Name, _schema);
            Create(columnDefinitions);
        }

        public string Name
        {
            get;
        }

        public string NameWithSchema
        {
            get;
        }

        private IDbConnection Connection => Processor.Connection;

        public void Create(string[] columnDefinitions)
        {
            var sb = new StringBuilder();

            if (!string.IsNullOrEmpty(_schema))
            {
                sb.AppendFormat("CREATE SCHEMA {0};", _quoter.QuoteSchemaName(_schema));
            }

            var columns = string.Join(", ", columnDefinitions);
            sb.AppendFormat("CREATE TABLE {0} ({1})", NameWithSchema, columns);

            Processor.Execute(sb.ToString());
        }

        public void Dispose()
        {
            Drop();
        }

        public void Drop()
        {
            var tableCommand = string.Format("DROP TABLE {0}", NameWithSchema);
            Processor.Execute(tableCommand);

            if (!string.IsNullOrEmpty(_schema))
            {
                var schemaCommand = string.Format("DROP SCHEMA {0} RESTRICT", _quoter.QuoteSchemaName(_schema));
                Processor.Execute(schemaCommand);
            }
        }

        public void WithIndexOn(string column, string name)
        {
            var query = string.Format("CREATE UNIQUE INDEX {0} ON {1} ({2})",
                _quoter.QuoteIndexName(name, _schema),
                NameWithSchema,
                _quoter.QuoteColumnName(column)
                );

            Processor.Execute(query);
        }

        public void WithUniqueConstraintOn(string column, string name)
        {
            var constraintName = _quoter.QuoteConstraintName(name, _schema);

            var query = string.Format("ALTER TABLE {0} ADD CONSTRAINT {1} UNIQUE ({2})",
                NameWithSchema,
                constraintName,
                _quoter.QuoteColumnName(column)
            );

            Processor.Execute(query);
        }

        public Db2Processor Processor { get; }
    }
}
