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


namespace FluentMigrator.Runner.Generators.MySql
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Text;
    using FluentMigrator.Expressions;
    using FluentMigrator.Model;
    using FluentMigrator.Runner.Generators.Generic;
    using FluentMigrator.Runner.Generators.Base;


	public class MySqlGenerator : GenericGenerator
	{
		public MySqlGenerator() : base(new MySqlColumn(), new MySqlQuoter())
		{
		}
   
        public override string DropIndex { get{ return "DROP INDEX {0} ON {1}"; } }

        public override string AlterColumn { get { return "ALTER TABLE {0} MODIFY COLUMN {1}"; } }
        
        public override string DeleteConstraint { get { return "ALTER TABLE {0} DROP {1}{2}"; } }

        public override string CreateTable { get { return "CREATE TABLE {2}{0} ({1}) ENGINE = INNODB"; } }

        public override string Generate(DeleteIndexExpression expression)
        {
            return string.Format("DROP INDEX {0} ON {1}", Quoter.QuoteIndexName(expression.Index.Name), Quoter.QuoteTableName(expression.Index.TableName));
        }

        public override string Generate(RenameColumnExpression expression)
		{

			// may need to add definition to end. blerg
			//return String.Format("ALTER TABLE `{0}` CHANGE COLUMN {1} {2}", Quoter.QuoteTableName(expression.TableName), Quoter.QuoteColumnName(expression.OldName), Quoter.QuoteColumnName(expression.NewName));
			
			// NOTE: The above does not work, as the CHANGE COLUMN syntax in Mysql requires the column definition to be re-specified,
			// even if it has not changed; so marking this as not working for now
            return compatabilityMode.HandleCompatabilty("Renaming of columns is not supporteed for MySql");
		}

		public override string Generate(AlterDefaultConstraintExpression expression)
		{
            return compatabilityMode.HandleCompatabilty("Altering of default constrints is not supporteed for MySql");
		}

        public override string Generate(DeleteConstraintExpression expression)
        {
            if (expression.Constraint.IsPrimaryKeyConstraint)
            {
                return string.Format(DeleteConstraint, Quoter.QuoteTableName(expression.Constraint.TableName), "PRIMARY KEY","");
            }
            return string.Format(DeleteConstraint, Quoter.QuoteTableName(expression.Constraint.TableName),"INDEX ", Quoter.Quote(expression.Constraint.ConstraintName));
        }

        public override string Generate(DeleteForeignKeyExpression expression)
		{
            return string.Format(DeleteConstraint, Quoter.QuoteTableName(expression.ForeignKey.ForeignTable), "FOREIGN KEY ", Quoter.QuoteColumnName(expression.ForeignKey.Name));
		}
	}
}
