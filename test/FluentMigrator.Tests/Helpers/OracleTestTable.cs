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

using FluentMigrator.Generation;
using FluentMigrator.Runner.Generators.Oracle;
using FluentMigrator.Runner.Processors;

namespace FluentMigrator.Tests.Helpers
{
    public class OracleTestTable : IDisposable
    {
        private readonly IQuoter _quoter = new OracleQuoterQuotedIdentifier();
        private readonly GenericProcessorBase _processor;
        private readonly string _schema;
        private readonly List<string> _constraints = new List<string>();
        private readonly List<string> _indexies = new List<string>();

        private IDbConnection Connection => _processor.Connection;
        public string Name { get; set; }


        public OracleTestTable(GenericProcessorBase processor, string schema, params string[] columnDefinitions)
        {
            _processor = processor;
            _schema = schema;

            if (Connection.State != ConnectionState.Open)
                Connection.Open();

            Name = "TestTable";
            Create(columnDefinitions);
        }

        public OracleTestTable(string table, GenericProcessorBase processor, string schema, params string[] columnDefinitions)
        {
            _processor = processor;
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

            _processor.Execute(sb.ToString());
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
            _processor.Execute(sb.ToString());
            _constraints.Add(name);
       }

        public void WithIndexOn(string column)
        {
            WithIndexOn(column, "UI_" + column);
        }

        public void WithIndexOn(string column, string name)
        {
            var sb = new StringBuilder();
            sb.Append(string.Format("CREATE UNIQUE INDEX {0} ON {1} ({2})", _quoter.QuoteIndexName(name), _quoter.QuoteTableName(Name), _quoter.QuoteColumnName(column)));
            _processor.Execute(sb.ToString());
            _indexies.Add(name);
        }

        public void Drop()
        {
            foreach(var constraint in _constraints)
            {
                var cmd = string.Format(
                    "ALTER TABLE {0} DROP CONSTRAINT {1}",
                    _quoter.QuoteTableName(Name),
                    _quoter.QuoteConstraintName(constraint));
                _processor.Execute(cmd);
            }

            foreach (var index in _indexies)
            {
                var cmd = string.Format("DROP INDEX {0}", _quoter.QuoteIndexName(index));
                _processor.Execute(cmd);
            }

            var dropSql = "DROP TABLE " + _quoter.QuoteTableName(Name);
            _processor.Execute(dropSql);
        }
    }
}
