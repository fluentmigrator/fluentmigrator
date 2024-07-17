#region License
// Copyright (c) 2024, Fluent Migrator Project
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

using FluentMigrator.Infrastructure;

namespace FluentMigrator.Builders.Schema.Sequence
{
    /// <summary>
    /// The implementation of the <see cref="ISchemaSequenceSyntax"/> interface.
    /// </summary>
    public class SchemaSequenceQuery : ISchemaSequenceSyntax
    {
        private readonly IMigrationContext _context;
        private readonly string _schemaName;
        private readonly string _sequenceName;

        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaSequenceQuery"/> class.
        /// </summary>
        /// <param name="context">The migration context</param>
        /// <param name="schemaName">The schema name</param>
        /// <param name="sequenceName">The sequence name</param>
        public SchemaSequenceQuery(IMigrationContext context, string schemaName, string sequenceName)
        {
            _context = context;
            _schemaName = schemaName;
            _sequenceName = sequenceName;
        }

        /// <inheritdoc />
        public bool Exists()
        {
            return _context.QuerySchema.SequenceExists(_schemaName, _sequenceName);
        }
    }
}
