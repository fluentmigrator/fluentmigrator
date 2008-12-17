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

		public ICreateTableColumnOptionOrWithColumnSyntax AsAnsiString(int size)
		{
			Expression.CurrentColumn.Type = DbType.AnsiString;
            Expression.CurrentColumn.Size = size;
			return this;
		}

		public ICreateTableColumnOptionOrWithColumnSyntax AsBinary(int size)
		{
			Expression.CurrentColumn.Type = DbType.Binary;
            Expression.CurrentColumn.Size = size;
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

		public ICreateTableColumnOptionOrWithColumnSyntax AsDecimal(int size, int precision)
		{
			Expression.CurrentColumn.Type = DbType.Decimal;
            Expression.CurrentColumn.Size = size;
		    Expression.CurrentColumn.Precision = precision;
			return this;
		}

		public ICreateTableColumnOptionOrWithColumnSyntax AsDouble()
		{
			Expression.CurrentColumn.Type = DbType.Double;
			return this;
		}

        public ICreateTableColumnOptionOrWithColumnSyntax AsFixedLengthString(int size)
		{
			Expression.CurrentColumn.Type = DbType.StringFixedLength;
            Expression.CurrentColumn.Size = size;
			return this;
		}

        public ICreateTableColumnOptionOrWithColumnSyntax AsFixedLengthAnsiString(int size)
		{
			Expression.CurrentColumn.Type = DbType.AnsiStringFixedLength;
            Expression.CurrentColumn.Size = size;
			return this;
		}

		public ICreateTableColumnOptionOrWithColumnSyntax AsFloat()
		{
			Expression.CurrentColumn.Type = DbType.Single;
			return this;
		}

        public ICreateTableColumnOptionOrWithColumnSyntax AsGuid()
        {
            Expression.CurrentColumn.Type = DbType.Guid;
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

		public ICreateTableColumnOptionOrWithColumnSyntax AsString(int size)
		{
			Expression.CurrentColumn.Type = DbType.String;
            Expression.CurrentColumn.Size = size;
			return this;
		}

		public ICreateTableColumnOptionOrWithColumnSyntax AsTime()
		{
			Expression.CurrentColumn.Type = DbType.Time;
			return this;
		}

		public ICreateTableColumnOptionOrWithColumnSyntax AsXml(int size)
		{
			Expression.CurrentColumn.Type = DbType.Xml;
		    Expression.CurrentColumn.Size = size;
			return this;
		}

		public ICreateTableColumnAsTypeSyntax WithColumn(string name)
		{
			var column = new ColumnDefinition {Name = name};
			Expression.Columns.Add(column);
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