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

using System.Data;
using FluentMigrator.Expressions;

namespace FluentMigrator.Builders.Create.ForeignKey
{
    /// <summary>
    /// An expression builder for a <see cref="CreateForeignKeyExpression"/>
    /// </summary>
    public class CreateForeignKeyExpressionBuilder : ExpressionBuilderBase<CreateForeignKeyExpression>,
        ICreateForeignKeyFromTableSyntax,
        ICreateForeignKeyForeignColumnOrInSchemaSyntax,
        ICreateForeignKeyToTableSyntax,
        ICreateForeignKeyPrimaryColumnOrInSchemaSyntax,
        ICreateForeignKeyCascadeSyntax
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreateForeignKeyExpressionBuilder"/> class.
        /// </summary>
        /// <param name="expression">The underlying expression</param>
        public CreateForeignKeyExpressionBuilder(CreateForeignKeyExpression expression)
            : base(expression)
        {
        }

        /// <inheritdoc />
        public ICreateForeignKeyForeignColumnOrInSchemaSyntax FromTable(string table)
        {
            Expression.ForeignKey.ForeignTable = table;
            return this;
        }

        /// <inheritdoc />
        public ICreateForeignKeyToTableSyntax ForeignColumn(string column)
        {
            Expression.ForeignKey.ForeignColumns.Add(column);
            return this;
        }

        /// <inheritdoc />
        public ICreateForeignKeyToTableSyntax ForeignColumns(params string[] columns)
        {
            foreach (var column in columns)
                Expression.ForeignKey.ForeignColumns.Add(column);
            return this;
        }

        /// <inheritdoc />
        ICreateForeignKeyForeignColumnSyntax ICreateForeignKeyForeignColumnOrInSchemaSyntax.InSchema(string schemaName)
        {
            Expression.ForeignKey.ForeignTableSchema = schemaName;
            return this;
        }

        /// <inheritdoc />
        public ICreateForeignKeyPrimaryColumnOrInSchemaSyntax ToTable(string table)
        {
            Expression.ForeignKey.PrimaryTable = table;
            return this;
        }

        /// <inheritdoc />
        public ICreateForeignKeyCascadeSyntax PrimaryColumn(string column)
        {
            Expression.ForeignKey.PrimaryColumns.Add(column);
            return this;
        }

        /// <inheritdoc />
        public ICreateForeignKeyCascadeSyntax PrimaryColumns(params string[] columns)
        {
            foreach (var column in columns)
                Expression.ForeignKey.PrimaryColumns.Add(column);
            return this;
        }

        /// <inheritdoc />
        public ICreateForeignKeyCascadeSyntax OnDelete(Rule rule)
        {
            Expression.ForeignKey.OnDelete = rule;
            return this;
        }

        /// <inheritdoc />
        public ICreateForeignKeyCascadeSyntax OnUpdate(Rule rule)
        {
            Expression.ForeignKey.OnUpdate = rule;
            return this;
        }

        /// <inheritdoc />
        public void OnDeleteOrUpdate(Rule rule)
        {
            Expression.ForeignKey.OnDelete = rule;
            Expression.ForeignKey.OnUpdate = rule;
        }

        /// <inheritdoc />
        ICreateForeignKeyPrimaryColumnSyntax ICreateForeignKeyPrimaryColumnOrInSchemaSyntax.InSchema(string schemaName)
        {
            Expression.ForeignKey.PrimaryTableSchema = schemaName;
            return this;
        }
    }
}
