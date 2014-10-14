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

namespace FluentMigrator.Builders.Create.Table
{
    public class CreateTableExpressionBuilder : ExpressionBuilderWithColumnTypesBase<CreateTableExpression, ICreateTableColumnOptionOrWithColumnSyntax>,
                                                ICreateTableWithColumnOrSchemaOrDescriptionSyntax,
                                                ICreateTableColumnAsTypeSyntax,
                                                ICreateTableColumnOptionOrForeignKeyCascadeOrWithColumnSyntax,
                                                IColumnExpressionBuilder
    {
        private readonly IMigrationContext _context;

        public CreateTableExpressionBuilder(CreateTableExpression expression, IMigrationContext context)
            : base(expression)
        {
            _context = context;
            ColumnHelper = new ColumnExpressionBuilderHelper(this, context);
        }

        public ColumnDefinition CurrentColumn { get; set; }
        public ForeignKeyDefinition CurrentForeignKey { get; set; }
        public ColumnExpressionBuilderHelper ColumnHelper { get; set; }

        public ICreateTableWithColumnSyntax InSchema(string schemaName)
        {
            Expression.SchemaName = schemaName;
            return this;
        }

        public ICreateTableColumnAsTypeSyntax WithColumn(string name)
        {
            var column = new ColumnDefinition { Name = name, TableName = Expression.TableName, ModificationType = ColumnModificationType.Create };
            Expression.Columns.Add(column);
            CurrentColumn = column;
            return this;
        }

        public ICreateTableWithColumnOrSchemaSyntax WithDescription(string description)
        {
            Expression.TableDescription = description;
            return this;
        }

        public ICreateTableColumnOptionOrWithColumnSyntax WithDefault(SystemMethods method)
        {
            CurrentColumn.DefaultValue = method;
            return this;
        }

        public ICreateTableColumnOptionOrWithColumnSyntax WithDefaultValue(object value)
        {
            CurrentColumn.DefaultValue = value;
            return this;
        }

        public ICreateTableColumnOptionOrWithColumnSyntax WithColumnDescription(string description)
        {
            CurrentColumn.ColumnDescription = description;
            return this;
        }

        public ICreateTableColumnOptionOrWithColumnSyntax Identity()
        {
            CurrentColumn.IsIdentity = true;
            return this;
        }

        public ICreateTableColumnOptionOrWithColumnSyntax Indexed()
        {
            return Indexed(null);
        }

        public ICreateTableColumnOptionOrWithColumnSyntax Indexed(string indexName)
        {
            ColumnHelper.Indexed(indexName);
            return this;
        }

        public ICreateTableColumnOptionOrWithColumnSyntax PrimaryKey()
        {
            CurrentColumn.IsPrimaryKey = true;
            return this;
        }

        public ICreateTableColumnOptionOrWithColumnSyntax PrimaryKey(string primaryKeyName)
        {
            CurrentColumn.IsPrimaryKey = true;
            CurrentColumn.PrimaryKeyName = primaryKeyName;
            return this;
        }

        public ICreateTableColumnOptionOrWithColumnSyntax Nullable()
        {
            ColumnHelper.SetNullable(true);
            return this;
        }

        public ICreateTableColumnOptionOrWithColumnSyntax NotNullable()
        {
            ColumnHelper.SetNullable(false);
            return this;
        }

        public ICreateTableColumnOptionOrWithColumnSyntax Unique()
        {
            ColumnHelper.Unique(null);
            return this;
        }

        public ICreateTableColumnOptionOrWithColumnSyntax Unique(string indexName)
        {
            ColumnHelper.Unique(indexName);
            return this;
        }

        public ICreateTableColumnOptionOrForeignKeyCascadeOrWithColumnSyntax ForeignKey(string primaryTableName, string primaryColumnName)
        {
            return ForeignKey(null, null, primaryTableName, primaryColumnName);
        }

        public ICreateTableColumnOptionOrForeignKeyCascadeOrWithColumnSyntax ForeignKey(string foreignKeyName, string primaryTableName, string primaryColumnName)
        {
            return ForeignKey(foreignKeyName, null, primaryTableName, primaryColumnName);
        }

        public ICreateTableColumnOptionOrForeignKeyCascadeOrWithColumnSyntax ForeignKey(string foreignKeyName, string primaryTableSchema,
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

        public ICreateTableColumnOptionOrForeignKeyCascadeOrWithColumnSyntax ReferencedBy(string foreignTableName, string foreignColumnName)
        {
            return ReferencedBy(null, null, foreignTableName, foreignColumnName);
        }

        public ICreateTableColumnOptionOrForeignKeyCascadeOrWithColumnSyntax ReferencedBy(string foreignKeyName, string foreignTableName,
                                                                                          string foreignColumnName)
        {
            return ReferencedBy(foreignKeyName, null, foreignTableName, foreignColumnName);
        }

        public ICreateTableColumnOptionOrForeignKeyCascadeOrWithColumnSyntax ReferencedBy(string foreignKeyName, string foreignTableSchema,
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

        public ICreateTableColumnOptionOrForeignKeyCascadeOrWithColumnSyntax ForeignKey()
        {
            CurrentColumn.IsForeignKey = true;
            return this;
        }

        [Obsolete("Please use ReferencedBy syntax. This method will be removed in the next version")]
        public ICreateTableColumnOptionOrWithColumnSyntax References(string foreignKeyName, string foreignTableName, IEnumerable<string> foreignColumnNames)
        {
            return References(foreignKeyName, null, foreignTableName, foreignColumnNames);
        }

        [Obsolete("Please use ReferencedBy syntax. This method will be removed in the next version")]
        public ICreateTableColumnOptionOrWithColumnSyntax References(string foreignKeyName, string foreignTableSchema, string foreignTableName,
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

        public override ColumnDefinition GetColumnForType()
        {
            return CurrentColumn;
        }

        public ICreateTableColumnOptionOrForeignKeyCascadeOrWithColumnSyntax OnDelete(Rule rule)
        {
            CurrentForeignKey.OnDelete = rule;
            return this;
        }

        public ICreateTableColumnOptionOrForeignKeyCascadeOrWithColumnSyntax OnUpdate(Rule rule)
        {
            CurrentForeignKey.OnUpdate = rule;
            return this;
        }

        public ICreateTableColumnOptionOrWithColumnSyntax OnDeleteOrUpdate(Rule rule)
        {
            OnDelete(rule);
            OnUpdate(rule);
            return this;
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