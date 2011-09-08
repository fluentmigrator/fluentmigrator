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
using System.Linq;
using System;

namespace FluentMigrator.Builders.Create.Column
{
	public class CreateColumnExpressionBuilder : ExpressionBuilderWithColumnTypesBase<CreateColumnExpression, ICreateColumnOptionSyntax>,
		ICreateColumnOnTableSyntax,
		ICreateColumnOptionSyntax,
		ICreateColumnAsTypeOrInSchemaSyntax
	{
		private readonly IMigrationContext _context;

		public CreateColumnExpressionBuilder(CreateColumnExpression expression, IMigrationContext context)
			: base(expression)
		{
			_context = context;
		}

		public ICreateColumnAsTypeOrInSchemaSyntax OnTable(string name)
		{
			Expression.TableName = name;
			return this;
		}

		public ICreateColumnAsTypeSyntax InSchema(string schemaName)
		{
			Expression.SchemaName = schemaName;
			return this;
		}

		public ICreateColumnOptionSyntax WithDefaultValue(object value)
		{
			Expression.Column.DefaultValue = value;
			return this;
		}

		public ICreateColumnOptionSyntax Identity()
		{
			Expression.Column.IsIdentity = true;
			return this;
		}

		public ICreateColumnOptionSyntax Indexed()
		{
			Expression.Column.IsIndexed = true;
			return this;
		}

		public ICreateColumnOptionSyntax PrimaryKey()
		{
			Expression.Column.IsPrimaryKey = true;
			return this;
		}

		public ICreateColumnOptionSyntax PrimaryKey(string primaryKeyName)
		{
			Expression.Column.IsPrimaryKey = true;
			Expression.Column.PrimaryKeyName = primaryKeyName;
			return this;
		}

		public ICreateColumnOptionSyntax Nullable()
		{
			Expression.Column.IsNullable = true;
			return this;
		}

		public ICreateColumnOptionSyntax NotNullable()
		{
			Expression.Column.IsNullable = false;
			return this;
		}

		public ICreateColumnOptionSyntax Unique()
		{
			Expression.Column.IsUnique = true;
			return this;
        }

        public ICreateColumnOptionSyntax ForeignKey(string primaryTableName, string primaryColumnName)
        {
            return ForeignKey(null, null, primaryTableName, primaryColumnName);
        }

        public ICreateColumnOptionSyntax ForeignKey(string foreignKeyName, string primaryTableName, string primaryColumnName)
        {
            return ForeignKey(foreignKeyName, null, primaryTableName, primaryColumnName);
        }

        public ICreateColumnOptionSyntax ForeignKey(string foreignKeyName, string primaryTableSchema, string primaryTableName, string primaryColumnName)
        {
            Expression.Column.IsForeignKey = true;

            var fk = new CreateForeignKeyExpression
            {
                ForeignKey = new ForeignKeyDefinition
                {
                    Name = foreignKeyName,
                    TableContainingPrimayKey = primaryTableName,
                    SchemaOfTableContainingPrimaryKey = primaryTableSchema,
                    TableContainingForeignKey = Expression.TableName,
                    SchemaOfTableContainingForeignKey = Expression.SchemaName
                }
            };
            fk.ForeignKey.ColumnsInPrimaryKeyTableToInclude.Add(primaryColumnName);
            fk.ForeignKey.ColumnsInForeignKeyTableToInclude.Add(Expression.Column.Name);

            _context.Expressions.Add(fk);
            return this;
        }

        public ICreateColumnOptionSyntax ReferencedBy(string foreignTableName, string foreignColumnName)
        {
            return ReferencedBy(null, null, foreignTableName, foreignColumnName);
        }

        public ICreateColumnOptionSyntax ReferencedBy(string foreignKeyName, string foreignTableName, string foreignColumnName)
        {
            return ReferencedBy(foreignKeyName, null, foreignTableName, foreignColumnName);
        }

        public ICreateColumnOptionSyntax ReferencedBy(string foreignKeyName, string foreignTableSchema, string foreignTableName, string foreignColumnName)
        {
            var fk = new CreateForeignKeyExpression
            {
                ForeignKey = new ForeignKeyDefinition {
                    Name = foreignKeyName,
                    TableContainingPrimayKey = Expression.TableName,
                    SchemaOfTableContainingPrimaryKey = Expression.SchemaName,
                    TableContainingForeignKey = foreignTableName,
                    SchemaOfTableContainingForeignKey = foreignTableSchema
                }
            };
            fk.ForeignKey.ColumnsInPrimaryKeyTableToInclude.Add(Expression.Column.Name);
            fk.ForeignKey.ColumnsInForeignKeyTableToInclude.Add(foreignColumnName);

            _context.Expressions.Add(fk);
            return this;
        }

        [Obsolete("Use ForeignKey(string,string)")]
        public ICreateColumnOptionSyntax ForeignKey()
        {
            Expression.Column.IsForeignKey = true;
            return this;
        }

        [Obsolete("Use ReferencedBy")]
        public ICreateColumnOptionSyntax References(string foreignKeyName, string foreignTableName, IEnumerable<string> foreignColumnNames)
        {
            return References(foreignKeyName, null, foreignTableName, foreignColumnNames);
        }

        [Obsolete("Use ReferencedBy")]
        public ICreateColumnOptionSyntax References(string foreignKeyName, string foreignTableSchema, string foreignTableName, IEnumerable<string> foreignColumnNames)
        {
            return ReferencedBy(foreignKeyName, foreignTableSchema, foreignTableName, foreignColumnNames.First());
        }

		protected override ColumnDefinition GetColumnForType()
		{
			return Expression.Column;
		}
    }
}