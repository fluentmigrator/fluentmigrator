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

using System.Collections.Generic;
using System.Linq;

namespace FluentMigrator.Runner.Generators.Generic
{
    /// <summary>
    /// Base class to generate descriptions for tables/classes
    /// </summary>
    public abstract class GenericDescriptionGenerator : IDescriptionGenerator
    {
        protected abstract string GenerateTableDescription(
            string schemaName, string tableName, string tableDescription);
        protected abstract string GenerateColumnDescription(
            string descriptionName, string schemaName, string tableName, string columnName, string columnDescription);

        public virtual IEnumerable<string> GenerateDescriptionStatements(Expressions.CreateTableExpression expression)
        {
            var statements = new List<string>();

            if (!string.IsNullOrEmpty(expression.TableDescription))
                statements.Add(GenerateTableDescription(expression.SchemaName, expression.TableName, expression.TableDescription));

            foreach (var column in expression.Columns)
            {
                if (column.ColumnDescriptions.Count == 0)
                    continue;

                foreach (var description in column.ColumnDescriptions)
                {
                    statements.Add(GenerateColumnDescription(
                    description.Key,
                    expression.SchemaName,
                    expression.TableName,
                    column.Name,
                    description.Value));
                }
            }

            return statements;
        }

        public virtual string GenerateDescriptionStatement(Expressions.AlterTableExpression expression)
        {
            if (string.IsNullOrEmpty(expression.TableDescription))
                return string.Empty;

            return GenerateTableDescription(
                expression.SchemaName, expression.TableName, expression.TableDescription);
        }

        public virtual string GenerateDescriptionStatement(Expressions.CreateColumnExpression expression)
        {
            if (expression.Column.ColumnDescriptions.Count == 0)
                return string.Empty;

            var descriptionsList = new List<string>();
            foreach (var description in expression.Column.ColumnDescriptions)
            {
                var newDescriptionStatement = GenerateColumnDescription(description.Key, expression.SchemaName, expression.TableName, expression.Column.Name, description.Value);
                descriptionsList.Add(newDescriptionStatement);
            }

            if (descriptionsList.Count == 1)
                return descriptionsList.First();

            return string.Join("\r\n", descriptionsList);
        }

        public virtual string GenerateDescriptionStatement(Expressions.AlterColumnExpression expression)
        {
            if (expression.Column.ColumnDescriptions.Count == 0)
                return string.Empty;

            var descriptionsList = new List<string>();
            foreach (var description in expression.Column.ColumnDescriptions)
            {
                var newDescriptionStatement = GenerateColumnDescription(description.Key, expression.SchemaName, expression.TableName, expression.Column.Name, description.Value);
                descriptionsList.Add(newDescriptionStatement);
            }

            if (descriptionsList.Count == 1)
                return descriptionsList.First();

            return string.Join("\r\n", descriptionsList);
        }
    }
}
