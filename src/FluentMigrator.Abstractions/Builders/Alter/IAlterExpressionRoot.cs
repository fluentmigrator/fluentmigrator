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

using FluentMigrator.Builders.Alter.Column;
using FluentMigrator.Builders.Alter.Table;

namespace FluentMigrator.Builders.Alter
{
    /// <summary>
    /// The root expression interface for the alterations
    /// </summary>
    public interface IAlterExpressionRoot : IFluentSyntaxRoot
    {
        /// <summary>
        /// Alter the table or its columns/options
        /// </summary>
        /// <param name="tableName">The table name</param>
        /// <returns>The interface for the modifications</returns>
        IAlterTableAddColumnOrAlterColumnOrSchemaOrDescriptionSyntax Table(string tableName);

        /// <summary>
        /// Alter the column for a given table
        /// </summary>
        /// <param name="columnName">The column name</param>
        /// <returns>The interface to specify the table</returns>
        IAlterColumnOnTableSyntax Column(string columnName);
    }
}
