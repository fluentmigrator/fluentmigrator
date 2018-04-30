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

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using FluentMigrator.Model;

namespace FluentMigrator.Expressions
{
    /// <summary>
    /// Expression to delete the data
    /// </summary>
    public class DeleteDataExpression : IMigrationExpression, ISchemaExpression
    {
        /// <inheritdoc />
        public virtual string SchemaName { get; set; }

        /// <summary>
        /// Gets or sets the table name
        /// </summary>
        [Required]
        public string TableName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether all rows should be deleted
        /// </summary>
        public virtual bool IsAllRows { get; set; }

        /// <summary>
        /// Gets the list of row definitions
        /// </summary>
        public List<DeletionDataDefinition> Rows { get; } = new List<DeletionDataDefinition>();

        /// <inheritdoc />
        [Obsolete("Use the System.ComponentModel.DataAnnotations.Validator instead")]
        public void CollectValidationErrors(ICollection<string> errors)
        {
            this.CollectErrors(errors);
        }

        /// <inheritdoc />
        public void ExecuteWith(IMigrationProcessor processor)
        {
            processor.Process(this);
        }

        /// <inheritdoc />
        public IMigrationExpression Reverse()
        {
            var expression = new InsertDataExpression
            {
                SchemaName = SchemaName,
                TableName = TableName
            };

            foreach (var row in Rows)
            {
                var dataDefinition = new InsertionDataDefinition();
                dataDefinition.AddRange(row);

                expression.Rows.Add(dataDefinition);
            }

            return expression;
        }
    }
}
