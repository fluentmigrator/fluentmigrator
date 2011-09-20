#region License
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

namespace FluentMigrator.Builders.Create.Table
{
    public class CreateTableExpressionBuilder : ExpressionBuilderWithColumnTypesBase<CreateTableExpression, ICreateTableColumnOptionOrWithColumnSyntax>,
        ICreateTableWithColumnOrSchemaSyntax,
        ICreateTableColumnAsTypeSyntax,
        ICreateTableColumnOptionOrWithColumnSyntax
    {
        public ColumnDefinition CurrentColumn { get; set; }
        private readonly IMigrationContext _context;

        public CreateTableExpressionBuilder(CreateTableExpression expression, IMigrationContext context)
            : base(expression)
        {
            _context = context;
        }

        public ICreateTableWithColumnSyntax InSchema(string schemaName)
        {
            Expression.SchemaName = schemaName;
            return this;
        }

        public ICreateTableColumnAsTypeSyntax WithColumn(string name)
        {
            var column = new ColumnDefinition { Name = name, TableName = Expression.TableName };
            Expression.Columns.Add(column);
            CurrentColumn = column;
            return this;
        }

        public ICreateTableColumnOptionOrWithColumnSyntax WithDefaultValue(object value)
        {
            CurrentColumn.DefaultValue = value;
            return this;
        }

        public ICreateTableColumnOptionOrWithColumnSyntax WithDefaultExpression(string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("Argument should not be empty", "value");

            var result = WithDefaultValue(new ExpressionString(value));
            return result;
        }

        public ICreateTableColumnOptionOrWithColumnSyntax WithDefaultGuid()
        {
            var result = WithDefaultValue(SystemMethods.NewGuid);
            return result;
        }

        public ICreateTableColumnOptionOrWithColumnSyntax WithDefaultCurrentDateTime()
        {
            var result = WithDefaultValue(SystemMethods.CurrentDateTime);
            return result;
        }

        public ICreateTableColumnOptionOrWithColumnSyntax Identity()
        {
            CurrentColumn.IsIdentity = true;
            return this;
        }

        public ICreateTableColumnOptionOrWithColumnSyntax Indexed()
        {
            CurrentColumn.IsIndexed = true;
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
            CurrentColumn.IsNullable = true;
            return this;
        }

        public ICreateTableColumnOptionOrWithColumnSyntax NotNullable()
        {
            CurrentColumn.IsNullable = false;
            return this;
        }

        public ICreateTableColumnOptionOrWithColumnSyntax Unique()
        {
            CurrentColumn.IsUnique = true;
            return this;
        }

        public ICreateTableColumnOptionOrWithColumnSyntax ForeignKey(string primaryTableName, string primaryColumnName)
        {
            return ForeignKey(null, null, primaryTableName, primaryColumnName);
        }

        public ICreateTableColumnOptionOrWithColumnSyntax ForeignKey(string foreignKeyName, string primaryTableName, string primaryColumnName)
        {
            return ForeignKey(foreignKeyName, null, primaryTableName, primaryColumnName);
        }

        public ICreateTableColumnOptionOrWithColumnSyntax ForeignKey(string foreignKeyName, string primaryTableSchema, string primaryTableName, string primaryColumnName)
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
            return this;
        }

        public ICreateTableColumnOptionOrWithColumnSyntax ReferencedBy(string foreignTableName, string foreignColumnName)
        {
            return ReferencedBy(null, null, foreignTableName, foreignColumnName);
        }

        public ICreateTableColumnOptionOrWithColumnSyntax ReferencedBy(string foreignKeyName, string foreignTableName, string foreignColumnName)
        {
            return ReferencedBy(foreignKeyName, null, foreignTableName, foreignColumnName);
        }

        public ICreateTableColumnOptionOrWithColumnSyntax ReferencedBy(string foreignKeyName, string foreignTableSchema, string foreignTableName, string foreignColumnName)
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
            return this;
        }

        [Obsolete("Use ForeignKey(string,string)")]
        public ICreateTableColumnOptionOrWithColumnSyntax ForeignKey()
        {
            CurrentColumn.IsForeignKey = true;
            return this;
        }

        [Obsolete("Use ReferencedBy")]
        public ICreateTableColumnOptionOrWithColumnSyntax References(string foreignKeyName, string foreignTableName, IEnumerable<string> foreignColumnNames)
        {
            return References(foreignKeyName, null, foreignTableName, foreignColumnNames);
        }

        [Obsolete("Use ReferencedBy")]
        public ICreateTableColumnOptionOrWithColumnSyntax References(string foreignKeyName, string foreignTableSchema, string foreignTableName, IEnumerable<string> foreignColumnNames)
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

        protected override ColumnDefinition GetColumnForType()
        {
            return CurrentColumn;
        }
    }
}