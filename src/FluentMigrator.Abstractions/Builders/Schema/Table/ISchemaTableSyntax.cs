#region License
//
// Copyright (c) 2007-2018, Sean Chambers <schambers80@gmail.com>
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
using FluentMigrator.Builders.Schema.Index;

namespace FluentMigrator.Builders.Schema.Table
{
    /// <summary>
    /// Queries a tables (or one of its childs) existence
    /// </summary>
    public interface ISchemaTableSyntax
    {
        /// <summary>
        /// Returns <c>true</c> when the table exists
        /// </summary>
        /// <returns><c>true</c> when the table exists</returns>
        bool Exists();

        /// <summary>
        /// Specifies the column to check
        /// </summary>
        /// <param name="columnName">The column name</param>
        /// <returns>The next step</returns>
        ISchemaColumnSyntax Column(string columnName);

        /// <summary>
        /// Specify the index to check
        /// </summary>
        /// <param name="indexName">The index name</param>
        /// <returns>The next step</returns>
        ISchemaIndexSyntax Index(string indexName);

        /// <summary>
        /// Specify the constraint to check
        /// </summary>
        /// <param name="constraintName">The constraint name</param>
        /// <returns>The next step</returns>
        ISchemaConstraintSyntax Constraint(string constraintName);
    }
}
