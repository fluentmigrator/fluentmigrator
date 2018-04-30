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

using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Model;

namespace FluentMigrator.Builders.Delete.Index
{
    /// <summary>
    /// An expression builder for a <see cref="DeleteIndexExpression"/>
    /// </summary>
    public class DeleteIndexExpressionBuilder : ExpressionBuilderBase<DeleteIndexExpression>,
        IDeleteIndexForTableSyntax,
        IDeleteIndexOnColumnOrInSchemaSyntax,
        IDeleteIndexOptionsSyntax,
        ISupportAdditionalFeatures
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteIndexExpressionBuilder"/> class.
        /// </summary>
        /// <param name="expression">The underlying expression</param>
        public DeleteIndexExpressionBuilder(DeleteIndexExpression expression)
            : base(expression)
        {
        }

        /// <summary>
        /// Gets or sets the current column
        /// </summary>
        [Obsolete("Unused by the Fluent Migrator infrastructure")]
        public IndexColumnDefinition CurrentColumn { get; set; }

        /// <inheritdoc />
        public IDictionary<string, object> AdditionalFeatures => Expression.AdditionalFeatures;

        /// <inheritdoc />
        public IDeleteIndexOnColumnOrInSchemaSyntax OnTable(string tableName)
        {
            Expression.Index.TableName = tableName;
            return this;
        }

        /// <inheritdoc />
        public IDeleteIndexOnColumnSyntax InSchema(string schemaName)
        {
            Expression.Index.SchemaName = schemaName;
            return this;
        }

        /// <summary>
        /// Defines the column of the index to delete
        /// </summary>
        /// <param name="columnName">The column name</param>
        public void OnColumn(string columnName)
        {
            var column = new IndexColumnDefinition { Name = columnName };
            Expression.Index.Columns.Add(column);
        }

        /// <summary>
        /// Defines the columns of the index to delete
        /// </summary>
        /// <param name="columnNames">The column names</param>
        public void OnColumns(params string[] columnNames)
        {
            foreach (var columnName in columnNames)
            {
                Expression.Index.Columns.Add(new IndexColumnDefinition { Name = columnName });
            }
        }

        /// <inheritdoc />
        IDeleteIndexOptionsSyntax IDeleteIndexOnColumnSyntax.OnColumns(params string[] columnNames)
        {
            OnColumns(columnNames);
            return this;
        }

        /// <inheritdoc />
        IDeleteIndexOptionsSyntax IDeleteIndexOnColumnSyntax.OnColumn(string columnName)
        {
            OnColumn(columnName);
            return this;
        }

        /// <summary>
        /// Define additional index options
        /// </summary>
        /// <returns>The extension point for additional options</returns>
        public IDeleteIndexOptionsSyntax WithOptions()
        {
            return this;
        }
    }
}
