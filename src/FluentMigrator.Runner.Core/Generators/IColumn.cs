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

using System;
using System.Collections.Generic;
using System.Data;

using FluentMigrator.Model;

using JetBrains.Annotations;

namespace FluentMigrator.Runner.Generators
{
    /// <summary>
    /// Interface for column-oriented SQL fragment generation
    /// </summary>
    public interface IColumn
    {
        /// <summary>
        /// Generates the complete column definition SQL fragment
        /// </summary>
        /// <param name="column">The column definition</param>
        /// <returns>The SQL fragment</returns>
        [NotNull]
        string Generate([NotNull] ColumnDefinition column);

        /// <summary>
        /// Generate the SQL fragment for all column definitions
        /// </summary>
        /// <param name="columns">The column definitions</param>
        /// <param name="tableName">The table name</param>
        /// <returns>The SQL fragment</returns>
        [NotNull]
        string Generate([NotNull, ItemNotNull] IEnumerable<ColumnDefinition> columns, [NotNull] string tableName);

        /// <summary>
        /// Generates the default foreign key name
        /// </summary>
        /// <param name="foreignKey">The foreign key definition</param>
        /// <returns>The SQL fragment</returns>
        [NotNull]
        string GenerateForeignKeyName([NotNull] ForeignKeyDefinition foreignKey);

        /// <summary>
        /// Formats the foreign key SQL fragment optionally using a custom foreign key name generator
        /// </summary>
        /// <param name="foreignKey">The foreign key definition</param>
        /// <param name="fkNameGeneration">The custom foreign key name generator</param>
        /// <returns>The SQL fragment</returns>
        [NotNull]
        string FormatForeignKey([NotNull] ForeignKeyDefinition foreignKey, [NotNull] Func<ForeignKeyDefinition, string> fkNameGeneration);

        /// <summary>
        /// Formats the foreign key cascading SQL fragment
        /// </summary>
        /// <param name="onWhat">The action this cascade clause applies to (update or delete)</param>
        /// <param name="rule">The cascade rule</param>
        /// <returns>The formatted SQL fragment</returns>
        [NotNull]
        string FormatCascade([NotNull] string onWhat, Rule rule);
    }
}
