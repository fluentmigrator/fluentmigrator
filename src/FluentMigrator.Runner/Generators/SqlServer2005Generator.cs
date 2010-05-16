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

using System.Data;
using FluentMigrator.Expressions;

namespace FluentMigrator.Runner.Generators
{
	public class SqlServer2005Generator : SqlServer2000Generator
	{
		protected override void SetupTypeMaps()
		{
			base.SetupTypeMaps();

			SetTypeMap(DbType.String, "NVARCHAR(MAX)", UnicodeTextCapacity);
			SetTypeMap(DbType.AnsiString, "VARCHAR(MAX)", AnsiTextCapacity);
			SetTypeMap(DbType.Binary, "VARBINARY(MAX)", ImageCapacity);
		}

		public override string Generate(CreateSchemaExpression expression)
		{
			return FormatExpression("CREATE SCHEMA [{0}]", expression.SchemaName);
		}

		public override string Generate(DeleteSchemaExpression expression)
		{
			return FormatExpression("DROP SCHEMA [{0}]", expression.SchemaName);
		}

		public override string Generate(CreateTableExpression expression)
		{
			return FormatExpression("CREATE TABLE [{0}].[{1}] ({2})", expression.SchemaName ?? "dbo", expression.TableName, GetColumnDDL(expression));
		}

		public override string Generate(DeleteTableExpression expression)
		{
			return FormatExpression("DROP TABLE [{0}].[{1}]", expression.SchemaName ?? "dbo", expression.TableName);
		}
	}
}
