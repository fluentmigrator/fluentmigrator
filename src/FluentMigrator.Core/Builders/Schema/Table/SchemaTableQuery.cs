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

using FluentMigrator.Builders.Schema.Column;
using FluentMigrator.Builders.Schema.Constraint;
using FluentMigrator.Infrastructure;
using FluentMigrator.Builders.Schema.Index;

namespace FluentMigrator.Builders.Schema.Table
{
    public class SchemaTableQuery : ISchemaTableSyntax
    {
        private readonly IMigrationContext _context;
        private readonly string _schemaName;
        private readonly string _tableName;

        public SchemaTableQuery(IMigrationContext context, string schemaName, string tableName)
        {
            _context = context;
            _schemaName = schemaName;
            _tableName = tableName;
        }

        public bool Exists()
        {
            return _context.QuerySchema.TableExists(_schemaName, _tableName);
        }

        public ISchemaColumnSyntax Column(string columnName)
        {
            return new SchemaColumnQuery(_schemaName, _tableName, columnName, _context);
        }

        public ISchemaIndexSyntax Index(string indexName)
        {
            return new SchemaIndexQuery(_schemaName, _tableName, indexName, _context);
        }

        public ISchemaConstraintSyntax Constraint(string constraintName)
        {
            return new SchemaConstraintQuery(_schemaName, _tableName, constraintName, _context);
        }

    }
}