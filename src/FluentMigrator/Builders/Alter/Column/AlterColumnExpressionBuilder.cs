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
    public class AlterColumnExpressionBuilder : ExpressionBuilderBase<AlterColumnExpression>,
        IAlterColumnOnTableSyntax,
        IAlterColumnOptionSyntax,
        IAlterColumnAsTypeSyntax
    {
        private readonly IMigrationContext _context;

        public AlterColumnExpressionBuilder(AlterColumnExpression expression, IMigrationContext context)
			: base(expression)
		{
			_context = context;
		}

        public IAlterColumnAsTypeSyntax OnTable(string name)
        {
            Expression.TableName = name;
            return this;
        }

        public IAlterColumnOptionSyntax InSchema(string schemaName)
        {
            Expression.SchemaName = schemaName;
            return this;
        }

        public IAlterColumnOptionSyntax AsAnsiString()
        {
            Expression.Column.Type = DbType.AnsiString;
            return this;
        }

        public IAlterColumnOptionSyntax AsAnsiString(int size)
        {
            Expression.Column.Type = DbType.AnsiString;
            Expression.Column.Size = size;
            return this;
        }

        public IAlterColumnOptionSyntax AsBinary()
        {
            Expression.Column.Type = DbType.Binary;
            return this;
        }

        public IAlterColumnOptionSyntax AsBinary(int size)
        {
            Expression.Column.Type = DbType.Binary;
            Expression.Column.Size = size;
            return this;
        }

        public IAlterColumnOptionSyntax AsBoolean()
        {
            Expression.Column.Type = DbType.Boolean;
            return this;
        }

        public IAlterColumnOptionSyntax AsByte()
        {
            Expression.Column.Type = DbType.Byte;
            return this;
        }

        public IAlterColumnOptionSyntax AsCurrency()
        {
            Expression.Column.Type = DbType.Currency;
            return this;
        }

        public IAlterColumnOptionSyntax AsDate()
        {
            Expression.Column.Type = DbType.Date;
            return this;
        }

        public IAlterColumnOptionSyntax AsDateTime()
        {
            Expression.Column.Type = DbType.DateTime;
            return this;
        }

        public IAlterColumnOptionSyntax AsDecimal()
        {
            Expression.Column.Type = DbType.Decimal;
            return this;
        }

        public IAlterColumnOptionSyntax AsDecimal(int size, int precision)
        {
            Expression.Column.Type = DbType.Decimal;
            Expression.Column.Size = size;
            Expression.Column.Precision = precision;
            return this;
        }

        public IAlterColumnOptionSyntax AsDouble()
        {
            Expression.Column.Type = DbType.Double;
            return this;
        }

        public IAlterColumnOptionSyntax AsFixedLengthString(int size)
        {
            Expression.Column.Type = DbType.StringFixedLength;
            Expression.Column.Size = size;
            return this;
        }

        public IAlterColumnOptionSyntax AsFixedLengthAnsiString(int size)
        {
            Expression.Column.Type = DbType.AnsiStringFixedLength;
            Expression.Column.Size = size;
            return this;
        }

        public IAlterColumnOptionSyntax AsFloat()
        {
            Expression.Column.Type = DbType.Single;
            return this;
        }

        public IAlterColumnOptionSyntax AsGuid()
        {
            Expression.Column.Type = DbType.Guid;
            return this;
        }

        public IAlterColumnOptionSyntax AsInt16()
        {
            Expression.Column.Type = DbType.Int16;
            return this;
        }

        public IAlterColumnOptionSyntax AsInt32()
        {
            Expression.Column.Type = DbType.Int32;
            return this;
        }

        public IAlterColumnOptionSyntax AsInt64()
        {
            Expression.Column.Type = DbType.Int64;
            return this;
        }

        public IAlterColumnOptionSyntax AsString()
        {
            Expression.Column.Type = DbType.String;
            return this;
        }

        public IAlterColumnOptionSyntax AsString(int size)
        {
            Expression.Column.Type = DbType.String;
            Expression.Column.Size = size;
            return this;
        }

        public IAlterColumnOptionSyntax AsTime()
        {
            Expression.Column.Type = DbType.Time;
            return this;
        }

        public IAlterColumnOptionSyntax AsXml()
        {
            Expression.Column.Type = DbType.Xml;
            return this;
        }

        public IAlterColumnOptionSyntax AsXml(int size)
        {
            Expression.Column.Type = DbType.Xml;
            Expression.Column.Size = size;
            return this;
        }

        public IAlterColumnOptionSyntax AsCustom(string customType)
        {
            Expression.Column.Type = null;
            Expression.Column.CustomType = customType;
            return this;
        }

        public IAlterColumnOptionSyntax WithDefaultValue(object value)
        {
            Expression.Column.DefaultValue = value;
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

        public IAlterColumnOptionSyntax References(string foreignKeyName, string foreignTableName, IEnumerable<string> foreignColumnNames)
        {
            return References(foreignKeyName, null, foreignTableName, foreignColumnNames);
        }

        public IAlterColumnOptionSyntax References(string foreignKeyName, string foreignTableSchema, string foreignTableName, IEnumerable<string> foreignColumnNames)
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
            foreach (var foreignColumnName in foreignColumnNames)
                fk.ForeignKey.ForeignColumns.Add(foreignColumnName);

            _context.Expressions.Add(fk);
            return this;
        }
    }
}
