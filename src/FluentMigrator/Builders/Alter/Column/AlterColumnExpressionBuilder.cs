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

namespace FluentMigrator.Builders.Alter.Column
{
	public class AlterColumnExpressionBuilder : ExpressionBuilderWithColumnTypesBase<AlterColumnExpression, IAlterColumnOptionSyntax>,
		IAlterColumnOnTableSyntax,
		IAlterColumnOptionSyntax,
		IAlterColumnAsTypeSyntax
	{
		private readonly IMigrationContext _context;

		public AlterColumnExpressionBuilder( AlterColumnExpression expression, IMigrationContext context )
			: base( expression )
		{
			_context = context;
		}

		public IAlterColumnAsTypeSyntax OnTable( string name )
		{
			Expression.TableName = name;
			return this;
		}

		public IAlterColumnOptionSyntax InSchema( string schemaName )
		{
			Expression.SchemaName = schemaName;
			return this;
		}

		public IAlterColumnOptionSyntax WithDefaultValue( object value )
		{
			// we need to do a drop constraint and then add constraint to change the defualt value
			var dc = new AlterDefaultConstraintExpression
						 {
							 TableName = Expression.TableName,
							 SchemaName = Expression.SchemaName,
							 ColumnName = Expression.Column.Name,
							 DefaultValue = value
						 };

			_context.Expressions.Add( dc );

			return this;
		}

		public IAlterColumnOptionSyntax ForeignKey()
		{
			Expression.Column.IsForeignKey = true;
			return this;
		}

		public IAlterColumnOptionSyntax Identity()
		{
			Expression.Column.IsIdentity = true;
			return this;
		}

		public IAlterColumnOptionSyntax Indexed()
		{
			Expression.Column.IsIndexed = true;
			return this;
		}

		public IAlterColumnOptionSyntax PrimaryKey()
		{
			Expression.Column.IsPrimaryKey = true;
			return this;
		}

		public IAlterColumnOptionSyntax PrimaryKey( string primaryKeyName )
		{
			Expression.Column.IsPrimaryKey = true;
			Expression.Column.PrimaryKeyName = primaryKeyName;
			return this;
		}

		public IAlterColumnOptionSyntax Nullable()
		{
			Expression.Column.IsNullable = true;
			return this;
		}

		public IAlterColumnOptionSyntax NotNullable()
		{
			Expression.Column.IsNullable = false;
			return this;
		}

		public IAlterColumnOptionSyntax Unique()
		{
			Expression.Column.IsUnique = true;
			return this;
		}

		public IAlterColumnOptionSyntax References( string foreignKeyName, string foreignTableName, IEnumerable<string> foreignColumnNames )
		{
			return References( foreignKeyName, null, foreignTableName, foreignColumnNames );
		}

		public IAlterColumnOptionSyntax References( string foreignKeyName, string foreignTableSchema, string foreignTableName, IEnumerable<string> foreignColumnNames )
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

			fk.ForeignKey.PrimaryColumns.Add( Expression.Column.Name );
			foreach ( var foreignColumnName in foreignColumnNames )
				fk.ForeignKey.ForeignColumns.Add( foreignColumnName );

			_context.Expressions.Add( fk );
			return this;
		}
		protected override ColumnDefinition GetColumnForType()
		{
			return Expression.Column;
		}
	}
}
