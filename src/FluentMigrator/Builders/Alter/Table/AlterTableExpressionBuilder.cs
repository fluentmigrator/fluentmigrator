﻿#region License
// 
// Copyright (c) 2007-2009, Sean Chambers <schambers80@gmail.com>
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
using FluentMigrator.Model;
using System;

namespace FluentMigrator.Builders.Alter.Table
{
    public class AlterTableExpressionBuilder : ExpressionBuilderWithColumnTypesBase<AlterTableExpression, IAlterTableColumnOptionOrAddColumnOrAlterColumnSyntax>,
        IAlterTableAddColumnOrAlterColumnOrSchemaSyntax,
        IAlterTableColumnAsTypeSyntax,
        IAlterTableColumnOptionOrAddColumnOrAlterColumnSyntax,
        IAlterTableColumnOptionOrAddColumnOrAlterColumnOrForeignKeyCascadeSyntax
    {
        public ColumnDefinition CurrentColumn { get; set; }
        public ForeignKeyDefinition CurrentForeignKey { get; set; }
        private readonly IMigrationContext _context;

        public AlterTableExpressionBuilder(AlterTableExpression expression, IMigrationContext context)
            : base(expression)
        {
            _context = context;
        }

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

          _context.Expressions.Add( alterSchema );
        }

        public IAlterTableColumnAsTypeSyntax AddColumn(string name)
        {
            var column = new ColumnDefinition { Name = name };
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
            var column = new ColumnDefinition { Name = name };
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

        public IAlterTableColumnOptionOrAddColumnOrAlterColumnSyntax WithDefaultValue(object value)
        {
            CurrentColumn.DefaultValue = value;
            return this;
        }

        public IAlterTableColumnOptionOrAddColumnOrAlterColumnSyntax Identity()
        {
            CurrentColumn.IsIdentity = true;
            return this;
        }

        public IAlterTableColumnOptionOrAddColumnOrAlterColumnSyntax Indexed()
        {
            CurrentColumn.IsIndexed = true;
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
            CurrentColumn.IsNullable = true;
            return this;
        }

        public IAlterTableColumnOptionOrAddColumnOrAlterColumnSyntax NotNullable()
        {
            CurrentColumn.IsNullable = false;
            return this;
        }

        public IAlterTableColumnOptionOrAddColumnOrAlterColumnSyntax Unique()
        {
            CurrentColumn.IsUnique = true;
            return this;
        }

        public IAlterTableColumnOptionOrAddColumnOrAlterColumnOrForeignKeyCascadeSyntax ForeignKey(string primaryTableName, string primaryColumnName)
        {
            return ForeignKey(null, null, primaryTableName, primaryColumnName);
        }

        public IAlterTableColumnOptionOrAddColumnOrAlterColumnOrForeignKeyCascadeSyntax ForeignKey(string foreignKeyName, string primaryTableName, string primaryColumnName)
        {
            return ForeignKey(foreignKeyName, null, primaryTableName, primaryColumnName);
        }

        public IAlterTableColumnOptionOrAddColumnOrAlterColumnOrForeignKeyCascadeSyntax ForeignKey(string foreignKeyName, string primaryTableSchema, string primaryTableName, string primaryColumnName)
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

        public IAlterTableColumnOptionOrAddColumnOrAlterColumnOrForeignKeyCascadeSyntax ReferencedBy(string foreignKeyName, string foreignTableName, string foreignColumnName)
        {
            return ReferencedBy(foreignKeyName, null, foreignTableName, foreignColumnName);
        }

        public IAlterTableColumnOptionOrAddColumnOrAlterColumnOrForeignKeyCascadeSyntax ReferencedBy(string foreignKeyName, string foreignTableSchema, string foreignTableName, string foreignColumnName)
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


        public IAlterTableColumnOptionOrAddColumnOrAlterColumnSyntax ForeignKey()
        {
            CurrentColumn.IsForeignKey = true;
            return this;
        }

        [Obsolete("Please use ReferencedBy syntax. This method will be removed in the next version")]
        public IAlterTableColumnOptionOrAddColumnOrAlterColumnSyntax References(string foreignKeyName, string foreignTableName, IEnumerable<string> foreignColumnNames)
        {
            return References(foreignKeyName, null, foreignTableName, foreignColumnNames);
        }

        [Obsolete("Please use ReferencedBy syntax. This method will be removed in the next version")]
        public IAlterTableColumnOptionOrAddColumnOrAlterColumnSyntax References(string foreignKeyName, string foreignTableSchema, string foreignTableName, IEnumerable<string> foreignColumnNames)
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

        public IAlterTableColumnOptionOrAddColumnOrAlterColumnOrForeignKeyCascadeSyntax OnDelete(System.Data.Rule rule)
        {
            CurrentForeignKey.OnDelete = rule;
            return this;
        }

        public IAlterTableColumnOptionOrAddColumnOrAlterColumnOrForeignKeyCascadeSyntax OnUpdate(System.Data.Rule rule)
        {
            CurrentForeignKey.OnUpdate = rule;
            return this;
        }

        public IAlterTableColumnOptionOrAddColumnOrAlterColumnSyntax OnDeleteOrUpdate(System.Data.Rule rule)
        {
            OnDelete(rule);
            OnUpdate(rule);
            return this;
        }

        protected override ColumnDefinition GetColumnForType()
        {
            return CurrentColumn;
        }
    }
}