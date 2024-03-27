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

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using FluentMigrator.Infrastructure;

namespace FluentMigrator.Expressions
{
    /// <summary>
    /// Expression to update data
    /// </summary>
    public class UpdateDataExpression : MigrationExpressionBase, ISchemaExpression, IValidatableObject
    {
        /// <inheritdoc />
        public string SchemaName { get; set; }

        /// <summary>
        /// Gets or sets the table name
        /// </summary>
        [Required(ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = nameof(ErrorMessages.TableNameCannotBeNullOrEmpty))]
        public string TableName { get; set; }

        /// <summary>
        /// Gets or sets the values to be set
        /// </summary>
        public List<KeyValuePair<string, object>> Set { get; set; }

        /// <summary>
        /// Gets or sets the condition column/value pairs
        /// </summary>
        public List<KeyValuePair<string, object>> Where { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether all rows should be updated
        /// </summary>
        public bool IsAllRows { get; set; }

        /// <inheritdoc />
        public override void ExecuteWith(IMigrationProcessor processor)
        {
            processor.Process(this);
        }

        /// <inheritdoc />
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!IsAllRows && (Where == null || Where.Count == 0))
                yield return new ValidationResult(ErrorMessages.UpdateDataExpressionMustSpecifyWhereClauseOrAllRows);

            if (IsAllRows && Where != null && Where.Count > 0)
                yield return new ValidationResult(ErrorMessages.UpdateDataExpressionMustNotSpecifyBothWhereClauseAndAllRows);
        }
    }
}
