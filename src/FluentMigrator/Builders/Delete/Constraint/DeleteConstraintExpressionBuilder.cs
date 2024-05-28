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

using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Builders.Delete.Constraint
{
    /// <summary>
    /// An expression builder for a <see cref="DeleteColumnExpression"/>
    /// </summary>
    public class DeleteConstraintExpressionBuilder
        : ExpressionBuilderBase<DeleteConstraintExpression>,
            IDeleteConstraintOnTableSyntax,
            IDeleteConstraintInSchemaOptionsSyntax,
            ISupportAdditionalFeatures,
            IDeleteConstraintOptionsSyntax,
            IDeleteConstraintColumnsOptionsSyntax
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:CreateConstraintExpressionBuilder"/> class.
        /// </summary>
        /// <param name="expression">The underlying expression</param>
        public DeleteConstraintExpressionBuilder(DeleteConstraintExpression expression, IMigrationContext context, IMigration migration)
            : base(expression)
        {
            _context = context;
            _migration = migration;
        }

        /// <inheritdoc />
        public IDictionary<string, object> AdditionalFeatures => Expression.AdditionalFeatures;

        /// <summary>
        /// The context where the expression was added
        /// </summary>
        private readonly IMigrationContext _context;

        /// <summary>
        /// The base migration instance
        /// </summary>
        private readonly IMigration _migration;

        /// <inheritdoc />
        public IDeleteConstraintInSchemaOptionsSyntax FromTable(string tableName)
        {
            Expression.Constraint.TableName = tableName;
            return this;
        }

        /// <inheritdoc />
        public IDeleteConstraintInSchemaOptionsSyntax InSchema(string schemaName)
        {
            Expression.Constraint.SchemaName = schemaName;
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
            foreach (var columnName in columnNames)
            {
                Expression.Constraint.Columns.Add(columnName);
            }
        }

        /// <inheritdoc />
        public IDeleteConstraintOptionsSyntax WithOptions()
        {
            return this;
        }

        /// <inheritdoc />
        public IDeleteConstraintColumnsOptionsSyntax IfExists()
        {
            var exists = _migration.Schema
                .Schema(Expression.Constraint.SchemaName)
                .Table(Expression.Constraint.TableName)
                .Constraint(Expression.Constraint.ConstraintName)
                .Exists();
            if (!exists) _context.Expressions.Remove(Expression);
            return this;
        }
    }
}
