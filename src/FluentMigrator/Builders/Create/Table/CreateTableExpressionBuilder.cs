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

using System;
using System.Data;
using FluentMigrator.Expressions;
using FluentMigrator.Model;

namespace FluentMigrator.Builders.Create.Table
{
	public class CreateTableExpressionBuilder : ExpressionBuilderBase<CreateTableExpression>,
		ICreateTableColumnAsTypeSyntax, ICreateTableColumnOptionOrWithColumnSyntax
	{
		public ColumnDefinition CurrentColumn { get; set; }

		public CreateTableExpressionBuilder(CreateTableExpression expression)
			: base(expression)
		{
		}

		public ICreateTableColumnAsTypeSyntax WithColumn(string name)
		{
			var column = new ColumnDefinition { Name = name };
			Expression.Columns.Add(column);
			CurrentColumn = column;
			return this;
		}

		public ICreateTableColumnOptionOrWithColumnSyntax AsAnsiString()
		{
			CurrentColumn.Type = DbType.AnsiString;
			return this;
		}

		public ICreateTableColumnOptionOrWithColumnSyntax AsAnsiString(int size)
		{
			CurrentColumn.Type = DbType.AnsiString;
			CurrentColumn.Size = size;
			return this;
		}

		public ICreateTableColumnOptionOrWithColumnSyntax AsBinary(int size)
		{
			CurrentColumn.Type = DbType.Binary;
			CurrentColumn.Size = size;
			return this;
		}

		public ICreateTableColumnOptionOrWithColumnSyntax AsBoolean()
		{
			CurrentColumn.Type = DbType.Boolean;
			return this;
		}

		public ICreateTableColumnOptionOrWithColumnSyntax AsByte()
		{
			CurrentColumn.Type = DbType.Byte;
			return this;
		}

		public ICreateTableColumnOptionOrWithColumnSyntax AsCurrency()
		{
			CurrentColumn.Type = DbType.Currency;
			return this;
		}

		public ICreateTableColumnOptionOrWithColumnSyntax AsDate()
		{
			CurrentColumn.Type = DbType.Date;
			return this;
		}

		public ICreateTableColumnOptionOrWithColumnSyntax AsDateTime()
		{
			CurrentColumn.Type = DbType.DateTime;
			return this;
		}

		public ICreateTableColumnOptionOrWithColumnSyntax AsDecimal()
		{
			CurrentColumn.Type = DbType.Decimal;
			return this;
		}

		public ICreateTableColumnOptionOrWithColumnSyntax AsDecimal(int size, int precision)
		{
			CurrentColumn.Type = DbType.Decimal;
			CurrentColumn.Size = size;
			CurrentColumn.Precision = precision;
			return this;
		}

		public ICreateTableColumnOptionOrWithColumnSyntax AsDouble()
		{
			CurrentColumn.Type = DbType.Double;
			return this;
		}

		public ICreateTableColumnOptionOrWithColumnSyntax AsFixedLengthString(int size)
		{
			CurrentColumn.Type = DbType.StringFixedLength;
			CurrentColumn.Size = size;
			return this;
		}

		public ICreateTableColumnOptionOrWithColumnSyntax AsFixedLengthAnsiString(int size)
		{
			CurrentColumn.Type = DbType.AnsiStringFixedLength;
			CurrentColumn.Size = size;
			return this;
		}

		public ICreateTableColumnOptionOrWithColumnSyntax AsFloat()
		{
			CurrentColumn.Type = DbType.Single;
			return this;
		}

		public ICreateTableColumnOptionOrWithColumnSyntax AsGuid()
		{
			CurrentColumn.Type = DbType.Guid;
			return this;
		}

		public ICreateTableColumnOptionOrWithColumnSyntax AsInt16()
		{
			CurrentColumn.Type = DbType.Int16;
			return this;
		}

		public ICreateTableColumnOptionOrWithColumnSyntax AsInt32()
		{
			CurrentColumn.Type = DbType.Int32;
			return this;
		}

		public ICreateTableColumnOptionOrWithColumnSyntax AsInt64()
		{
			CurrentColumn.Type = DbType.Int64;
			return this;
		}

		public ICreateTableColumnOptionOrWithColumnSyntax AsString()
		{
			CurrentColumn.Type = DbType.String;
			return this;
		}

		public ICreateTableColumnOptionOrWithColumnSyntax AsString(int size)
		{
			CurrentColumn.Type = DbType.String;
			CurrentColumn.Size = size;
			return this;
		}

		public ICreateTableColumnOptionOrWithColumnSyntax AsTime()
		{
			CurrentColumn.Type = DbType.Time;
			return this;
		}

		public ICreateTableColumnOptionOrWithColumnSyntax AsXml()
		{
			CurrentColumn.Type = DbType.Xml;
			return this;
		}

		public ICreateTableColumnOptionOrWithColumnSyntax AsXml(int size)
		{
			CurrentColumn.Type = DbType.Xml;
			CurrentColumn.Size = size;
			return this;
		}

		public ICreateTableColumnOptionOrWithColumnSyntax AsCustom(string customType)
		{
			CurrentColumn.Type = null;
		    CurrentColumn.CustomType = customType;
			return this;
		}

		public ICreateTableColumnOptionOrWithColumnSyntax WithDefaultValue(object value)
		{
			CurrentColumn.DefaultValue = value;
			return this;
		}

		public ICreateTableColumnOptionOrWithColumnSyntax ForeignKey()
		{
			CurrentColumn.IsForeignKey = true;
			return this;
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
	}
}