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



namespace FluentMigrator.Runner.Generators.SqlServer
{
    using System;
    using FluentMigrator.Expressions;
    using FluentMigrator.Runner.Generators;

	public class SqlServerCeGenerator : SqlServer2000Generator
    {
        public SqlServerCeGenerator()
            : base(new SqlServerColumn(new SqlServerCeTypeMap()))
        {
        }

        public override string Generate(RenameTableExpression expression)
        {
			// Don't quote the table names, as those square brackets are treated as invalid by SQL Compact.
			return String.Format("sp_rename '{0}', '{1}'", expression.OldName, expression.NewName);
        }

        //All Schema method throw by default as only Sql server 2005 and up supports them.
        public override string Generate(CreateSchemaExpression expression)
        {
            throw new DatabaseOperationNotSupportedExecption();
        }

        public override string Generate(DeleteSchemaExpression expression)
        {
            throw new DatabaseOperationNotSupportedExecption();
        }

        public override string Generate(AlterSchemaExpression expression)
        {
            throw new DatabaseOperationNotSupportedExecption();
        }

        public override string Generate(DeleteColumnExpression expression)
        {
            // Limited functionality in CE, for now will just drop the column.. no DECLARE support!
            const string sql = @"ALTER TABLE {0} DROP COLUMN {1};";
            return String.Format(sql, Quoter.QuoteTableName(expression.TableName), Quoter.QuoteColumnName(expression.ColumnName));
        }

        public override string Generate(DeleteIndexExpression expression)
        {
            return String.Format("DROP INDEX {0}.{1}", Quoter.QuoteTableName(expression.Index.TableName), Quoter.QuoteIndexName(expression.Index.Name));
        }
    }
}