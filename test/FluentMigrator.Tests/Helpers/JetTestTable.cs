#region License
// Copyright (c) 2007-2024, Fluent Migrator Project
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
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Text;
using FluentMigrator.Runner.Generators.Jet;
using FluentMigrator.Runner.Processors.Jet;

namespace FluentMigrator.Tests.Helpers
{
    public class JetTestTable : IDisposable
    {
        private readonly JetQuoter _quoter = new JetQuoter();
        public OleDbConnection Connection { get; private set; }
        public string Name { get; set; }
        public OleDbTransaction Transaction { get; private set; }

        public JetTestTable(JetProcessor processor, params string[] columnDefinitions)
        {
            Name = "Table" + Guid.NewGuid().ToString("N");
            Init(processor, columnDefinitions);
        }

        public JetTestTable(string tableName, JetProcessor processor, params string[] columnDefinitions)
        {
            Name = _quoter.QuoteTableName(tableName);
            Init(processor, columnDefinitions);
        }

        private void Init(JetProcessor processor, IEnumerable<string> columnDefinitions)
        {
            Connection = processor.Connection;
            Transaction = processor.Transaction;

            var csb = new OleDbConnectionStringBuilder(Connection.ConnectionString);
            var dbFileName = HostUtilities.ReplaceDataDirectory(csb.DataSource);
            csb.DataSource = dbFileName;

            if (!File.Exists(dbFileName))
            {
                var connString = csb.ConnectionString;
                var type = Type.GetTypeFromProgID("ADOX.Catalog");
                if (type != null)
                {
                    dynamic cat = Activator.CreateInstance(type);
                    cat.Create(connString);
                }
            }

            Create(columnDefinitions);
        }

        public void Dispose()
        {
            Drop();
        }

        public void Create(IEnumerable<string> columnDefinitions)
        {
            if (Connection.State != ConnectionState.Open)
                Connection.Open();

            var sb = new StringBuilder();
            sb.Append("CREATE TABLE ");

            sb.Append(Name);

            foreach (string definition in columnDefinitions)
            {
                sb.Append("(");
                sb.Append(definition);
                sb.Append("), ");
            }

            sb.Remove(sb.Length - 2, 2);
            using (var command = new OleDbCommand(sb.ToString(), Connection, Transaction))
                command.ExecuteNonQuery();
        }

        public void Drop()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("DROP TABLE {0}", Name);

            using (var command = new OleDbCommand(sb.ToString(), Connection, Transaction))
                command.ExecuteNonQuery();
        }
    }
}
