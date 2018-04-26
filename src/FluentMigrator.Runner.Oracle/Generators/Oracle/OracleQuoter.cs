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

namespace FluentMigrator.Runner.Generators.Oracle
{
    public class OracleQuoter : OracleQuoterQuotedIdentifier
    {
        public override string Quote(string name)
        {
            return UnQuote(name);
        }

        public override string QuoteConstraintName(string constraintName, string schemaName = null)
        {
            return base.QuoteConstraintName(UnQuote(constraintName), UnQuote(schemaName));
        }

        public override string QuoteIndexName(string indexName, string schemaName)
        {
            return base.QuoteIndexName(UnQuote(indexName), UnQuote(schemaName));
        }

        public override string QuoteTableName(string tableName, string schemaName = null)
        {
            return base.QuoteTableName(UnQuote(tableName), UnQuote(schemaName));
        }

        public override string QuoteSequenceName(string sequenceName, string schemaName)
        {
            return base.QuoteTableName(UnQuote(sequenceName), UnQuote(schemaName));
        }
    }
}
