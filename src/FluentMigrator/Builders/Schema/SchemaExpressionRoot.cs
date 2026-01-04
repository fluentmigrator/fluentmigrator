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

using FluentMigrator.Builders.Schema.Schema;
using FluentMigrator.Builders.Schema.Sequence;
using FluentMigrator.Builders.Schema.Table;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Builders.Schema
{
    /// <summary>
    /// The implementation of the <see cref="ISchemaExpressionRoot"/> interface.
    /// </summary>
    public class SchemaExpressionRoot : ISchemaExpressionRoot
    {
        private readonly IMigrationContext _context;

        /// <summary>
        /// ctorc
        /// </summary>
        /// <param name="context">The migration context</param>
        public SchemaExpressionRoot(IMigrationContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        public IMigrationContext GetMigrationContext() => _context;

        /// <inheritdoc />
        public ISchemaTableSyntax Table(string tableName)
        {
            return new SchemaTableQuery(_context, null, tableName);
        }

        /// <inheritdoc />
        public ISchemaSchemaSyntax Schema(string schemaName)
        {
            return new SchemaSchemaQuery(_context, schemaName);
        }

        /// <inheritdoc />
        public ISchemaSequenceSyntax Sequence(string sequenceName)
        {
            return new SchemaSequenceQuery(_context, null, sequenceName);
        }
    }
}
