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
using System.ComponentModel.DataAnnotations;

using FluentMigrator.Infrastructure;

namespace FluentMigrator.Model
{
    /// <summary>
    /// The Sequence Constraint Definition
    /// </summary>
    public class SequenceConstraintDefinition
        : ICloneable,
#pragma warning disable 618
          ICanBeValidated
#pragma warning restore 618
    {
        /// <summary>
        /// Gets or sets the sequence constraint name
        /// </summary>
        [Required(ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = nameof(ErrorMessages.SequenceConstraintNameCannotBeNullOrEmpty))]
        public virtual string Name { get; set; }

        /// <summary>
        /// Gets or sets the Table Schema name
        /// </summary>
        public virtual string TableSchemaName { get; set; }

        /// <summary>
        /// Gets or sets the Table Name
        /// </summary>
        public virtual string TableName { get; set; }

        /// <summary>
        /// Gets or sets the Column Name
        /// </summary>
        public virtual string ColumnName { get; set; }

        /// <summary>
        /// Gets or sets Sequence Schema Name
        /// </summary>
        public virtual long? SequenceSchemaName { get; set; }

        /// <summary>
        /// Gets or sets the Sequence Name
        /// </summary>
        public virtual long? SequenceName { get; set; }

        /// <inheritdoc />
        public object Clone()
        {
            return new SequenceConstraintDefinition
            {
                Name = Name,
                TableSchemaName = TableSchemaName,
                TableName = TableName,
                ColumnName = ColumnName,
                SequenceSchemaName = SequenceSchemaName,
                SequenceName = SequenceName
            };
        }

        /// <inheritdoc />
        [Obsolete("Use the System.ComponentModel.DataAnnotations.Validator instead")]
        public void CollectValidationErrors(ICollection<string> errors)
        {
            this.CollectErrors(errors);
        }
    }
}
