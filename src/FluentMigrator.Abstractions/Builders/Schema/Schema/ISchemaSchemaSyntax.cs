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

using FluentMigrator.Builders.Schema.Sequence;
using FluentMigrator.Builders.Schema.Table;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Builders.Schema.Schema
{
    /// <summary>
    /// Query a schemas (or its of its tables) existence
    /// </summary>
    public interface ISchemaSchemaSyntax : IFluentSyntax
    {
        /// <summary>
        /// Returns <c>true</c> when a schema exists
        /// </summary>
        /// <returns><c>true</c> when a schema exists</returns>
        bool Exists();

        /// <summary>
        /// Specify the table name as basis for a query a database objects existence
        /// </summary>
        /// <param name="tableName">The table name</param>
        /// <returns>The next step</returns>
        ISchemaTableSyntax Table(string tableName);

        /// <summary>
        /// Specify the sequence name as basis for a query a database objects existence
        /// </summary>
        /// <param name="sequenceName"></param>
        /// <returns>The next step</returns>
        ISchemaSequenceSyntax Sequence(string sequenceName);
    }
}
