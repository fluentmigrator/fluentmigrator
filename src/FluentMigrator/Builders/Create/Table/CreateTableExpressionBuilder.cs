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
using System.Data;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Model;

namespace FluentMigrator.Builders.Create.Table
{
    public class CreateTableExpressionBuilder : ExpressionBuilderWithColumnTypesBase<CreateTableExpression, ICreateTableColumnOptionOrWithColumnOrForeignKeySyntax>,
		ICreateTableWithColumnOrSchemaSyntax,
		ICreateTableColumnAsTypeSyntax,
        ICreateTableColumnOptionOrWithColumnOrForeignKeySyntax,
        ICreateTableColumnOptionOrWithColumnOrForeignKeyCascadeSyntax
	{
		public ColumnDefinition CurrentColumn { get; set; }
        public ForeignKeyDefinition CurrentForeignKey { get; set; }
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

        public ICreateTableColumnOptionOrWithColumnOrForeignKeySyntax WithDefaultValue(object value)
		{
			CurrentColumn.DefaultValue = value;
			return this;
		}

        public ICreateTableColumnOptionOrWithColumnOrForeignKeySyntax Identity()
		{
			CurrentColumn.IsIdentity = true;
			return this;
		}

        public ICreateTableColumnOptionOrWithColumnOrForeignKeySyntax Indexed()
		{
			CurrentColumn.IsIndexed = true;
			return this;
		}

        public ICreateTableColumnOptionOrWithColumnOrForeignKeySyntax PrimaryKey()
		{
			CurrentColumn.IsPrimaryKey = true;
			return this;
		}

        public ICreateTableColumnOptionOrWithColumnOrForeignKeySyntax PrimaryKey(string primaryKeyName)
		{
			CurrentColumn.IsPrimaryKey = true;
			CurrentColumn.PrimaryKeyName = primaryKeyName;
			return this;
		}

        public ICreateTableColumnOptionOrWithColumnOrForeignKeySyntax Nullable()
		{
			CurrentColumn.IsNullable = true;
			return this;
		}

        public ICreateTableColumnOptionOrWithColumnOrForeignKeySyntax NotNullable()
		{
			CurrentColumn.IsNullable = false;
			return this;
		}

        public ICreateTableColumnOptionOrWithColumnOrForeignKeySyntax Unique()
		{
			CurrentColumn.IsUnique = true;
			return this;
        }

        public ICreateTableColumnOptionOrWithColumnOrForeignKeyCascadeSyntax ForeignKey(string primaryTableName, string primaryColumnName)
        {
            return ForeignKey(null, null, primaryTableName, primaryColumnName);
        }

        public ICreateTableColumnOptionOrWithColumnOrForeignKeyCascadeSyntax ForeignKey(string foreignKeyName, string primaryTableName, string primaryColumnName)
        {
            return ForeignKey(foreignKeyName, null, primaryTableName, primaryColumnName);
        }

        public ICreateTableColumnOptionOrWithColumnOrForeignKeyCascadeSyntax ForeignKey(string foreignKeyName, string primaryTableSchema, string primaryTableName, string primaryColumnName)
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

            fk.ForeignKey.ForeignColumns.Add(CurrentColumn.Name);
            fk.ForeignKey.PrimaryColumns.Add(primaryColumnName);

            _context.Expressions.Add(fk);
            CurrentForeignKey = fk.ForeignKey;
            return this;
        }

        public ICreateTableColumnOptionOrWithColumnOrForeignKeyCascadeSyntax ReferencedBy(string foreignTableName, string foreignColumnName)
        {
            return ReferencedBy(null, foreignTableName, foreignColumnName);
        }

        public ICreateTableColumnOptionOrWithColumnOrForeignKeyCascadeSyntax ReferencedBy(string foreignKeyName, string foreignTableName, string foreignColumnName)
        {
            return ReferencedBy(foreignKeyName, null, foreignTableName, foreignColumnName);
        }

        public ICreateTableColumnOptionOrWithColumnOrForeignKeyCascadeSyntax ReferencedBy(string foreignKeyName, string foreignTableSchema, string foreignTableName, string foreignColumnName)
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

        public ICreateTableColumnOptionOrWithColumnOrForeignKeyCascadeSyntax OnDelete(Rule rule)
        {
            CurrentForeignKey.OnDelete = rule;
            return this;
        }

        public ICreateTableColumnOptionOrWithColumnOrForeignKeyCascadeSyntax OnUpdate(Rule rule)
        {
            CurrentForeignKey.OnUpdate = rule;
            return this;
        }

        public ICreateTableColumnOptionOrWithColumnOrForeignKeyCascadeSyntax OnDeleteOrUpdate(Rule rule)
        {
            CurrentForeignKey.OnDelete = rule;
            CurrentForeignKey.OnUpdate = rule;
            return this;
        }

        protected override ColumnDefinition GetColumnForType()
        {
            return CurrentColumn;
        }
    }
}