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

using FluentMigrator.Builders.Schema.Column;
using FluentMigrator.Builders.Schema.Constraint;
using FluentMigrator.Infrastructure;
using FluentMigrator.Builders.Schema.Index;

namespace FluentMigrator.Builders.Schema.Table
{
    /// <summary>
    /// The implementation of the <see cref="ISchemaTableSyntax"/> interface.
    /// </summary>
    public class SchemaTableQuery : ISchemaTableSyntax
    {
        private readonly IMigrationContext _context;
        private readonly string _schemaName;
        private readonly string _tableName;

        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaTableQuery"/> class.
        /// </summary>
        /// <param name="context">The migration context</param>
        /// <param name="schemaName">The schema name</param>
        /// <param name="tableName">The table name</param>
        public SchemaTableQuery(IMigrationContext context, string schemaName, string tableName)
        {
            _context = context;
            _schemaName = schemaName;
            _tableName = tableName;
        }

        /// <inheritdoc />
        public bool Exists()
        {
            return _context.QuerySchema.TableExists(_schemaName, _tableName);
        }

        /// <inheritdoc />
        public ISchemaColumnSyntax Column(string columnName)
        {
            return new SchemaColumnQuery(_schemaName, _tableName, columnName, _context);
        }

        /// <inheritdoc />
        public ISchemaIndexSyntax Index(string indexName)
        {
            return new SchemaIndexQuery(_schemaName, _tableName, indexName, _context);
        }

        /// <inheritdoc />
        public ISchemaConstraintSyntax Constraint(string constraintName)
        {
            return new SchemaConstraintQuery(_schemaName, _tableName, constraintName, _context);
        }
    }
}
