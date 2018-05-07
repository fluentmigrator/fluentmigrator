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
    /// The sequence definition
    /// </summary>
    public class SequenceDefinition
        : ICloneable,
#pragma warning disable 618
          ICanBeValidated
#pragma warning restore 618
    {
        /// <summary>
        /// Gets or sets the sequence name
        /// </summary>
        [Required(ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = nameof(ErrorMessages.SequenceNameCannotBeNullOrEmpty))]
        public virtual string Name { get; set; }

        /// <summary>
        /// Gets or sets the schema name
        /// </summary>
        public virtual string SchemaName { get; set; }

        /// <summary>
        /// Gets or sets the increment of the sequence
        /// </summary>
        public virtual long? Increment { get; set; }

        /// <summary>
        /// Gets or sets the minimum value of the sequence (inclusive)
        /// </summary>
        public virtual long? MinValue { get; set; }

        /// <summary>
        /// Gets or sets the maximum value of the sequence (inclusive)
        /// </summary>
        public virtual long? MaxValue { get; set; }

        /// <summary>
        /// Gets or sets the start value of the sequence
        /// </summary>
        public virtual long? StartWith { get; set; }

        /// <summary>
        /// Gets or sets the number of cached sequence values
        /// </summary>
        /// <remarks>
        /// Normally used together with <see cref="Increment"/>.
        /// </remarks>
        public virtual long? Cache { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the sequence should start with <see cref="MinValue"/> for the next value after <see cref="MaxValue"/>
        /// </summary>
        public virtual bool Cycle { get; set; }

        /// <inheritdoc />
        public object Clone()
        {
            return new SequenceDefinition
                   {
                           Name = Name,
                           SchemaName = SchemaName,
                           Increment = Increment,
                           MinValue = MinValue,
                           MaxValue = MaxValue,
                           StartWith = StartWith,
                           Cache = Cache,
                           Cycle = Cycle
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
