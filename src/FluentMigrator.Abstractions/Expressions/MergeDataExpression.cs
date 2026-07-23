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
using System.Data;
using System.Linq;

using FluentMigrator.Infrastructure;
using FluentMigrator.Model;

namespace FluentMigrator.Expressions
{
    /// <summary>
    /// Expression to merge (insert or update) data based on match criteria
    /// </summary>
    public class MergeDataExpression : MigrationExpressionBase, ISupportAdditionalFeatures, ISchemaExpression
    {
        /// <inheritdoc />
        public string SchemaName { get; set; }

        /// <summary>
        /// Gets or sets the table name
        /// </summary>
        [Required(ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = nameof(ErrorMessages.TableNameCannotBeNullOrEmpty))]
        public string TableName { get; set; }

        /// <inheritdoc />
        public IDictionary<string, object> AdditionalFeatures { get; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets the rows to be merged
        /// </summary>
        public List<InsertionDataDefinition> Rows { get; } = new List<InsertionDataDefinition>();

        /// <summary>
        /// Gets the column names used for matching existing rows
        /// </summary>
        public List<string> MatchColumns { get; } = new List<string>();

        /// <inheritdoc />
        public override void ExecuteWith(IMigrationProcessor processor)
        {
            // Read existing data from the table
            var existingDataSet = processor.ReadTableData(SchemaName, TableName);
            var existingTable = existingDataSet.Tables[0];

            foreach (var row in Rows)
            {
                // Check if a row with matching columns already exists
                var exists = existingTable.Rows.OfType<DataRow>().Any(existingRow =>
                {
                    return MatchColumns.All(matchColumn =>
                    {
                        var existingValue = existingRow[matchColumn];
                        var newValue = row.Where(p => p.Key == matchColumn).Select(p => p.Value).SingleOrDefault();
                        
                        if (existingValue == null || newValue == null)
                        {
                            return existingValue == newValue;
                        }
                        
                        return existingValue.Equals(newValue);
                    });
                });

                if (exists)
                {
                    ExecuteUpdateWith(processor, row);
                }
                else
                {
                    ExecuteInsertWith(processor, row);
                }
            }
        }

        private void ExecuteUpdateWith(IMigrationProcessor processor, InsertionDataDefinition row)
        {
            var updateExpression = new UpdateDataExpression
            {
                SchemaName = SchemaName,
                TableName = TableName,
                IsAllRows = false,
                Set = row.Where(p => !MatchColumns.Contains(p.Key)).ToList(),
                Where = MatchColumns.Select(matchColumn =>
                {
                    var value = row.Where(p => p.Key == matchColumn).Select(p => p.Value).SingleOrDefault();
                    return new KeyValuePair<string, object>(matchColumn, value);
                }).ToList()
            };

            processor.Process(updateExpression);
        }

        private void ExecuteInsertWith(IMigrationProcessor processor, InsertionDataDefinition row)
        {
            var insertExpression = new InsertDataExpression
            {
                SchemaName = SchemaName,
                TableName = TableName
            };

            foreach (var additionalFeature in AdditionalFeatures)
            {
                insertExpression.AdditionalFeatures.Add(additionalFeature.Key, additionalFeature.Value);
            }

            insertExpression.Rows.Add(row);

            processor.Process(insertExpression);
        }
    }
}