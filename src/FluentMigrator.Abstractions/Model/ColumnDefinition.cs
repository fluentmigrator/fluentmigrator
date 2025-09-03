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

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;

using FluentMigrator.Infrastructure;

namespace FluentMigrator.Model
{
    /// <summary>
    /// The column definition
    /// </summary>
    public class ColumnDefinition
        : ICloneable,
          IColumnDataType,
          ISupportAdditionalFeatures,
          IValidatableObject
    {
        /// <summary>
        /// Gets or sets the column definition name
        /// </summary>
        [Required(ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = nameof(ErrorMessages.ColumnNameCannotBeNullOrEmpty))]
        public virtual string Name { get; set; }

        /// <summary>
        /// Gets or sets the column type
        /// </summary>
        public virtual DbType? Type { get; set; }

        /// <summary>
        /// Gets or sets the column type size (read: precision or length)
        /// </summary>
        public virtual int? Size { get; set; }

        /// <summary>
        /// Gets or sets the column type precision (read: scale)
        /// </summary>
        public virtual int? Precision { get; set; }

        /// <summary>
        /// Gets or sets a database specific custom column type
        /// </summary>
        public virtual string CustomType { get; set; }

        /// <summary>
        /// Gets or sets a expression that defines the column
        /// </summary>
        public virtual string Expression { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the calculated value is stored
        /// </summary>
        public virtual bool ExpressionStored { get; set; }

        /// <summary>
        /// Gets or sets the columns default value
        /// </summary>
        public virtual object DefaultValue { get; set; } = new UndefinedDefaultValue();

        /// <summary>
        /// Gets or sets a value indicating whether this column is a foreign key
        /// </summary>
        public virtual bool IsForeignKey { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this column gets its value using an identity definition
        /// </summary>
        public virtual bool IsIdentity { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that this column is indexed
        /// </summary>
        public virtual bool IsIndexed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this column is a primary key
        /// </summary>
        public virtual bool IsPrimaryKey { get; set; }

        /// <summary>
        /// Gets or sets the primary key constraint name
        /// </summary>
        public virtual string PrimaryKeyName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this column is nullable
        /// </summary>
        public virtual bool? IsNullable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this column must be unique
        /// </summary>
        public virtual bool IsUnique { get; set; }

        /// <summary>
        /// Gets or sets the columns table name
        /// </summary>
        public virtual string TableName { get; set; }

        /// <summary>
        /// Gets or sets if the column definition results in a CREATE or an ALTER SQL statement
        /// </summary>
        public virtual ColumnModificationType ModificationType { get; set; }

        /// <summary>
        /// Gets or sets the column description
        /// </summary>
        public virtual string ColumnDescription { get; set; }

        /// <summary>
        /// Gets or sets any additional column descriptions
        /// </summary>
        public virtual Dictionary<string, string> AdditionalColumnDescriptions { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the collation name if the column has a string or ansi string type
        /// </summary>
        public virtual string CollationName { get; set; }

        /// <summary>
        /// Gets or sets the foreign key definition
        /// </summary>
        /// <remarks>
        /// A column might be marked as <see cref="IsForeignKey"/>, but
        /// <see cref="ForeignKey"/> might still be <c>null</c>. This
        /// happens when <c>ForeignKey()</c> without arguments gets
        /// called on a column.
        /// </remarks>
        public virtual ForeignKeyDefinition ForeignKey { get; set; }

        /// <inheritdoc />
        public virtual object Clone()
        {
            return MemberwiseClone();
        }

        /// <summary>
        /// Instances of this class are used to specify an undefined default value
        /// </summary>
        public sealed class UndefinedDefaultValue
        {
        }

        /// <inheritdoc />
        public IDictionary<string, object> AdditionalFeatures { get; } = new Dictionary<string, object>();

        /// <inheritdoc />
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Type == null && CustomType == null)
            {
                yield return new ValidationResult(ErrorMessages.ColumnTypeMustBeDefined);
            }

            if (ForeignKey != null)
            {
                foreach (var item in ForeignKey.Validate(validationContext))
                {
                    yield return item;
                }
            }
        }
    }
}
