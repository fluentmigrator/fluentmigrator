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

using System;
using FluentMigrator.Expressions;

namespace FluentMigrator.Runner.Generators
{
	public class SqlServer2005Generator : SqlServer2000Generator
	{
		public SqlServer2005Generator() : base(new SqlServer2005TypeMap())		{
		}

		protected SqlServer2005Generator(ITypeMap typeMap) : base(typeMap)
		{
		}

		public override string Generate(CreateSchemaExpression expression)
		{
			return String.Format("CREATE SCHEMA [{0}]", expression.SchemaName);
		}

		public override string Generate(DeleteSchemaExpression expression)
		{
			return String.Format("DROP SCHEMA [{0}]", expression.SchemaName);
		}

		protected override string FormatSchema(string schemaName, bool escapeSchemaName)
		{
			return string.Format(
				escapeSchemaName
					? "[{0}]."
					: "{0}.",
				schemaName ?? "dbo");
		}
	}
}
