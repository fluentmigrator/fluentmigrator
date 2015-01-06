#region License

// Copyright (c) 2007-2009, Sean Chambers <schambers80@gmail.com>
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
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Model;

namespace FluentMigrator.Builders.Alter.Table
{
    public class AlterTableExpressionBuilder : ExpressionBuilderWithColumnTypesBase<AlterTableExpression, IAlterTableColumnOptionOrAddColumnOrAlterColumnSyntax>,
                                               IAlterTableAddColumnOrAlterColumnOrSchemaOrDescriptionSyntax,
                                               IAlterTableColumnAsTypeSyntax,
                                               IAlterTableColumnOptionOrAddColumnOrAlterColumnOrForeignKeyCascadeSyntax,
                                               IColumnExpressionBuilder
    {
        private readonly IMigrationContext _context;

        public AlterTableExpressionBuilder(AlterTableExpression expression, IMigrationContext context)
            : base(expression)
        {
            _context = context;
            ColumnHelper = new ColumnExpressionBuilderHelper(this, context);
        }

        public ColumnDefinition CurrentColumn { get; set; }
        public ForeignKeyDefinition CurrentForeignKey { get; set; }
        public ColumnExpressionBuilderHelper ColumnHelper { get; set; }

        public IAlterTableAddColumnOrAlterColumnSyntax InSchema(string schemaName)
        {
            Expression.SchemaName = schemaName;
            return this;
        }

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

        public IAlterTableAddColumnOrAlterColumnOrSchemaSyntax WithDescription(string description)
        {
            Expression.TableDescription = description;
            return this;
        }

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

        public IAlterTableColumnOptionOrAddColumnOrAlterColumnSyntax WithDefault(SystemMethods method)
        {
            return WithDefaultValue(method);
        }

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

        public IAlterTableColumnOptionOrAddColumnOrAlterColumnSyntax SetExistingRowsTo(object value)
        {
           ColumnHelper.SetExistingRowsTo(value);
           return this;
        }

        public IAlterTableColumnOptionOrAddColumnOrAlterColumnSyntax WithColumnDescription(string description)
        {
            CurrentColumn.ColumnDescription = description;
            return this;
        }

        public IAlterTableColumnOptionOrAddColumnOrAlterColumnSyntax Identity()
        {
            CurrentColumn.IsIdentity = true;
            return this;
        }

        public IAlterTableColumnOptionOrAddColumnOrAlterColumnSyntax Indexed()
        {
            return Indexed(null);
        }

        public IAlterTableColumnOptionOrAddColumnOrAlterColumnSyntax Indexed(string indexName)
        {
            ColumnHelper.Indexed(indexName);
            return this;
        }

        public IAlterTableColumnOptionOrAddColumnOrAlterColumnSyntax PrimaryKey()
        {
            CurrentColumn.IsPrimaryKey = true;
            return this;
        }

        public IAlterTableColumnOptionOrAddColumnOrAlterColumnSyntax PrimaryKey(string primaryKeyName)
        {
            CurrentColumn.IsPrimaryKey = true;
            CurrentColumn.PrimaryKeyName = primaryKeyName;
            return this;
        }

        public IAlterTableColumnOptionOrAddColumnOrAlterColumnSyntax Nullable()
        {
            ColumnHelper.SetNullable(true);
            return this;
        }

        public IAlterTableColumnOptionOrAddColumnOrAlterColumnSyntax NotNullable()
        {
            ColumnHelper.SetNullable(false);
            return this;
        }

        public IAlterTableColumnOptionOrAddColumnOrAlterColumnSyntax Unique()
        {
            ColumnHelper.Unique(null);
            return this;
        }

        public IAlterTableColumnOptionOrAddColumnOrAlterColumnSyntax Unique(string indexName)
        {
            ColumnHelper.Unique(indexName);
            return this;
        }

        public IAlterTableColumnOptionOrAddColumnOrAlterColumnOrForeignKeyCascadeSyntax ForeignKey(string primaryTableName, string primaryColumnName)
        {
            return ForeignKey(null, null, primaryTableName, primaryColumnName);
        }

        public IAlterTableColumnOptionOrAddColumnOrAlterColumnOrForeignKeyCascadeSyntax ForeignKey(string foreignKeyName, string primaryTableName,
                                                                                                   string primaryColumnName)
        {
            return ForeignKey(foreignKeyName, null, primaryTableName, primaryColumnName);
        }

        public IAlterTableColumnOptionOrAddColumnOrAlterColumnOrForeignKeyCascadeSyntax ForeignKey(string foreignKeyName, string primaryTableSchema,
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
            return this;
        }

        public IAlterTableColumnOptionOrAddColumnOrAlterColumnOrForeignKeyCascadeSyntax ReferencedBy(string foreignTableName, string foreignColumnName)
        {
            return ReferencedBy(null, null, foreignTableName, foreignColumnName);
        }

        public IAlterTableColumnOptionOrAddColumnOrAlterColumnOrForeignKeyCascadeSyntax ReferencedBy(string foreignKeyName, string foreignTableName,
                                                                                                     string foreignColumnName)
        {
            return ReferencedBy(foreignKeyName, null, foreignTableName, foreignColumnName);
        }

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

        public IAlterTableColumnOptionOrAddColumnOrAlterColumnOrForeignKeyCascadeSyntax ForeignKey()
        {
            CurrentColumn.IsForeignKey = true;
            return this;
        }

        [Obsolete("Please use ReferencedBy syntax. This method will be removed in the next version")]
        public IAlterTableColumnOptionOrAddColumnOrAlterColumnSyntax References(string foreignKeyName, string foreignTableName,
                                                                                IEnumerable<string> foreignColumnNames)
        {
            return References(foreignKeyName, null, foreignTableName, foreignColumnNames);
        }

        [Obsolete("Please use ReferencedBy syntax. This method will be removed in the next version")]
        public IAlterTableColumnOptionOrAddColumnOrAlterColumnSyntax References(string foreignKeyName, string foreignTableSchema, string foreignTableName,
                                                                                IEnumerable<string> foreignColumnNames)
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
            foreach (var foreignColumnName in foreignColumnNames)
                fk.ForeignKey.ForeignColumns.Add(foreignColumnName);

            _context.Expressions.Add(fk);
            return this;
        }

        public IAlterTableColumnOptionOrAddColumnOrAlterColumnOrForeignKeyCascadeSyntax OnDelete(Rule rule)
        {
            CurrentForeignKey.OnDelete = rule;
            return this;
        }

        public IAlterTableColumnOptionOrAddColumnOrAlterColumnOrForeignKeyCascadeSyntax OnUpdate(Rule rule)
        {
            CurrentForeignKey.OnUpdate = rule;
            return this;
        }

        public IAlterTableColumnOptionOrAddColumnOrAlterColumnSyntax OnDeleteOrUpdate(Rule rule)
        {
            OnDelete(rule);
            OnUpdate(rule);
            return this;
        }

        public override ColumnDefinition GetColumnForType()
        {
            return CurrentColumn;
        }

        string IColumnExpressionBuilder.SchemaName
        {
           get
           {
              return Expression.SchemaName;
           }
        }

        string IColumnExpressionBuilder.TableName
        {
           get
           {
              return Expression.TableName;
           }
        }

        ColumnDefinition IColumnExpressionBuilder.Column
        {
           get
           {
              return CurrentColumn;
           }
        }
    }
}