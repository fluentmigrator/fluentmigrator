using System;
using System.Data;
using FluentMigrator.Expressions;

namespace FluentMigrator.Builders.Create.Column
{
	public class CreateColumnExpressionBuilder : ExpressionBuilderBase<CreateColumnExpression>,
		ICreateColumnOnTableSyntax, ICreateColumnAsTypeSyntax, ICreateColumnOptionSyntax
	{
		public CreateColumnExpressionBuilder(CreateColumnExpression expression)
			: base(expression)
		{
		}

		public ICreateColumnAsTypeSyntax OnTable(string name)
		{
			Expression.TableName = name;
			return this;
		}

		public ICreateColumnOptionSyntax AsInt16()
		{
			Expression.Column.Type = DbType.Int16;
			return this;
		}

		public ICreateColumnOptionSyntax AsInt32()
		{
			Expression.Column.Type = DbType.Int32;
			return this;
		}

		public ICreateColumnOptionSyntax AsInt64()
		{
			Expression.Column.Type = DbType.Int64;
			return this;
		}

		public ICreateColumnOptionSyntax AsString()
		{
			Expression.Column.Type = DbType.String;
			return this;
		}

		public ICreateColumnOptionSyntax AsFixedLengthString()
		{
			Expression.Column.Type = DbType.StringFixedLength;
			return this;
		}

		public ICreateColumnOptionSyntax WithSize(int size)
		{
			Expression.Column.Size = size;
			return this;
		}

		public ICreateColumnOptionSyntax WithDefaultValue(object value)
		{
			Expression.Column.DefaultValue = value;
			return this;
		}

		public ICreateColumnOptionSyntax ForeignKey()
		{
			Expression.Column.IsForeignKey = true;
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
	}
}