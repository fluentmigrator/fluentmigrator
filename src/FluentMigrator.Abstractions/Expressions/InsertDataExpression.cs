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
using FluentMigrator.Model;

namespace FluentMigrator.Expressions
{
    /// <summary>
    /// Expression to insert data
    /// </summary>
    public class InsertDataExpression : IMigrationExpression, ISupportAdditionalFeatures, ISchemaExpression
    {
        /// <inheritdoc />
        public string SchemaName { get; set; }

        /// <summary>
        /// Gets or sets the table name
        /// </summary>
        [Required]
        public string TableName { get; set; }

        /// <inheritdoc />
        public IDictionary<string, object> AdditionalFeatures { get; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets the rows to be inserted
        /// </summary>
        public List<InsertionDataDefinition> Rows { get; } = new List<InsertionDataDefinition>();

        /// <inheritdoc />
        public void ExecuteWith(IMigrationProcessor processor)
        {
            processor.Process(this);
        }

        /// <inheritdoc />
        public IMigrationExpression Reverse()
        {
            var expression = new DeleteDataExpression
                                {
                                    SchemaName = SchemaName,
                                    TableName = TableName
                                };

            for (var index = Rows.Count - 1; index >= 0; index--)
            {
                var dataDefinition = new DeletionDataDefinition();
                dataDefinition.AddRange(Rows[index]);

                expression.Rows.Add(dataDefinition);
            }

            return expression;
        }
    }
}
