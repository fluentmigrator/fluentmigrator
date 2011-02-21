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


namespace FluentMigrator.Runner.Generators.SQLite
{

    using System;
    using System.Collections.Generic;
    using System.Text;
    using FluentMigrator.Expressions;
    using FluentMigrator.Model;
    using FluentMigrator.Runner.Generators.Generic;
    using FluentMigrator.Runner.Generators.Base;

	public class SqliteGenerator : GenericGenerator
	{
		public SqliteGenerator()
			: base(new SqliteColumn(), new SqliteQuoter())
		{
		}

        

       

        public override string RenameTable { get { return "ALTER TABLE {0} RENAME TO {1}"; } }

		

        public override string Generate(AlterColumnExpression expression)
        {
            throw new DatabaseOperationNotSupportedExecption("Sqlite does not support alter column");
        }

        public override string Generate(RenameColumnExpression expression)
        {
            throw new DatabaseOperationNotSupportedExecption("Sqlite does not support renaming of columns");
        }

		public override string Generate(AlterDefaultConstraintExpression expression)
		{
            throw new DatabaseOperationNotSupportedExecption();
		}

		public override string Generate(CreateForeignKeyExpression expression)
		{
			// Ignore foreign keys for SQLite
			return "";
		}

		public override string Generate(DeleteForeignKeyExpression expression)
		{
			return "";
		}
	}
}
