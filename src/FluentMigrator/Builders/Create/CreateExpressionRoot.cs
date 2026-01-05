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

using FluentMigrator.Builders.Create.Column;
using FluentMigrator.Builders.Create.Constraint;
using FluentMigrator.Builders.Create.ForeignKey;
using FluentMigrator.Builders.Create.Index;
using FluentMigrator.Builders.Create.Schema;
using FluentMigrator.Builders.Create.Sequence;
using FluentMigrator.Builders.Create.Table;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Model;

namespace FluentMigrator.Builders.Create
{
    /// <summary>
    /// The <see cref="ICreateExpressionRoot"/> implementation
    /// </summary>
    public class CreateExpressionRoot : ICreateExpressionRoot, IMigrationContextAccessor
    {
        private readonly IMigrationContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateExpressionRoot"/> class.
        /// </summary>
        /// <param name="context">The migration context</param>
        public CreateExpressionRoot(IMigrationContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        IMigrationContext IMigrationContextAccessor.GetMigrationContext() => _context;

        /// <inheritdoc />
        public ICreateSchemaOptionsSyntax Schema(string schemaName)
        {
            var expression = new CreateSchemaExpression { SchemaName = schemaName };
            _context.Expressions.Add(expression);
            return new CreateSchemaExpressionBuilder(expression);
        }

        /// <inheritdoc />
        public ICreateTableWithColumnOrSchemaOrDescriptionSyntax Table(string tableName)
        {
            var expression = new CreateTableExpression { TableName = tableName };
            _context.Expressions.Add(expression);
            return new CreateTableExpressionBuilder(expression, _context);
        }

        /// <inheritdoc />
        public ICreateColumnOnTableSyntax Column(string columnName)
        {
            var expression = new CreateColumnExpression { Column = { Name = columnName } };
            _context.Expressions.Add(expression);
            return new CreateColumnExpressionBuilder(expression, _context);
        }

        /// <inheritdoc />
        public ICreateForeignKeyFromTableSyntax ForeignKey()
        {
            var expression = new CreateForeignKeyExpression();
            _context.Expressions.Add(expression);
            return new CreateForeignKeyExpressionBuilder(expression);
        }

        /// <inheritdoc />
        public ICreateForeignKeyFromTableSyntax ForeignKey(string foreignKeyName)
        {
            var expression = new CreateForeignKeyExpression { ForeignKey = { Name = foreignKeyName } };
            _context.Expressions.Add(expression);
            return new CreateForeignKeyExpressionBuilder(expression);
        }

        /// <inheritdoc />
        public ICreateIndexForTableSyntax Index()
        {
            var expression = new CreateIndexExpression();
            _context.Expressions.Add(expression);
            return new CreateIndexExpressionBuilder(expression);
        }

        /// <inheritdoc />
        public ICreateIndexForTableSyntax Index(string indexName)
        {
            var expression = new CreateIndexExpression { Index = { Name = indexName } };
            _context.Expressions.Add(expression);
            return new CreateIndexExpressionBuilder(expression);
        }

        /// <inheritdoc />
        public ICreateSequenceInSchemaSyntax Sequence(string sequenceName)
        {
            var expression = new CreateSequenceExpression { Sequence = { Name = sequenceName } };
            _context.Expressions.Add(expression);
            return new CreateSequenceExpressionBuilder(expression);
        }

        /// <inheritdoc />
        public ICreateConstraintOnTableSyntax UniqueConstraint()
        {
            var expression = new CreateConstraintExpression(ConstraintType.Unique);
            _context.Expressions.Add(expression);
            return new CreateConstraintExpressionBuilder(expression);
        }

        /// <inheritdoc />
        public ICreateConstraintOnTableSyntax UniqueConstraint(string constraintName)
        {
            var expression = new CreateConstraintExpression(ConstraintType.Unique);
            expression.Constraint.ConstraintName = constraintName;
            _context.Expressions.Add(expression);
            return new CreateConstraintExpressionBuilder(expression);
        }

        /// <inheritdoc />
        public ICreateConstraintOnTableSyntax PrimaryKey()
        {
            var expression = new CreateConstraintExpression(ConstraintType.PrimaryKey);
            _context.Expressions.Add(expression);
            return new CreateConstraintExpressionBuilder(expression);
        }

        /// <inheritdoc />
        public ICreateConstraintOnTableSyntax PrimaryKey(string primaryKeyName)
        {
            var expression = new CreateConstraintExpression(ConstraintType.PrimaryKey);
            expression.Constraint.ConstraintName = primaryKeyName;
            _context.Expressions.Add(expression);
            return new CreateConstraintExpressionBuilder(expression);
        }
    }
}
