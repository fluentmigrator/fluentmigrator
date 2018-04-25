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
using System.Data;

using FluentMigrator.Runner.Generators.Hana;
using FluentMigrator.Runner.Processors.Hana;

using Sap.Data.Hana;

namespace FluentMigrator.Tests.Helpers
{
    public class HanaTestSequence: IDisposable
    {
        private readonly HanaQuoter _quoter = new HanaQuoter();
        private readonly string _schemaName;
        private HanaConnection Connection { get; }
        public string Name { get; set; }
        public string NameWithSchema { get; set; }
        private HanaTransaction Transaction { get; }

        public HanaTestSequence(HanaProcessor processor, string schemaName, string sequenceName)
        {
            _schemaName = schemaName;
            Name = _quoter.QuoteSequenceName(sequenceName, null);

            Connection = (HanaConnection)processor.Connection;
            Transaction = (HanaTransaction)processor.Transaction;
            NameWithSchema = _quoter.QuoteSequenceName(sequenceName, schemaName);
            Create();
        }

        public void Dispose()
        {
            Drop();
        }

        public void Create()
        {
            if (Connection.State != ConnectionState.Open)
                Connection.Open();

            if (!string.IsNullOrEmpty(_schemaName))
            {
                using (var command = new HanaCommand($"CREATE SCHEMA \"{_schemaName}\";", Connection, Transaction))
                    command.ExecuteNonQuery();
            }

            string createCommand = $"CREATE SEQUENCE {NameWithSchema} INCREMENT BY 2 MINVALUE 0 MAXVALUE 100 START WITH 2 CACHE 10 CYCLE";
            using (var command = new HanaCommand(createCommand, Connection, Transaction))
                command.ExecuteNonQuery();
        }

        public void Drop()
        {
            using (var command = new HanaCommand("DROP SEQUENCE " + NameWithSchema, Connection, Transaction))
                command.ExecuteNonQuery();

            if (!string.IsNullOrEmpty(_schemaName))
            {
                using (var command = new HanaCommand($"DROP SCHEMA \"{_schemaName}\"", Connection, Transaction))
                    command.ExecuteNonQuery();
            }
        }
    }
}
