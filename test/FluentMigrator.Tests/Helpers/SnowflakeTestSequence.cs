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
using System.Data;
using System.Data.Common;

using FluentMigrator.Runner.Generators.Snowflake;
using FluentMigrator.Runner.Processors.Snowflake;

namespace FluentMigrator.Tests.Helpers
{
    public class SnowflakeTestSequence : IDisposable
    {
        private readonly string _schema;

        public SnowflakeProcessor Processor { get; }
        private readonly SnowflakeQuoter _quoter;

        public SnowflakeTestSequence(SnowflakeProcessor processor, string schema, string sequenceName)
        {
            Processor = processor;
            _quoter = Processor.Quoter;
            _schema = schema;

            if (Connection.State != ConnectionState.Open)
                Connection.Open();

            Name = _quoter.UnQuote(sequenceName);
            Create();
        }

        public string Name
        {
            get;
        }

        private IDbConnection Connection => Processor.Connection;

        public void Dispose()
        {
            Drop();
        }

        public void Create()
        {
            if (!string.IsNullOrEmpty(_schema))
            {
                Processor.Execute($"CREATE SCHEMA {_quoter.QuoteSchemaName(_schema)}");
            }

            var createCommand =
                $"CREATE SEQUENCE {_quoter.QuoteSchemaName(_schema)}.{_quoter.Quote(Name)} START 2 INCREMENT BY 2";
            Processor.Execute(createCommand);
        }

        public void Drop()
        {
            Processor.Execute($"DROP SEQUENCE {_quoter.QuoteSchemaName(_schema)}.{_quoter.Quote(Name)}");
            if (!string.IsNullOrEmpty(_schema))
            {
                Processor.Execute($"DROP SCHEMA {_quoter.QuoteSchemaName(_schema)}");
            }
        }
    }
}
