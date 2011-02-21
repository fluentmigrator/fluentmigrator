#region License
// 
// Copyright (c) 2010, Nathan Brown
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



namespace FluentMigrator.Runner.Generators.SqlServer
{
    using System;
    using FluentMigrator.Expressions;

	public class SqlServer2005Generator : SqlServer2000Generator
	{
		public SqlServer2005Generator() : base(new SqlServerColumn(new SqlServer2005TypeMap()))		{
		}

		protected SqlServer2005Generator(IColumn column) : base(column)
		{
		}

        public override string DropIndex { get { return "DROP INDEX {0} ON {1}"; } }

		public override string Generate(CreateSchemaExpression expression)
		{
			return String.Format("CREATE SCHEMA {0}", Quoter.QuoteForSchemaName(expression.SchemaName));
		}

		public override string Generate(DeleteSchemaExpression expression)
		{
			return String.Format("DROP SCHEMA {0}", Quoter.QuoteForSchemaName(expression.SchemaName));
		}

        public override string Generate( AlterSchemaExpression expression )
        {
            return String.Format("ALTER SCHEMA {0} TRANSFER {1}{2}", Quoter.QuoteForSchemaName(expression.DestinationSchemaName), Quoter.QuoteForSchemaName(expression.SourceSchemaName), Quoter.QuoteTableName(expression.TableName));
        }
	}
}
