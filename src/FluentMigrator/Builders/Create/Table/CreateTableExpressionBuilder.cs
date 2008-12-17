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

		public ICreateTableColumnOptionOrWithColumnSyntax AsAnsiString()
		{
			Expression.CurrentColumn.Type = DbType.AnsiString;
			return this;
		}

		public ICreateTableColumnOptionOrWithColumnSyntax AsBinary()
		{
			Expression.CurrentColumn.Type = DbType.Binary;
			return this;
		}

		public ICreateTableColumnOptionOrWithColumnSyntax AsBoolean()
		{
			Expression.CurrentColumn.Type = DbType.Boolean;
			return this;
		}

		public ICreateTableColumnOptionOrWithColumnSyntax AsByte()
		{
			Expression.CurrentColumn.Type = DbType.Byte;
			return this;
		}

		public ICreateTableColumnOptionOrWithColumnSyntax AsCurrency()
		{
			Expression.CurrentColumn.Type = DbType.Currency;
			return this;
		}

		public ICreateTableColumnOptionOrWithColumnSyntax AsDate()
		{
			Expression.CurrentColumn.Type = DbType.Date;
			return this;
		}

		public ICreateTableColumnOptionOrWithColumnSyntax AsDateTime()
		{
			Expression.CurrentColumn.Type = DbType.DateTime;
			return this;
		}

		public ICreateTableColumnOptionOrWithColumnSyntax AsDecimal()
		{
			Expression.CurrentColumn.Type = DbType.Decimal;
			return this;
		}

		public ICreateTableColumnOptionOrWithColumnSyntax AsDouble()
		{
			Expression.CurrentColumn.Type = DbType.Double;
			return this;
		}

		public ICreateTableColumnOptionOrWithColumnSyntax AsFixedLengthString()
		{
			Expression.CurrentColumn.Type = DbType.StringFixedLength;
			return this;
		}

		public ICreateTableColumnOptionOrWithColumnSyntax AsFixedLengthAnsiString()
		{
			Expression.CurrentColumn.Type = DbType.AnsiStringFixedLength;
			return this;
		}

		public ICreateTableColumnOptionOrWithColumnSyntax AsFloat()
		{
			Expression.CurrentColumn.Type = DbType.Single;
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

		public ICreateTableColumnOptionOrWithColumnSyntax AsTime()
		{
			Expression.CurrentColumn.Type = DbType.Time;
			return this;
		}

		public ICreateTableColumnOptionOrWithColumnSyntax AsXml()
		{
			Expression.CurrentColumn.Type = DbType.Xml;
			return this;
		}

		public ICreateTableColumnAsTypeSyntax WithColumn(string name)
		{
			var column = new ColumnDefinition {Name = name};
			Expression.Columns.Add(column);
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