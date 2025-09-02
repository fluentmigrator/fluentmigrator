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
using System.Linq;
using System.Text;

using JetBrains.Annotations;

namespace FluentMigrator.Runner.Generators.Generic
{
    /// <summary>
    /// Base class to generate descriptions for tables/classes.
    /// </summary>
    public abstract class GenericDescriptionGenerator : IDescriptionGenerator
    {
        /// <inheritdoc />
        protected abstract string GenerateTableDescription(
            string schemaName, string tableName, string tableDescription);

        /// <inheritdoc />
        protected abstract string GenerateColumnDescription(
            string descriptionName, string schemaName, string tableName, string columnName, string columnDescription);

        /// <inheritdoc />
        [StringFormatMethod("format")]
        protected string FormatStatement(string format, params object[] args)
        {
            var builder = new StringBuilder().AppendFormat(format, args);

            AppendSqlStatementEndToken(builder);

            return builder.ToString();
        }

        /// <inheritdoc />
        protected virtual StringBuilder AppendSqlStatementEndToken(StringBuilder stringBuilder)
        {
            return stringBuilder.Append(";");
        }

        /// <inheritdoc />
        public virtual IEnumerable<string> GenerateDescriptionStatements(Expressions.CreateTableExpression expression)
        {
            var statements = new List<string>();

            if (!string.IsNullOrEmpty(expression.TableDescription))
                statements.Add(GenerateTableDescription(expression.SchemaName, expression.TableName, expression.TableDescription));

            foreach (var column in expression.Columns)
            {
                if (string.IsNullOrEmpty(column.ColumnDescription))
                    continue;

                string initialDescriptionStatement = GenerateColumnDescription(
                    "Description",
                    expression.SchemaName,
                    expression.TableName,
                    column.Name,
                    "Description:" + column.ColumnDescription);

                if (column.AdditionalColumnDescriptions.Count == 0)
                {
                    statements.Add(initialDescriptionStatement);
                }
                else
                {
                    initialDescriptionStatement = "Description:" + column.ColumnDescription;
                    var descriptionsList = new List<string>
                    {
                        initialDescriptionStatement
                    };
                    descriptionsList.AddRange(from description in column.AdditionalColumnDescriptions
                                              let newDescriptionStatement = description.Key + ":" + description.Value
                                              select newDescriptionStatement);
                    statements.Add(GenerateColumnDescription("Description", expression.SchemaName, expression.TableName, column.Name, string.Join(Environment.NewLine, descriptionsList)));
                }
            }

            return statements;
        }

        /// <inheritdoc />
        public virtual string GenerateDescriptionStatement(Expressions.AlterTableExpression expression)
        {
            if (string.IsNullOrEmpty(expression.TableDescription))
                return string.Empty;

            return GenerateTableDescription(
                expression.SchemaName, expression.TableName, expression.TableDescription);
        }

        /// <inheritdoc />
        public virtual string GenerateDescriptionStatement(Expressions.CreateColumnExpression expression)
        {
            if (string.IsNullOrEmpty(expression.Column.ColumnDescription))
                return string.Empty;

            string initialDescriptionStatement = GenerateColumnDescription(
                "Description", expression.SchemaName, expression.TableName, expression.Column.Name, "Description:" + expression.Column.ColumnDescription);

            if (expression.Column.AdditionalColumnDescriptions.Count == 0)
            {
                return initialDescriptionStatement;
            }
            else
            {
                initialDescriptionStatement = "Description:" + expression.Column.ColumnDescription;
                var descriptionsList = new List<string>
                {
                    initialDescriptionStatement
                };
                descriptionsList.AddRange(from description in expression.Column.AdditionalColumnDescriptions
                                          let newDescriptionStatement = description.Key + ":" + description.Value
                                          select newDescriptionStatement);
                return GenerateColumnDescription("Description", expression.SchemaName, expression.TableName, expression.Column.Name, string.Join(Environment.NewLine, descriptionsList));
            }
        }

        /// <inheritdoc />
        public virtual string GenerateDescriptionStatement(Expressions.AlterColumnExpression expression)
        {
            if (string.IsNullOrEmpty(expression.Column.ColumnDescription))
                return string.Empty;

            string initialDescriptionStatement = GenerateColumnDescription(string.Empty, expression.SchemaName, expression.TableName, expression.Column.Name, "Description:"+expression.Column.ColumnDescription);
            if (expression.Column.AdditionalColumnDescriptions.Count == 0)
            {
                return initialDescriptionStatement;
            }
            else
            {
                initialDescriptionStatement = "Description:" + expression.Column.ColumnDescription;
                var descriptionsList = new List<string>
                {
                    initialDescriptionStatement
                };
                descriptionsList.AddRange(from description in expression.Column.AdditionalColumnDescriptions
                                          let newDescriptionStatement = description.Key + ":" + description.Value
                                          select newDescriptionStatement);
                return GenerateColumnDescription("Description", expression.SchemaName, expression.TableName, expression.Column.Name, string.Join(Environment.NewLine, descriptionsList));
            }
        }
    }
}
