#region License
// Copyright (c) 2007-2018, Sean Chambers <schambers80@gmail.com>
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
using FluentMigrator.Runner.Processors.SqlServer;

using Microsoft.Data.SqlClient;

namespace FluentMigrator.Tests.Helpers
{
    public class SqlServerTestSequence : IDisposable
    {
        private readonly string _schemaName;

        private SqlConnection Connection { get; set; }

        public string Name { get; set; }

        private SqlTransaction Transaction { get; set; }

        public SqlServerTestSequence(SqlServerProcessor processor, string schemaName, string sequenceName)
        {
            _schemaName = schemaName;
            Name = sequenceName;

            Connection = (SqlConnection)processor.Connection;
            Transaction = (SqlTransaction)processor.Transaction;
            Create();
        }

        public void Dispose()
        {
            Drop();
        }

        public void Create()
        {
            if (!string.IsNullOrEmpty(_schemaName))
            {
                using (var command = new SqlCommand(string.Format("CREATE SCHEMA [{0}]", _schemaName), Connection, Transaction))
                    command.ExecuteNonQuery();
            }

            var schema = string.IsNullOrEmpty(_schemaName) ? "dbo" : _schemaName;

            string createCommand = string.Format("CREATE SEQUENCE [{0}].[{1}] INCREMENT BY 2 MINVALUE 0 MAXVALUE 100 START WITH 2 CACHE 10 CYCLE", schema, Name);
            using (var command = new SqlCommand(createCommand, Connection, Transaction))
                command.ExecuteNonQuery();
        }

        public void Drop()
        {
            if (string.IsNullOrEmpty(_schemaName))
            {
                using (var command = new SqlCommand("DROP SEQUENCE " + Name, Connection, Transaction))
                    command.ExecuteNonQuery();
            }
            else
            {
                using (var command = new SqlCommand(string.Format("DROP SEQUENCE [{0}].{1}", _schemaName, Name), Connection, Transaction))
                    command.ExecuteNonQuery();

                using (var command = new SqlCommand(string.Format("DROP SCHEMA [{0}]", _schemaName), Connection, Transaction))
                    command.ExecuteNonQuery();
            }
        }
    }
}
