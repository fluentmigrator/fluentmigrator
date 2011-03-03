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



namespace FluentMigrator.Runner.Generators.SqlServer
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Text;
    using FluentMigrator.Expressions;
    using FluentMigrator.Model;
    using FluentMigrator.Runner.Generators.Generic;
    using FluentMigrator.Runner.Generators.Base;

    public class SqlServer2000Generator : GenericGenerator
    {
        public SqlServer2000Generator()
            : base(new SqlServerColumn(new SqlServer2000TypeMap()), new SqlServerQuoter())
        {
        }

        protected SqlServer2000Generator(IColumn column)
            : base(column, new SqlServerQuoter())
        {
        }

        public override string IfNotExistsString(CreateTableExpression expression)
        {
            return expression.IfNotExists ? string.Format("IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'{0}') AND type in (N'U')) ",Quoter.QuoteTableName(expression.TableName)) : "";
        }

        public override string CreateTable
        {
            get { return "{2}CREATE TABLE {0} ({1})"; }
        }

        public override string RenameTable { get { return "sp_rename '{0}', '{1}'"; } }

        public override string RenameColumn { get { return "sp_rename '{0}.{1}', '{2}'"; } }

        public override string DropIndex { get { return "DROP INDEX {1}.{0}"; } }

        public override string AddColumn { get { return "ALTER TABLE {0} ADD {1}"; } }

        //Not need for the nonclusted keyword as it is the default mode
        public override string GetClusterTypeString(CreateIndexExpression column)
        {
            return column.Index.IsClustered ? "CLUSTERED " : string.Empty;
        }

        public override string Generate(RenameTableExpression expression)
        {
            return String.Format(RenameTable, Quoter.QuoteTableName(Quoter.QuoteCommand(expression.OldName)),Quoter.QuoteCommand(expression.NewName));
        }

        public override string Generate(RenameColumnExpression expression)
        {
            return String.Format(RenameColumn, Quoter.QuoteTableName(expression.TableName), Quoter.QuoteColumnName(Quoter.QuoteCommand(expression.OldName)),Quoter.QuoteCommand(expression.NewName));
        }

        public override string Generate(DeleteColumnExpression expression)
        {
            // before we drop a column, we have to drop any default value constraints in SQL Server
            const string sql = @"
			DECLARE @default sysname, @sql nvarchar(max);

			-- get name of default constraint
			SELECT @default = name
			FROM sys.default_constraints 
			WHERE parent_object_id = object_id('{0}')
			AND type = 'D'
			AND parent_column_id = (
				SELECT column_id 
				FROM sys.columns 
				WHERE object_id = object_id('{0}')
				AND name = '{1}'
			);

			-- create alter table command as string and run it
			SET @sql = N'ALTER TABLE {0} DROP CONSTRAINT ' + @default;
			EXEC sp_executesql @sql;

			-- now we can finally drop column
			ALTER TABLE {0} DROP COLUMN {1};";

            return String.Format(sql, Quoter.QuoteTableName(expression.TableName), Quoter.QuoteColumnName(expression.ColumnName));
        }

        public override string Generate(AlterDefaultConstraintExpression expression)
        {
            const string sql =
                @"
			DECLARE @default sysname, @sql nvarchar(max);

			-- get name of default constraint
			SELECT @default = name
			FROM sys.default_constraints 
			WHERE parent_object_id = object_id('{0}')
			AND type = 'D'
			AND parent_column_id = (
				SELECT column_id 
				FROM sys.columns 
				WHERE object_id = object_id('{0}')
				AND name = '{1}'
			);

			-- create alter table command to drop contraint as string and run it
			SET @sql = N'ALTER TABLE {0} DROP CONSTRAINT ' + @default;
			EXEC sp_executesql @sql;

			-- create alter table command to create new default constraint as string and run it
			SET @sql = N'ALTER TABLE {0} WITH NOCHECK ADD CONSTRAINT [' + @default + '] DEFAULT({2}) FOR {1}';
			EXEC sp_executesql @sql;";

            return String.Format(sql,Quoter.QuoteTableName(expression.TableName), Quoter.QuoteColumnName(expression.ColumnName),Quoter.QuoteValue(expression.DefaultValue));
        }
    }
}
