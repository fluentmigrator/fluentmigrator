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
    using Expressions;
    using Generic;

    public class MySqlGenerator : GenericGenerator
	{
		public MySqlGenerator() : base(new MySqlColumn(), new MySqlQuoter())
		{
		}

        public override string AlterColumn { get { return "ALTER TABLE {0} MODIFY COLUMN {1}"; } }

        public override string DeleteConstraint { get { return "ALTER TABLE {0} DROP FOREIGN KEY {1}"; } }

        public override string CreateTable { get { return "CREATE TABLE {0} ({1}) ENGINE = INNODB"; } }

        public override string Generate(DeleteIndexExpression expression)
        {
            return string.Format("DROP INDEX {0} ON {1}", Quoter.QuoteIndexName(expression.Index.Name), Quoter.QuoteTableName(expression.Index.TableName));
        }

        public override string Generate(RenameColumnExpression expression)
        {
            return string.Format("ALTER TABLE {0} CHANGE {1} {2} ", Quoter.QuoteTableName(expression.TableName), Quoter.QuoteColumnName(expression.OldName), Quoter.QuoteColumnName(expression.NewName));
        }

        public override string Generate(AlterDefaultConstraintExpression expression)
		{
            return compatabilityMode.HandleCompatabilty("Altering of default constraints is not supporteed for MySql");
		}

        public override string Generate(CreateSequenceExpression expression)
        {
            return compatabilityMode.HandleCompatabilty("Sequences is not supporteed for MySql");
        }

        public override string Generate(DeleteSequenceExpression expression)
        {
            return compatabilityMode.HandleCompatabilty("Sequences is not supporteed for MySql");
        }
	}
}
