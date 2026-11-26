#region License
//
// Copyright (c) 2026, Fluent Migrator Project
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

using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Builders.Delete.CheckConstraint
{
    /// <summary>
    /// Builds a <see cref="DeleteCheckConstraintExpression"/>.
    /// </summary>
    public class DeleteCheckConstraintExpressionBuilder : ExpressionBuilderBase<DeleteCheckConstraintExpression>,
        IDeleteCheckFromTableSyntax,
        IInSchemaSyntax,
        IDeleteCheckUnnamedFromTableSyntax,
        IDeleteCheckInSchemaOrColumnsSyntax,
        ISupportAdditionalFeatures
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="DeleteCheckConstraintExpressionBuilder"/> class.
        /// </summary>
        /// <param name="expression">The underlying expression.</param>
        public DeleteCheckConstraintExpressionBuilder(DeleteCheckConstraintExpression expression)
            : base(expression)
        {
        }

        /// <inheritdoc />
        public IDictionary<string, object> AdditionalFeatures => Expression.AdditionalFeatures;

        /// <inheritdoc />
        IDeleteCheckInSchemaOrColumnsSyntax IDeleteCheckUnnamedFromTableSyntax.FromTable(string tableName)
        {
            FromTable(tableName);
            return this;
        }

        /// <inheritdoc />
        IDeleteCheckColumnsSyntax IDeleteCheckInSchemaOrColumnsSyntax.InSchema(string schemaName)
        {
            InSchema(schemaName);
            return this;
        }

        /// <inheritdoc />
        public void Column(string columnName)
        {
            Expression.Constraint.Columns.Add(columnName);
        }

        /// <inheritdoc />
        public void Columns(params string[] columnNames)
        {
            Expression.Constraint.Columns.AddRange(columnNames);
        }

        /// <inheritdoc />
        public IInSchemaSyntax FromTable(string tableName)
        {
            Expression.Constraint.TableName = tableName;
            return this;
        }

        /// <inheritdoc />
        public void InSchema(string schemaName)
        {
            Expression.Constraint.SchemaName = schemaName;
        }
    }
}
