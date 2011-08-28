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

namespace FluentMigrator.Builders.Create.Column
{
	public class CreateColumnExpressionBuilder : ExpressionBuilderWithColumnTypesBase<CreateColumnExpression, ICreateColumnOptionOrForeignKeySyntax>,
		ICreateColumnOnTableSyntax,
		ICreateColumnAsTypeOrInSchemaSyntax,
		ICreateColumnOptionOrForeignKeySyntax,
		ICreateColumnOptionOrForeignKeyCascadeSyntax
	{
		public ForeignKeyDefinition CurrentForeignKey { get; set; }
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

		public ICreateColumnOptionOrForeignKeySyntax WithDefaultValue(object value)
		{
			Expression.Column.DefaultValue = value;
			return this;
		}

		public ICreateColumnOptionOrForeignKeySyntax Identity()
		{
			Expression.Column.IsIdentity = true;
			return this;
		}

		public ICreateColumnOptionOrForeignKeySyntax Indexed()
		{
			Expression.Column.IsIndexed = true;
			return this;
		}

		public ICreateColumnOptionOrForeignKeySyntax PrimaryKey()
		{
			Expression.Column.IsPrimaryKey = true;
			return this;
		}

		public ICreateColumnOptionOrForeignKeySyntax PrimaryKey(string primaryKeyName)
		{
			Expression.Column.IsPrimaryKey = true;
			Expression.Column.PrimaryKeyName = primaryKeyName;
			return this;
		}

		public ICreateColumnOptionOrForeignKeySyntax Nullable()
		{
			Expression.Column.IsNullable = true;
			return this;
		}

		public ICreateColumnOptionOrForeignKeySyntax NotNullable()
		{
			Expression.Column.IsNullable = false;
			return this;
		}

		public ICreateColumnOptionOrForeignKeySyntax Unique()
		{
			Expression.Column.IsUnique = true;
			return this;
		}

		public ICreateColumnOptionOrForeignKeyCascadeSyntax ForeignKey(string primaryTableName, string primaryColumnName)
		{
			return ForeignKey(null, null, primaryTableName, primaryColumnName);
		}

		public ICreateColumnOptionOrForeignKeyCascadeSyntax ForeignKey(string foreignKeyName, string primaryTableName, string primaryColumnName)
		{
			return ForeignKey(foreignKeyName, null, primaryTableName, primaryColumnName);
		}

		public ICreateColumnOptionOrForeignKeyCascadeSyntax ForeignKey(string foreignKeyName, string primaryTableSchema, string primaryTableName, string primaryColumnName)
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

			fk.ForeignKey.ForeignColumns.Add(Expression.Column.Name);
			fk.ForeignKey.PrimaryColumns.Add(primaryColumnName);

			_context.Expressions.Add(fk);
			CurrentForeignKey = fk.ForeignKey;
			return this;
		}

		public ICreateColumnOptionOrForeignKeyCascadeSyntax ReferencedBy(string foreignTableName, string foreignColumnName)
		{
			return ReferencedBy(null, foreignTableName, foreignColumnName);
		}

		public ICreateColumnOptionOrForeignKeyCascadeSyntax ReferencedBy(string foreignKeyName, string foreignTableName, string foreignColumnName)
		{
			return ReferencedBy(foreignKeyName, null, foreignTableName, foreignColumnName);
		}

		public ICreateColumnOptionOrForeignKeyCascadeSyntax ReferencedBy(string foreignKeyName, string foreignTableSchema, string foreignTableName, string foreignColumnName)
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

		public ICreateColumnOptionOrForeignKeyCascadeSyntax OnDelete(Rule rule)
		{
			CurrentForeignKey.OnDelete = rule;
			return this;
		}

		public ICreateColumnOptionOrForeignKeyCascadeSyntax OnUpdate(Rule rule)
		{
			CurrentForeignKey.OnUpdate = rule;
			return this;
		}

		public ICreateColumnOptionOrForeignKeyCascadeSyntax OnDeleteOrUpdate(Rule rule)
		{
			CurrentForeignKey.OnDelete = rule;
			CurrentForeignKey.OnUpdate = rule;
			return this;
		}

		protected override ColumnDefinition GetColumnForType()
		{
			return Expression.Column;
		}
	}
}