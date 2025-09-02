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
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using FluentMigrator.Infrastructure;

namespace FluentMigrator.Model
{
    /// <summary>
    /// Represents a definition for an included column in a PostgreSQL index.
    /// </summary>
    /// <remarks>
    /// This class is used to specify additional columns to include in a PostgreSQL index.
    /// It implements <see cref="ICloneable"/> for creating shallow copies and
    /// <see cref="IValidatableObject"/> for validating its properties.
    /// </remarks>
    public class PostgresIndexIncludeDefinition
        : ICloneable,
            IValidatableObject
    {
        /// <summary>
        /// Gets or sets the name of the column to be included in the PostgreSQL index.
        /// </summary>
        /// <value>
        /// The name of the included column. This value must not be null, empty, or consist only of whitespace.
        /// </value>
        /// <remarks>
        /// This property is required and is validated to ensure it is not null or empty.
        /// </remarks>
        /// <exception cref="ValidationException">
        /// Thrown when the value is null, empty, or consists only of whitespace.
        /// </exception>
        [Required(ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = nameof(ErrorMessages.IndexIncludeColumnNameMustNotBeNullOrEmpty))]
        public virtual string Name { get; set; }

        /// <inheritdoc />
        public object Clone()
        {
            return MemberwiseClone();
        }

        /// <inheritdoc />
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                yield return new ValidationResult(ErrorMessages.IndexIncludeColumnNameMustNotBeNullOrEmpty);
            }
        }
    }
}
