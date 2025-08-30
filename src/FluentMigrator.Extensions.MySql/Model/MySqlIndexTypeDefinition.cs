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
    /// Data model for MySQL Index Type.
    /// </summary>
    public class MySqlIndexTypeDefinition
        : ICloneable
    {
        [Required(ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = nameof(ErrorMessages.ColumnNameCannotBeNullOrEmpty))]
        public virtual IndexType IndexType { get; set; }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }

    /// <summary>
    /// Enumeration of supported MySQL Index Types.
    /// </summary>
    /// <seealso cref="MySqlIndexTypeDefinition"/>
    public enum IndexType
    {
        /// <summary>
        /// MySQL primarily uses B-tree indexes (specifically B+ trees) as its default and most common indexing method for tables.
        /// </summary>
        BTree,
        /// <summary>
        /// MySQL utilizes hash indexes primarily for efficient exact-match lookups. While the MEMORY storage engine supports explicit hash index creation, InnoDB, MySQL's default storage engine, implements an internal feature called the Adaptive Hash Index (AHI).
        /// </summary>
        Hash
    }
}
