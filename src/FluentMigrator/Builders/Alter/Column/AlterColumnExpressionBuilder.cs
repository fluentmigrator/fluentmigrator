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

namespace FluentMigrator.Builders.Alter.Column
{
    public class AlterColumnExpressionBuilder : ExpressionBuilderWithColumnTypesBase<AlterColumnExpression, IAlterColumnOptionSyntax>,
                                                IAlterColumnOnTableSyntax,
                                                IAlterColumnAsTypeOrInSchemaSyntax,
                                                IAlterColumnOptionOrForeignKeyCascadeSyntax,
                                                IColumnExpressionBuilder
    {
        private readonly IMigrationContext _context;

        public AlterColumnExpressionBuilder(AlterColumnExpression expression, IMigrationContext context)
            : base(expression)
        {
            _context = context;
            ColumnHelper = new ColumnExpressionBuilderHelper(this, context);
        }

        public ForeignKeyDefinition CurrentForeignKey { get; set; }
        public ColumnExpressionBuilderHelper ColumnHelper { get; set; }

        public IAlterColumnAsTypeOrInSchemaSyntax OnTable(string name)
        {
            Expression.TableName = name;
            return this;
        }

        public IAlterColumnAsTypeSyntax InSchema(string schemaName)
        {
            Expression.SchemaName = schemaName;
            return this;
        }

        public IAlterColumnOptionSyntax WithDefault(SystemMethods method)
        {
            return WithDefaultValue(method);
        }

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

        public IAlterColumnOptionSyntax WithColumnDescription(string description)
        {
            Expression.Column.ColumnDescription = description;
            return this;
        }

        public IAlterColumnOptionSyntax Identity()
        {
            Expression.Column.IsIdentity = true;
            return this;
        }

        public IAlterColumnOptionSyntax Indexed()
        {
            return Indexed(null);
        }

        public IAlterColumnOptionSyntax Indexed(string indexName)
        {
            ColumnHelper.Indexed(indexName);
            return this;
        }

        public IAlterColumnOptionSyntax PrimaryKey()
        {
            Expression.Column.IsPrimaryKey = true;
            return this;
        }

        public IAlterColumnOptionSyntax PrimaryKey(string primaryKeyName)
        {
            Expression.Column.IsPrimaryKey = true;
            Expression.Column.PrimaryKeyName = primaryKeyName;
            return this;
        }

        public IAlterColumnOptionSyntax Nullable()
        {
            ColumnHelper.SetNullable(true);
            return this;
        }

        public IAlterColumnOptionSyntax NotNullable()
        {
            ColumnHelper.SetNullable(false);
            return this;
        }

        public IAlterColumnOptionSyntax Unique()
        {
            ColumnHelper.Unique(null);
            return this;
        }

        public IAlterColumnOptionSyntax Unique(string indexName)
        {
            ColumnHelper.Unique(indexName);
            return this;
        }

        public IAlterColumnOptionOrForeignKeyCascadeSyntax ForeignKey(string primaryTableName, string primaryColumnName)
        {
            return ForeignKey(null, null, primaryTableName, primaryColumnName);
        }

        public IAlterColumnOptionOrForeignKeyCascadeSyntax ForeignKey(string foreignKeyName, string primaryTableName, string primaryColumnName)
        {
            return ForeignKey(foreignKeyName, null, primaryTableName, primaryColumnName);
        }

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
            return this;
        }

        public IAlterColumnOptionOrForeignKeyCascadeSyntax ReferencedBy(string foreignTableName, string foreignColumnName)
        {
            return ReferencedBy(null, null, foreignTableName, foreignColumnName);
        }

        public IAlterColumnOptionOrForeignKeyCascadeSyntax ReferencedBy(string foreignKeyName, string foreignTableName, string foreignColumnName)
        {
            return ReferencedBy(foreignKeyName, null, foreignTableName, foreignColumnName);
        }

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

        public IAlterColumnOptionOrForeignKeyCascadeSyntax ForeignKey()
        {
            Expression.Column.IsForeignKey = true;
            return this;
        }

        [Obsolete("Please use ReferencedBy syntax. This method will be removed in the next version")]
        public IAlterColumnOptionSyntax References(string foreignKeyName, string foreignTableName, IEnumerable<string> foreignColumnNames)
        {
            return References(foreignKeyName, null, foreignTableName, foreignColumnNames);
        }

        [Obsolete("Please use ReferencedBy syntax. This method will be removed in the next version")]
        public IAlterColumnOptionSyntax References(string foreignKeyName, string foreignTableSchema, string foreignTableName,
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

            fk.ForeignKey.PrimaryColumns.Add(Expression.Column.Name);
            foreach (var foreignColumnName in foreignColumnNames)
                fk.ForeignKey.ForeignColumns.Add(foreignColumnName);

            _context.Expressions.Add(fk);
            return this;
        }

        public override ColumnDefinition GetColumnForType()
        {
            return Expression.Column;
        }

        public IAlterColumnOptionOrForeignKeyCascadeSyntax OnDelete(Rule rule)
        {
            CurrentForeignKey.OnDelete = rule;
            return this;
        }

        public IAlterColumnOptionOrForeignKeyCascadeSyntax OnUpdate(Rule rule)
        {
            CurrentForeignKey.OnUpdate = rule;
            return this;
        }

        public IAlterColumnOptionSyntax OnDeleteOrUpdate(Rule rule)
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
                return Expression.Column;
            }
        }
    }
}