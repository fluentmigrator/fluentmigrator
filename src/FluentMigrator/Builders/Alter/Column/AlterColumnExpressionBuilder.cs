#region License

// Copyright (c) 2007-2018, Sean Chambers <schambers80@gmail.com>
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#endregion

using System.Data;

using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Model;

namespace FluentMigrator.Builders.Alter.Column
{
    /// <summary>
    /// An expression builder for a <see cref="AlterColumnExpression"/>
    /// </summary>
    public class AlterColumnExpressionBuilder : ExpressionBuilderWithColumnTypesBase<AlterColumnExpression, IAlterColumnOptionSyntax>,
                                                IAlterColumnOnTableSyntax,
                                                IAlterColumnAsTypeOrInSchemaSyntax,
                                                IAlterColumnOptionOrForeignKeyCascadeSyntax,
                                                IColumnExpressionBuilder
    {
        private readonly IMigrationContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="AlterColumnExpressionBuilder"/> class.
        /// </summary>
        /// <param name="expression">The underlying expression</param>
        /// <param name="context">The migration context</param>
        public AlterColumnExpressionBuilder(AlterColumnExpression expression, IMigrationContext context)
            : base(expression)
        {
            _context = context;
            ColumnHelper = new ColumnExpressionBuilderHelper(this, context);
        }

        /// <summary>
        /// Gets or sets the current foreign key
        /// </summary>
        public ForeignKeyDefinition CurrentForeignKey { get; set; }

        /// <summary>
        /// Gets or sets a column expression builder helper
        /// </summary>
        public ColumnExpressionBuilderHelper ColumnHelper { get; set; }

        /// <inheritdoc />
        public IAlterColumnAsTypeOrInSchemaSyntax OnTable(string name)
        {
            Expression.TableName = name;
            return this;
        }

        /// <inheritdoc />
        public IAlterColumnAsTypeSyntax InSchema(string schemaName)
        {
            Expression.SchemaName = schemaName;
            return this;
        }

        /// <inheritdoc />
        public IAlterColumnOptionSyntax WithDefault(SystemMethods method)
        {
            return WithDefaultValue(method);
        }

        /// <inheritdoc />
        public IAlterColumnOptionSyntax WithDefaultValue(object value)
        {
            // we need to do a drop constraint and then add constraint to change the default value
            var dc = new AlterDefaultConstraintExpression
                         {
                             TableName = Expression.TableName,
                             SchemaName = Expression.SchemaName,
                             ColumnName = Expression.Column.Name,
                             DefaultValue = value
                         };

            _context.Expressions.Add(dc);

            Expression.Column.DefaultValue = value;

            return this;
        }

        /// <inheritdoc />
        public IAlterColumnOptionSyntax WithColumnDescription(string description)
        {
            Expression.Column.ColumnDescriptions.Add("Description",description);
            return this;
        }

        /// <inheritdoc />
        public IAlterColumnOptionSyntax WithColumnAdditionalDescription(string propertyName, string description)
        {
            Expression.Column.ColumnDescriptions.Add(propertyName, description);
            return this;
        }

        /// <inheritdoc />
        public IAlterColumnOptionSyntax Identity()
        {
            Expression.Column.IsIdentity = true;
            return this;
        }

        /// <inheritdoc />
        public IAlterColumnOptionSyntax Indexed()
        {
            return Indexed(null);
        }

        /// <inheritdoc />
        public IAlterColumnOptionSyntax Indexed(string indexName)
        {
            ColumnHelper.Indexed(indexName);
            return this;
        }

        /// <inheritdoc />
        public IAlterColumnOptionSyntax PrimaryKey()
        {
            Expression.Column.IsPrimaryKey = true;
            return this;
        }

        /// <inheritdoc />
        public IAlterColumnOptionSyntax PrimaryKey(string primaryKeyName)
        {
            Expression.Column.IsPrimaryKey = true;
            Expression.Column.PrimaryKeyName = primaryKeyName;
            return this;
        }

        /// <inheritdoc />
        public IAlterColumnOptionSyntax Nullable()
        {
            ColumnHelper.SetNullable(true);
            return this;
        }

        /// <inheritdoc />
        public IAlterColumnOptionSyntax NotNullable()
        {
            ColumnHelper.SetNullable(false);
            return this;
        }

        /// <inheritdoc />
        public IAlterColumnOptionSyntax Unique()
        {
            ColumnHelper.Unique(null);
            return this;
        }

        /// <inheritdoc />
        public IAlterColumnOptionSyntax Unique(string indexName)
        {
            ColumnHelper.Unique(indexName);
            return this;
        }

        /// <inheritdoc />
        public IAlterColumnOptionOrForeignKeyCascadeSyntax ForeignKey(string primaryTableName, string primaryColumnName)
        {
            return ForeignKey(null, null, primaryTableName, primaryColumnName);
        }

        /// <inheritdoc />
        public IAlterColumnOptionOrForeignKeyCascadeSyntax ForeignKey(string foreignKeyName, string primaryTableName, string primaryColumnName)
        {
            return ForeignKey(foreignKeyName, null, primaryTableName, primaryColumnName);
        }

        /// <inheritdoc />
        public IAlterColumnOptionOrForeignKeyCascadeSyntax ForeignKey(string foreignKeyName, string primaryTableSchema, string primaryTableName,
                                                                      string primaryColumnName)
        {
            Expression.Column.IsForeignKey = true;

            var fk = new CreateForeignKeyExpression
                         {
                             ForeignKey = new ForeignKeyDefinition
                                              {
                                                  Name = foreignKeyName,
                                                  PrimaryTable = primaryTableName,
                                                  PrimaryTableSchema = primaryTableSchema,
                                                  ForeignTable = Expression.TableName,
                                                  ForeignTableSchema = Expression.SchemaName
                                              }
                         };

            fk.ForeignKey.PrimaryColumns.Add(primaryColumnName);
            fk.ForeignKey.ForeignColumns.Add(Expression.Column.Name);

            _context.Expressions.Add(fk);
            CurrentForeignKey = fk.ForeignKey;
            Expression.Column.ForeignKey = fk.ForeignKey;
            return this;
        }

        /// <inheritdoc />
        public IAlterColumnOptionOrForeignKeyCascadeSyntax ReferencedBy(string foreignTableName, string foreignColumnName)
        {
            return ReferencedBy(null, null, foreignTableName, foreignColumnName);
        }

        /// <inheritdoc />
        public IAlterColumnOptionOrForeignKeyCascadeSyntax ReferencedBy(string foreignKeyName, string foreignTableName, string foreignColumnName)
        {
            return ReferencedBy(foreignKeyName, null, foreignTableName, foreignColumnName);
        }

        /// <inheritdoc />
        public IAlterColumnOptionOrForeignKeyCascadeSyntax ReferencedBy(string foreignKeyName, string foreignTableSchema, string foreignTableName,
                                                                        string foreignColumnName)
        {
            var fk = new CreateForeignKeyExpression
                         {
                             ForeignKey = new ForeignKeyDefinition
                                              {
                                                  Name = foreignKeyName,
                                                  PrimaryTable = Expression.TableName,
                                                  PrimaryTableSchema = Expression.SchemaName,
                                                  ForeignTable = foreignTableName,
                                                  ForeignTableSchema = foreignTableSchema
                                              }
                         };

            fk.ForeignKey.PrimaryColumns.Add(Expression.Column.Name);
            fk.ForeignKey.ForeignColumns.Add(foreignColumnName);

            _context.Expressions.Add(fk);
            CurrentForeignKey = fk.ForeignKey;
            return this;
        }

        /// <inheritdoc />
        public IAlterColumnOptionOrForeignKeyCascadeSyntax ForeignKey()
        {
            Expression.Column.IsForeignKey = true;
            return this;
        }

        /// <inheritdoc />
        public override ColumnDefinition GetColumnForType()
        {
            return Expression.Column;
        }

        /// <inheritdoc />
        public IAlterColumnOptionOrForeignKeyCascadeSyntax OnDelete(Rule rule)
        {
            CurrentForeignKey.OnDelete = rule;
            return this;
        }

        /// <inheritdoc />
        public IAlterColumnOptionOrForeignKeyCascadeSyntax OnUpdate(Rule rule)
        {
            CurrentForeignKey.OnUpdate = rule;
            return this;
        }

        /// <inheritdoc />
        public IAlterColumnOptionSyntax OnDeleteOrUpdate(Rule rule)
        {
            OnDelete(rule);
            OnUpdate(rule);
            return this;
        }

        /// <inheritdoc />
        string IColumnExpressionBuilder.SchemaName => Expression.SchemaName;

        /// <inheritdoc />
        string IColumnExpressionBuilder.TableName => Expression.TableName;

        /// <inheritdoc />
        ColumnDefinition IColumnExpressionBuilder.Column => Expression.Column;
    }
}
