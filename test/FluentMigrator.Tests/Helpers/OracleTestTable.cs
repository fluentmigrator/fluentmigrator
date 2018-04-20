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
using System.Collections.Generic;
using System.Data;
using System.Text;

using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Generators.Oracle;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Tests.Unit;

namespace FluentMigrator.Tests.Helpers
{
    public class OracleTestTable : IDisposable
    {
        private readonly IQuoter _quoter = new OracleQuoterQuotedIdentifier();

        private IMigrationProcessorOptions Options { get; set; }
        private IDbConnection Connection { get; set; }
        private IDbTransaction Transaction { get;set; }
        private IDbFactory Factory { get; set; }
        private string _schema;
        private List<string> constraints = new List<string>();
        private List<string> indexies = new List<string>();
        public string Name { get; set; }


        public OracleTestTable(GenericProcessorBase processor, string schema, params string[] columnDefinitions)
        {
            Options = new TestMigrationProcessorOptions();
            Connection = processor.Connection;
            Transaction = processor.Transaction;
            Factory = processor.Factory;
            _schema = schema;

            if (Connection.State != ConnectionState.Open)
                Connection.Open();

            Name = "TestTable";
            Create(columnDefinitions);
        }

        public OracleTestTable(string table, GenericProcessorBase processor, string schema, params string[] columnDefinitions)
        {
            Connection = processor.Connection;
            Transaction = processor.Transaction;
            Factory = processor.Factory;
            _schema = schema;

            if (Connection.State != ConnectionState.Open)
                Connection.Open();

            Name = table;
            Create(columnDefinitions);
        }

        public void Dispose()
        {
            Drop();
        }

        public void Create(IEnumerable<string> columnDefinitions)
        {
            var sb = CreateSchemaQuery();

            sb.Append("CREATE TABLE ");
            sb.Append(_quoter.QuoteTableName(Name));

            foreach (string definition in columnDefinitions)
            {
                sb.Append("(");
                sb.Append(definition);
                sb.Append("), ");
            }

            sb.Remove(sb.Length - 2, 2);

            using (var command = Factory.CreateCommand(sb.ToString(), Connection, Transaction, Options))
                command.ExecuteNonQuery();
        }

        private StringBuilder CreateSchemaQuery()
        {
            var sb = new StringBuilder();

            if (!string.IsNullOrEmpty(_schema))
            {
                sb.Append(string.Format("CREATE SCHEMA AUTHORIZATION {0} ", _schema));
            }
            return sb;
        }

        public void WithUniqueConstraintOn(string column)
        {
            WithUniqueConstraintOn(column, "UC_" + column);
        }

        public void WithUniqueConstraintOn(string column, string name)
        {
            var sb = new StringBuilder();
            sb.Append(string.Format("ALTER TABLE {0} ADD CONSTRAINT {1} UNIQUE ({2})", _quoter.QuoteTableName(Name), _quoter.QuoteConstraintName(name), _quoter.QuoteColumnName(column)));
            using (var command = Factory.CreateCommand(sb.ToString(), Connection, Transaction, Options))
                command.ExecuteNonQuery();
            constraints.Add(name);
       }

        public void WithIndexOn(string column)
        {
            WithIndexOn(column, "UI_" + column);
        }

        public void WithIndexOn(string column, string name)
        {
            var sb = new StringBuilder();
            sb.Append(string.Format("CREATE UNIQUE INDEX {0} ON {1} ({2})", _quoter.QuoteIndexName(name), _quoter.QuoteTableName(Name), _quoter.QuoteColumnName(column)));
            using (var command = Factory.CreateCommand(sb.ToString(), Connection, Transaction, Options))
                command.ExecuteNonQuery();
            indexies.Add(name);
        }

        public void Drop()
        {
            foreach(var constraint in constraints)
            {
                using (var command = Factory.CreateCommand(string.Format( "ALTER TABLE {0} DROP CONSTRAINT {1}", _quoter.QuoteTableName(Name), _quoter.QuoteConstraintName(constraint) ), Connection, Transaction, Options))
                    command.ExecuteNonQuery();
            }

            foreach (var index in indexies)
            {
                using (var command = Factory.CreateCommand(string.Format( "DROP INDEX {0}", _quoter.QuoteIndexName( index ) ), Connection, Transaction, Options))
                    command.ExecuteNonQuery();
            }

            using (var command = Factory.CreateCommand("DROP TABLE " + _quoter.QuoteTableName(Name), Connection, Transaction, Options))
                command.ExecuteNonQuery();
        }
    }
}
