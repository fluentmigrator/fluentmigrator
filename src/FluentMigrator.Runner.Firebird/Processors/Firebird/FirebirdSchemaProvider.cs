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

using System.Collections.Generic;
using System.Linq;

using FluentMigrator.Model;
using FluentMigrator.Runner.Generators.Firebird;
using FluentMigrator.Runner.Models;

namespace FluentMigrator.Runner.Processors.Firebird
{
    public class FirebirdSchemaProvider
    {
        private readonly FirebirdQuoter _quoter;
        internal Dictionary<string, FirebirdTableSchema> TableSchemas = new Dictionary<string, FirebirdTableSchema>();
        public FirebirdProcessor Processor { get; protected set; }

        public FirebirdSchemaProvider(FirebirdProcessor processor, FirebirdQuoter quoter)
        {
            _quoter = quoter;
            Processor = processor;
        }

        public ColumnDefinition GetColumnDefinition(string tableName, string columnName)
        {
            FirebirdTableDefinition firebirdTableDef = GetTableDefinition(tableName);
            return firebirdTableDef.Columns.First(x => x.Name == columnName);
        }

        internal FirebirdTableDefinition GetTableDefinition(string tableName)
        {
            return GetTableSchema(tableName).Definition;
        }

        internal FirebirdTableSchema GetTableSchema(string tableName)
        {
            if (TableSchemas.ContainsKey(tableName))
                return TableSchemas[tableName];
            return LoadTableSchema(tableName);
        }

        internal FirebirdTableSchema LoadTableSchema(string tableName)
        {
            FirebirdTableSchema schema = new FirebirdTableSchema(tableName, Processor, _quoter);
            TableSchemas.Add(tableName, schema);
            return schema;
        }

        public IndexDefinition GetIndex(string tableName, string indexName)
        {
            FirebirdTableDefinition firebirdTableDef = GetTableDefinition(tableName);
            if (firebirdTableDef.Indexes.Any(x => x.Name == indexName))
                return firebirdTableDef.Indexes.First(x => x.Name == indexName);
            return null;
        }

        public SequenceInfo GetSequence(string sequenceName)
        {
            return SequenceInfo.Read(Processor, sequenceName, _quoter);
        }
    }
}
