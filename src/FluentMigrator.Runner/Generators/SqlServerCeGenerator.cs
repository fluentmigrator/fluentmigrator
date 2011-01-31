#region License
// 
// Copyright (c) 2007-2009, Sean Chambers <schambers80@gmail.com>
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
    public class SqlServerCeGenerator : SqlServer2005Generator
    {
        public SqlServerCeGenerator()
            : base(new SqlServerColumn(new SqlServerCeTypeMap()))
        {
        }

        public override string Generate(RenameTableExpression expression)
        {
            return String.Format("sp_rename '{0}[{1}]', '{2}'", FormatSchema(expression.SchemaName), expression.OldName, FormatSqlEscape(expression.NewName));
        }

        public override string Generate(CreateSchemaExpression expression)
        {
            throw new NotImplementedException();
        }

        public override string Generate(DeleteSchemaExpression expression)
        {
            throw new NotImplementedException();
        }

        public override string Generate(AlterSchemaExpression expression)
        {
            throw new NotImplementedException();
        }

        protected override string FormatSchema(string schemaName, bool escapeSchemaName)
        {
            // schemas are not supported in CE
            return string.Empty;
        }

        public override string Generate(DeleteColumnExpression expression)
        {
            // Limited functionality in CE, for now will just drop the column.. no DECLARE support!
            const string sql = @"
			-- now we can finally drop column
			ALTER TABLE {0}[{2}] DROP COLUMN [{3}];";

            return String.Format(sql, FormatSchema(expression.SchemaName), FormatSchema(expression.SchemaName, false), expression.TableName, expression.ColumnName);
        }


    }
}