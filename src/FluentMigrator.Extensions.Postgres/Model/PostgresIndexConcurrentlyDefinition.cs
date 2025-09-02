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
using System.ComponentModel.DataAnnotations;

using FluentMigrator.Infrastructure;

namespace FluentMigrator.Model
{
    /// <summary>
    /// Represents the definition of a PostgreSQL index with the ability to specify whether it should be created concurrently.
    /// </summary>
    /// <remarks>
    /// This class is used to define the "CONCURRENTLY" option for PostgreSQL index creation, which allows the index to be created without locking the table.
    /// It implements <see cref="ICloneable"/> to allow cloning of its instances.
    /// </remarks>
    public class PostgresIndexConcurrentlyDefinition
        : ICloneable
    {
        /// <summary>
        /// Gets or sets a value indicating whether the PostgreSQL index should be created concurrently.
        /// </summary>
        /// <value>
        /// <c>true</c> if the index should be created with the "CONCURRENTLY" option, allowing it to be created without locking the table; 
        /// otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// The "CONCURRENTLY" option is useful for creating indexes on large tables in PostgreSQL without blocking write operations.
        /// </remarks>
        [Required(ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = nameof(ErrorMessages.ColumnNameCannotBeNullOrEmpty))]
        public virtual bool IsConcurrently { get; set; }

        /// <inheritdoc />
        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
