#region License

// Copyright (c) 2007-2024, Fluent Migrator Project
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
using System.Collections.Generic;
using System.Data;
using System.Linq;

using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Model;

namespace FluentMigrator.Builders.Alter.Table
{
    /// <summary>
    /// An expression builder for a <see cref="AlterTableExpression"/>
    /// </summary>
    public class AlterTableExpressionBuilder : ExpressionBuilderWithColumnTypesBase<AlterTableExpression, IAlterTableColumnOptionOrAddColumnOrAlterColumnSyntax>,
                                               IAlterTableAddColumnOrAlterColumnOrSchemaOrDescriptionSyntax,
                                               IAlterTableColumnAsTypeSyntax,
                                               IAlterTableColumnOptionOrAddColumnOrAlterColumnOrForeignKeyCascadeSyntax,
                                               IColumnExpressionBuilder
    {
        private readonly IMigrationContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="AlterTableExpressionBuilder"/> class.
        /// </summary>
        /// <param name="expression">The underlying expression</param>
        /// <param name="context">The migration context</param>
        public AlterTableExpressionBuilder(AlterTableExpression expression, IMigrationContext context)
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
        /// Gets or sets the current foreign key
        /// </summary>
        public ForeignKeyDefinition CurrentForeignKey { get; set; }

        /// <summary>
        /// Gets or sets a column expression builder helper
        /// </summary>
        public ColumnExpressionBuilderHelper ColumnHelper { get; set; }

        /// <inheritdoc />
        public IAlterTableAddColumnOrAlterColumnSyntax InSchema(string schemaName)
        {
            Expression.SchemaName = schemaName;
            return this;
        }

        /// <inheritdoc />
        public void ToSchema(string schemaName)
        {
            var alterSchema = new AlterSchemaExpression
                                  {
                                      SourceSchemaName = Expression.SchemaName,
                                      TableName = Expression.TableName,
                                      DestinationSchemaName = schemaName
                                  };

            _context.Expressions.Add(alterSchema);
        }

        /// <inheritdoc />
        public IAlterTableAddColumnOrAlterColumnOrSchemaSyntax WithDescription(string description)
        {
            Expression.TableDescription = description;
            return this;
        }

        /// <inheritdoc />
        public IAlterTableColumnAsTypeSyntax AddColumn(string name)
        {
            var column = new ColumnDefinition { Name = name, ModificationType = ColumnModificationType.Create };
            var createColumn = new CreateColumnExpression
                                   {
                                       Column = column,
                                       SchemaName = Expression.SchemaName,
                                       TableName = Expression.TableName
                                   };

            CurrentColumn = column;

            _context.Expressions.Add(createColumn);
            return this;
        }

        /// <inheritdoc />
        public IAlterTableColumnAsTypeSyntax AlterColumn(string name)
        {
            var column = new ColumnDefinition { Name = name, ModificationType = ColumnModificationType.Alter };
            var alterColumn = new AlterColumnExpression
                                  {
                                      Column = column,
                                      SchemaName = Expression.SchemaName,
                                      TableName = Expression.TableName
                                  };

            CurrentColumn = column;

            _context.Expressions.Add(alterColumn);
            return this;
        }

        /// <inheritdoc />
        public IAlterTableColumnOptionOrAddColumnOrAlterColumnSyntax WithDefault(SystemMethods method)
        {
            return WithDefaultValue(method);
        }

        /// <inheritdoc />
        public IAlterTableColumnOptionOrAddColumnOrAlterColumnSyntax WithDefaultValue(object value)
        {
            if (CurrentColumn.ModificationType == ColumnModificationType.Alter)
            {
                // TODO: This is code duplication from the AlterColumnExpressionBuilder
                // we need to do a drop constraint and then add constraint to change the default value
                var dc = new AlterDefaultConstraintExpression
                             {
                                 TableName = Expression.TableName,
                                 SchemaName = Expression.SchemaName,
                                 ColumnName = CurrentColumn.Name,
                                 DefaultValue = value
                             };

                _context.Expressions.Add(dc);
            }

            CurrentColumn.DefaultValue = value;
            return this;
        }

        /// <inheritdoc />
        public IAlterTableColumnOptionOrAddColumnOrAlterColumnSyntax SetExistingRowsTo(object value)
        {
           ColumnHelper.SetExistingRowsTo(value);
           return this;
        }

        /// <inheritdoc />
        public IAlterTableColumnOptionOrAddColumnOrAlterColumnSyntax WithColumnDescription(string description)
        {
            CurrentColumn.ColumnDescription = description;
            return this;
        }

        /// <inheritdoc />
        public IAlterTableColumnOptionOrAddColumnOrAlterColumnSyntax WithColumnAdditionalDescription(string descriptionName, string description)
        {
            if (string.IsNullOrWhiteSpace(descriptionName))
                throw new ArgumentException(@"Cannot be a null or empty string.", nameof(descriptionName));

            if (description.Equals("Description"))
                throw new InvalidOperationException("The given descriptionName is already used as a keyword to create a description, please choose another descriptionName.");

            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException(@"Cannot be a null or empty string.", nameof(description));

            if (CurrentColumn.AdditionalColumnDescriptions.Keys.Count(i => i.Equals(descriptionName)) > 0)
                throw new InvalidOperationException("The given descriptionName is already present in the columnDescription list.");

            CurrentColumn.AdditionalColumnDescriptions.Add(descriptionName, description);
            return this;
        }

        /// <inheritdoc />
        public IAlterTableColumnOptionOrAddColumnOrAlterColumnSyntax WithColumnAdditionalDescriptions(Dictionary<string, string> columnDescriptions)
        {
            if (columnDescriptions == null)
                throw new ArgumentNullException(nameof(columnDescriptions));

            if (!columnDescriptions.Any())
                throw new ArgumentException(@"Cannot be empty.", nameof(columnDescriptions));

            if (CurrentColumn.AdditionalColumnDescriptions.Keys.Count(i => i.Equals("Description")) > 0)
                throw new InvalidOperationException("The given descriptionName is already present in the columnDescription list.");

            var isPresent = false;
            foreach (var newDescription in columnDescriptions)
            {
                if (!isPresent)
                {
                    isPresent = CurrentColumn.AdditionalColumnDescriptions.Keys.Count(i => i.Equals(newDescription.Key)) > 0;
                }
            }

            if (isPresent)
                throw new ArgumentException(@"At least one of new keys provided is already present in the columnDescription list.", nameof(columnDescriptions));

            return this;
        }

        /// <inheritdoc />
        public IAlterTableColumnOptionOrAddColumnOrAlterColumnSyntax Identity()
        {
            CurrentColumn.IsIdentity = true;
            return this;
        }

        /// <inheritdoc />
        public IAlterTableColumnOptionOrAddColumnOrAlterColumnSyntax Indexed()
        {
            return Indexed(null);
        }

        /// <inheritdoc />
        public IAlterTableColumnOptionOrAddColumnOrAlterColumnSyntax Indexed(string indexName)
        {
            ColumnHelper.Indexed(indexName);
            return this;
        }

        /// <inheritdoc />
        public IAlterTableColumnOptionOrAddColumnOrAlterColumnSyntax PrimaryKey()
        {
            CurrentColumn.IsPrimaryKey = true;
            return this;
        }

        /// <inheritdoc />
        public IAlterTableColumnOptionOrAddColumnOrAlterColumnSyntax PrimaryKey(string primaryKeyName)
        {
            CurrentColumn.IsPrimaryKey = true;
            CurrentColumn.PrimaryKeyName = primaryKeyName;
            return this;
        }

        /// <inheritdoc />
        public IAlterTableColumnOptionOrAddColumnOrAlterColumnSyntax Nullable()
        {
            ColumnHelper.SetNullable(true);
            return this;
        }

        /// <inheritdoc />
        public IAlterTableColumnOptionOrAddColumnOrAlterColumnSyntax NotNullable()
        {
            ColumnHelper.SetNullable(false);
            return this;
        }

        /// <inheritdoc/>
        public IAlterTableColumnOptionOrAddColumnOrAlterColumnSyntax Computed(string expression, bool stored = false)
        {
            CurrentColumn.Expression = expression;
            CurrentColumn.ExpressionStored = stored;
            return this;
        }

        /// <inheritdoc />
        public IAlterTableColumnOptionOrAddColumnOrAlterColumnSyntax Unique()
        {
            ColumnHelper.Unique(null);
            return this;
        }

        /// <inheritdoc />
        public IAlterTableColumnOptionOrAddColumnOrAlterColumnSyntax Unique(string indexName)
        {
            ColumnHelper.Unique(indexName);
            return this;
        }

        /// <inheritdoc />
        public IAlterTableColumnOptionOrAddColumnOrAlterColumnOrForeignKeyCascadeSyntax ForeignKey(string primaryTableName, string primaryColumnName)
        {
            return ForeignKey(null, null, primaryTableName, primaryColumnName);
        }

        /// <inheritdoc />
        public IAlterTableColumnOptionOrAddColumnOrAlterColumnOrForeignKeyCascadeSyntax ForeignKey(string foreignKeyName, string primaryTableName,
                                                                                                   string primaryColumnName)
        {
            return ForeignKey(foreignKeyName, null, primaryTableName, primaryColumnName);
        }

        /// <inheritdoc />
        public IAlterTableColumnOptionOrAddColumnOrAlterColumnOrForeignKeyCascadeSyntax ForeignKey(
            string foreignKeyName, string primaryTableSchema,
            string primaryTableName, string primaryColumnName)
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
        public IAlterTableColumnOptionOrAddColumnOrAlterColumnOrForeignKeyCascadeSyntax ReferencedBy(string foreignTableName, string foreignColumnName)
        {
            return ReferencedBy(null, null, foreignTableName, foreignColumnName);
        }

        /// <inheritdoc />
        public IAlterTableColumnOptionOrAddColumnOrAlterColumnOrForeignKeyCascadeSyntax ReferencedBy(string foreignKeyName, string foreignTableName,
                                                                                                     string foreignColumnName)
        {
            return ReferencedBy(foreignKeyName, null, foreignTableName, foreignColumnName);
        }

        /// <inheritdoc />
        public IAlterTableColumnOptionOrAddColumnOrAlterColumnOrForeignKeyCascadeSyntax ReferencedBy(string foreignKeyName, string foreignTableSchema,
                                                                                                     string foreignTableName, string foreignColumnName)
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
        public IAlterTableColumnOptionOrAddColumnOrAlterColumnOrForeignKeyCascadeSyntax ForeignKey()
        {
            CurrentColumn.IsForeignKey = true;
            return this;
        }

        /// <inheritdoc />
        public IAlterTableColumnOptionOrAddColumnOrAlterColumnOrForeignKeyCascadeSyntax ForeignKey(string[] foreignColumns, string primaryTableName, string[] primaryColumns)
        {
            return ForeignKey(null, null, foreignColumns, primaryTableName, primaryColumns);
        }

        /// <inheritdoc />
        public IAlterTableColumnOptionOrAddColumnOrAlterColumnOrForeignKeyCascadeSyntax ForeignKey(string foreignKeyName, string[] foreignColumns, string primaryTableName, string[] primaryColumns)
        {
            return ForeignKey(foreignKeyName, null, foreignColumns, primaryTableName, primaryColumns);
        }

        /// <inheritdoc />
        public IAlterTableColumnOptionOrAddColumnOrAlterColumnOrForeignKeyCascadeSyntax ForeignKey(
            string foreignKeyName,
            string primaryTableSchema,
            string[] foreignColumns,
            string primaryTableName,
            string[] primaryColumns)
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

            // Add all foreign and primary columns
            foreach (var column in foreignColumns)
            {
                fk.ForeignKey.ForeignColumns.Add(column);
            }
            foreach (var column in primaryColumns)
            {
                fk.ForeignKey.PrimaryColumns.Add(column);
            }

            _context.Expressions.Add(fk);
            CurrentForeignKey = fk.ForeignKey;
            CurrentColumn.ForeignKey = fk.ForeignKey;
            return this;
        }

        /// <inheritdoc />
        public IAlterTableColumnOptionOrAddColumnOrAlterColumnOrForeignKeyCascadeSyntax OnDelete(Rule rule)
        {
            CurrentForeignKey.OnDelete = rule;
            return this;
        }

        /// <inheritdoc />
        public IAlterTableColumnOptionOrAddColumnOrAlterColumnOrForeignKeyCascadeSyntax OnUpdate(Rule rule)
        {
            CurrentForeignKey.OnUpdate = rule;
            return this;
        }

        /// <inheritdoc />
        public IAlterTableColumnOptionOrAddColumnOrAlterColumnSyntax OnDeleteOrUpdate(Rule rule)
        {
            OnDelete(rule);
            OnUpdate(rule);
            return this;
        }

        /// <inheritdoc />
        public override ColumnDefinition GetColumnForType()
        {
            return CurrentColumn;
        }

        /// <inheritdoc />
        string IColumnExpressionBuilder.SchemaName => Expression.SchemaName;

        /// <inheritdoc />
        string IColumnExpressionBuilder.TableName => Expression.TableName;

        /// <inheritdoc />
        ColumnDefinition IColumnExpressionBuilder.Column => CurrentColumn;
    }
}
