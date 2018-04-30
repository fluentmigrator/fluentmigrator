#region License
//
// Copyright (c) 2007-2018, Sean Chambers <schambers80@gmail.com>
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

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Expressions
{
    /// <summary>
    /// Expression to delete a column
    /// </summary>
    public class DeleteColumnExpression : MigrationExpressionBase, ISchemaExpression, IValidatableObject
    {
        /// <inheritdoc />
        public virtual string SchemaName { get; set; }

        /// <summary>
        /// Gets or sets a table name
        /// </summary>
        [Required(ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = nameof(ErrorMessages.TableNameCannotBeNullOrEmpty))]
        public virtual string TableName { get; set; }

        /// <summary>
        /// Gets or sets the column names
        /// </summary>
        public ICollection<string> ColumnNames { get; set; } = new List<string>();

        /// <inheritdoc />
        public override void ExecuteWith(IMigrationProcessor processor)
        {
            processor.Process(this);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return base.ToString() + TableName + " " + ColumnNames.Aggregate((a, b) => a + ", " + b);
        }

        /// <inheritdoc />
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (ColumnNames == null || !ColumnNames.Any() || ColumnNames.Any(string.IsNullOrEmpty))
            {
                yield return new ValidationResult(ErrorMessages.ColumnNameCannotBeNullOrEmpty);
            }

            if (ColumnNames != null && ColumnNames.GroupBy(x => x).Any(x => x.Count() > 1))
            {
                yield return new ValidationResult(ErrorMessages.ColumnNamesMustBeUnique);
            }
        }
    }
}
