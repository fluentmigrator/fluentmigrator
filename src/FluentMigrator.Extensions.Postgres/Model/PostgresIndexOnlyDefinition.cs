#region License
// Copyright (c) 2020, Fluent Migrator Project
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

using System;

namespace FluentMigrator.Model
{
    /// <summary>
    /// Represents a definition for a PostgreSQL index with the "ONLY" clause.
    /// </summary>
    /// <remarks>
    /// This class is used to specify whether the "ONLY" clause should be applied to a PostgreSQL index.
    /// The "ONLY" clause restricts the index to the specified table, excluding any inherited tables.
    /// </remarks>
    public class PostgresIndexOnlyDefinition
        : ICloneable
    {
        /// <summary>
        /// Gets or sets a value indicating whether the "ONLY" clause should be applied to the PostgreSQL index.
        /// </summary>
        /// <value>
        /// <c>true</c> if the "ONLY" clause is applied, restricting the index to the specified table and excluding inherited tables;
        /// otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// The "ONLY" clause is used in PostgreSQL to limit the scope of an index to the table it is defined on,
        /// without including any inherited tables.
        /// </remarks>
        public virtual bool IsOnly { get; set; }

        /// <inheritdoc />
        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
