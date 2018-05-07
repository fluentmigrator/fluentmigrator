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

using FluentMigrator.Infrastructure;
using FluentMigrator.Model;

namespace FluentMigrator.Builders
{
    /// <summary>
    /// Describes common attributes for expression builders which have a current table/column.
    /// </summary>
    public interface IColumnExpressionBuilder : IFluentSyntax
    {
        /// <summary>
        /// Gets the schema name
        /// </summary>
        string SchemaName { get; }

        /// <summary>
        /// Gets the table name
        /// </summary>
        string TableName { get; }

        /// <summary>
        /// Gets the current column definition
        /// </summary>
        ColumnDefinition Column { get; }
    }
}
