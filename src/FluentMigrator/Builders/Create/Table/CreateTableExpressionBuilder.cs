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

using System;
using System.Data;
using System.Linq;

using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Model;

namespace FluentMigrator.Builders.Create.Table
{
    /// <summary>
    /// An expression builder for a <see cref="CreateTableExpression"/>
    /// </summary>
    public class CreateTableExpressionBuilder : ExpressionBuilderWithColumnTypesBase<CreateTableExpression, ICreateTableColumnOptionOrWithColumnSyntax>,
                                                ICreateTableWithColumnOrSchemaOrDescriptionSyntax,
                                                ICreateTableColumnAsTypeSyntax,
                                                ICreateTableColumnOptionOrForeignKeyCascadeOrWithColumnSyntax,
                                                IColumnExpressionBuilder
    {
        private readonly IMigrationContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateTableExpressionBuilder"/> class.
        /// </summary>
        /// <param name="expression">The underlying expression</param>
        /// <param name="context">The migration context</param>
        public CreateTableExpressionBuilder(CreateTableExpression expression, IMigrationContext context)
            : base(expression)
        {
            _context = context;
            ColumnHelper = new ColumnExpressionBuilderHelper(this, context);
        }

        /// <summary>
        /// Gets or sets the current column definition
        /// </summary>
        public ColumnDefinition CurrentColumn { get; set; }

        /// <summary>
        /// Gets or sets the current foreign key definition
        /// </summary>
        public ForeignKeyDefinition CurrentForeignKey { get; set; }

        /// <summary>
        /// Gets or sets the column expression builder helper
        /// </summary>
        public ColumnExpressionBuilderHelper ColumnHelper { get; set; }

        /// <inheritdoc />
        public ICreateTableWithColumnSyntax InSchema(string schemaName)
        {
            Expression.SchemaName = schemaName;
            return this;
        }

        /// <inheritdoc />
        public ICreateTableColumnAsTypeSyntax WithColumn(string name)
        {
            var column = new ColumnDefinition { Name = name, TableName = Expression.TableName, ModificationType = ColumnModificationType.Create };
            Expression.Columns.Add(column);
            CurrentColumn = column;
            return this;
        }

        /// <inheritdoc />
        public ICreateTableWithColumnOrSchemaSyntax WithDescription(string description)
        {
            Expression.TableDescription = description;
            return this;
        }

        /// <inheritdoc />
        public ICreateTableColumnOptionOrWithColumnSyntax WithDefault(SystemMethods method)
        {
            CurrentColumn.DefaultValue = method;
            return this;
        }

        /// <inheritdoc />
        public ICreateTableColumnOptionOrWithColumnSyntax WithDefaultValue(object value)
        {
            CurrentColumn.DefaultValue = value;
            return this;
        }

        /// <inheritdoc />
        public ICreateTableColumnOptionOrWithColumnSyntax WithColumnDescription(string description)
        {
            CurrentColumn.ColumnDescriptions.Add("Description", description);
            return this;
        }

        /// <inheritdoc />
        public ICreateTableColumnOptionOrWithColumnSyntax WithColumnAdditionalDescription(string descriptionName, string description)
        {
            if (string.IsNullOrWhiteSpace(descriptionName))
                throw new ArgumentException("Cannot be the empty string.", "descriptionName");

            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Cannot be the empty string.", "description");

            if (CurrentColumn.ColumnDescriptions.Keys.Count(i => i.Equals(descriptionName)) > 0)
                throw new InvalidOperationException("The given descriptionName is already present in the columnDescription list.");

            CurrentColumn.ColumnDescriptions.Add(descriptionName, description);
            return this;
        }

        /// <inheritdoc />
        public ICreateTableColumnOptionOrWithColumnSyntax Identity()
        {
            CurrentColumn.IsIdentity = true;
            return this;
        }

        /// <inheritdoc />
        public ICreateTableColumnOptionOrWithColumnSyntax Indexed()
        {
            return Indexed(null);
        }

        /// <inheritdoc />
        public ICreateTableColumnOptionOrWithColumnSyntax Indexed(string indexName)
        {
            ColumnHelper.Indexed(indexName);
            return this;
        }

        /// <inheritdoc />
        public ICreateTableColumnOptionOrWithColumnSyntax PrimaryKey()
        {
            CurrentColumn.IsPrimaryKey = true;
            return this;
        }

        /// <inheritdoc />
        public ICreateTableColumnOptionOrWithColumnSyntax PrimaryKey(string primaryKeyName)
        {
            CurrentColumn.IsPrimaryKey = true;
            CurrentColumn.PrimaryKeyName = primaryKeyName;
            return this;
        }

        /// <inheritdoc />
        public ICreateTableColumnOptionOrWithColumnSyntax Nullable()
        {
            ColumnHelper.SetNullable(true);
            return this;
        }

        /// <inheritdoc />
        public ICreateTableColumnOptionOrWithColumnSyntax NotNullable()
        {
            ColumnHelper.SetNullable(false);
            return this;
        }

        /// <inheritdoc />
        public ICreateTableColumnOptionOrWithColumnSyntax Unique()
        {
            ColumnHelper.Unique(null);
            return this;
        }

        /// <inheritdoc />
        public ICreateTableColumnOptionOrWithColumnSyntax Unique(string indexName)
        {
            ColumnHelper.Unique(indexName);
            return this;
        }

        /// <inheritdoc />
        public ICreateTableColumnOptionOrForeignKeyCascadeOrWithColumnSyntax ForeignKey(string primaryTableName, string primaryColumnName)
        {
            return ForeignKey(null, null, primaryTableName, primaryColumnName);
        }

        /// <inheritdoc />
        public ICreateTableColumnOptionOrForeignKeyCascadeOrWithColumnSyntax ForeignKey(string foreignKeyName, string primaryTableName, string primaryColumnName)
        {
            return ForeignKey(foreignKeyName, null, primaryTableName, primaryColumnName);
        }

        /// <inheritdoc />
        public ICreateTableColumnOptionOrForeignKeyCascadeOrWithColumnSyntax ForeignKey(
            string foreignKeyName,
            string primaryTableSchema,
            string primaryTableName,
            string primaryColumnName)
        {
            CurrentColumn.IsForeignKey = true;

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
            fk.ForeignKey.ForeignColumns.Add(CurrentColumn.Name);

            _context.Expressions.Add(fk);
            CurrentForeignKey = fk.ForeignKey;
            CurrentColumn.ForeignKey = fk.ForeignKey;
            return this;
        }

        /// <inheritdoc />
        public ICreateTableColumnOptionOrForeignKeyCascadeOrWithColumnSyntax ReferencedBy(string foreignTableName, string foreignColumnName)
        {
            return ReferencedBy(null, null, foreignTableName, foreignColumnName);
        }

        /// <inheritdoc />
        public ICreateTableColumnOptionOrForeignKeyCascadeOrWithColumnSyntax ReferencedBy(string foreignKeyName, string foreignTableName,
                                                                                          string foreignColumnName)
        {
            return ReferencedBy(foreignKeyName, null, foreignTableName, foreignColumnName);
        }

        /// <inheritdoc />
        public ICreateTableColumnOptionOrForeignKeyCascadeOrWithColumnSyntax ReferencedBy(
            string foreignKeyName,
            string foreignTableSchema,
            string foreignTableName,
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

            fk.ForeignKey.PrimaryColumns.Add(CurrentColumn.Name);
            fk.ForeignKey.ForeignColumns.Add(foreignColumnName);

            _context.Expressions.Add(fk);
            CurrentForeignKey = fk.ForeignKey;
            return this;
        }

        /// <inheritdoc />
        public ICreateTableColumnOptionOrForeignKeyCascadeOrWithColumnSyntax ForeignKey()
        {
            CurrentColumn.IsForeignKey = true;
            return this;
        }

        /// <inheritdoc />
        public override ColumnDefinition GetColumnForType()
        {
            return CurrentColumn;
        }

        /// <inheritdoc />
        public ICreateTableColumnOptionOrForeignKeyCascadeOrWithColumnSyntax OnDelete(Rule rule)
        {
            CurrentForeignKey.OnDelete = rule;
            return this;
        }

        /// <inheritdoc />
        public ICreateTableColumnOptionOrForeignKeyCascadeOrWithColumnSyntax OnUpdate(Rule rule)
        {
            CurrentForeignKey.OnUpdate = rule;
            return this;
        }

        /// <inheritdoc />
        public ICreateTableColumnOptionOrWithColumnSyntax OnDeleteOrUpdate(Rule rule)
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
        ColumnDefinition IColumnExpressionBuilder.Column => CurrentColumn;
    }
}
