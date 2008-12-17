using System;
using System.Data;
using FluentMigrator.Expressions;
using FluentMigrator.Model;

namespace FluentMigrator.Builders.Create.Table
{
	public class CreateTableExpressionBuilder : ExpressionBuilderBase<CreateTableExpression>,
		ICreateTableColumnAsTypeSyntax, ICreateTableColumnOptionOrWithColumnSyntax
	{
		public CreateTableExpressionBuilder(CreateTableExpression expression)
			: base(expression)
		{
		}

		public ICreateTableColumnAsTypeSyntax WithColumn(string name)
		{
			var column = new ColumnDefinition(name);
			Expression.Columns.Add(column);
			return this;
		}

		public ICreateTableColumnOptionOrWithColumnSyntax AsInt16()
		{
			Expression.CurrentColumn.Type = DbType.Int16;
			return this;
		}

		public ICreateTableColumnOptionOrWithColumnSyntax AsInt32()
		{
			Expression.CurrentColumn.Type = DbType.Int32;
			return this;
		}

		public ICreateTableColumnOptionOrWithColumnSyntax AsInt64()
		{
			Expression.CurrentColumn.Type = DbType.Int64;
			return this;
		}

		public ICreateTableColumnOptionOrWithColumnSyntax AsString()
		{
			Expression.CurrentColumn.Type = DbType.String;
			return this;
		}

		public ICreateTableColumnOptionOrWithColumnSyntax AsFixedLengthString()
		{
			Expression.CurrentColumn.Type = DbType.StringFixedLength;
			return this;
		}

		public ICreateTableColumnOptionOrWithColumnSyntax WithSize(int size)
		{
			Expression.CurrentColumn.Size = size;
			return this;
		}

		public ICreateTableColumnOptionOrWithColumnSyntax WithDefaultValue(object value)
		{
			Expression.CurrentColumn.DefaultValue = value;
			return this;
		}

		public ICreateTableColumnOptionOrWithColumnSyntax ForeignKey()
		{
			Expression.CurrentColumn.IsForeignKey = true;
			return this;
		}

		public ICreateTableColumnOptionOrWithColumnSyntax Identity()
		{
			Expression.CurrentColumn.IsIdentity = true;
			return this;
		}

		public ICreateTableColumnOptionOrWithColumnSyntax Indexed()
		{
			Expression.CurrentColumn.IsIndexed = true;
			return this;
		}

		public ICreateTableColumnOptionOrWithColumnSyntax PrimaryKey()
		{
			Expression.CurrentColumn.IsPrimaryKey = true;
			return this;
		}

		public ICreateTableColumnOptionOrWithColumnSyntax Nullable()
		{
			Expression.CurrentColumn.IsNullable = true;
			return this;
		}

		public ICreateTableColumnOptionOrWithColumnSyntax NotNullable()
		{
			Expression.CurrentColumn.IsNullable = false;
			return this;
		}

		public ICreateTableColumnOptionOrWithColumnSyntax Unique()
		{
			Expression.CurrentColumn.IsUnique = true;
			return this;
		}
	}
}