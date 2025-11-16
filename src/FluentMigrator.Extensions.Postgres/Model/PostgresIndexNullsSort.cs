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
    /// Represents the sorting behavior for NULL values in PostgreSQL index definitions.
    /// </summary>
    /// <remarks>
    /// This class is used to specify whether NULL values should be sorted first or last
    /// in PostgreSQL index columns. It implements <see cref="ICloneable"/> to allow cloning
    /// of its instances.
    /// </remarks>
    public class PostgresIndexNullsSort
        : ICloneable
    {
        /// <summary>
        /// Gets or sets the sorting behavior for NULL values in a PostgreSQL index column.
        /// </summary>
        /// <value>
        /// A <see cref="NullSort"/> value indicating whether NULL values should be sorted
        /// first or last in the index.
        /// </value>
        /// <remarks>
        /// This property is used to define the position of NULL values in the sort order
        /// of a PostgreSQL index column. It supports two options: <see cref="NullSort.First"/>
        /// to sort NULL values first, and <see cref="NullSort.Last"/> to sort NULL values last.
        /// </remarks>
        public virtual NullSort Sort { get; set; }

        /// <inheritdoc />
        public object Clone()
        {
            return MemberwiseClone();
        }
    }

    /// <summary>
    /// Specifies the sorting order for NULL values in PostgreSQL index definitions.
    /// </summary>
    /// <remarks>
    /// This enumeration is used to determine whether NULL values should be sorted
    /// first or last in PostgreSQL index columns.
    /// </remarks>
    /// <summary>
    /// Indicates that NULL values should be sorted first.
    /// </summary>
    /// <summary>
    /// Indicates that NULL values should be sorted last.
    /// </summary>
    public enum NullSort
    {
        /// <summary>
        /// Represents the option to sort NULL values first in PostgreSQL index definitions.
        /// </summary>
        /// <remarks>
        /// When this value is used, NULL values will appear before non-NULL values
        /// in the sorted order of the index.
        /// </remarks>
        First,
        /// <summary>
        /// Indicates that NULL values should be sorted last in PostgreSQL index definitions.
        /// </summary>
        /// <remarks>
        /// This value is used in PostgreSQL index columns to specify that NULL values
        /// should appear after all non-NULL values when sorting.
        /// </remarks>
        Last
    }
}
