#region License
// 
// Copyright (c) 2007-2009, Sean Chambers <schambers80@gmail.com>
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

using FluentMigrator.Builders.Schema.Table;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Builders.Schema.Schema
{
    public class SchemaSchemaQuery : ISchemaSchemaSyntax
    {
        private readonly IMigrationContext _context;
        private readonly string _schemaName;

        public SchemaSchemaQuery(IMigrationContext context, string schemaName)
        {
            _context = context;
            _schemaName = schemaName;
        }

        public bool Exists()
        {
            return _context.QuerySchema.SchemaExists(_schemaName);
        }

        public ISchemaTableSyntax Table(string tableName)
        {
            return new SchemaTableQuery(_context, _schemaName, tableName);
        }
    }
}