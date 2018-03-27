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

using System;
using System.Linq;

namespace FluentMigrator.Runner.Generators.MySql
{
    using Expressions;
    using Generic;

    public class MySqlGenerator : GenericGenerator
    {
        public MySqlGenerator()
            : base(new MySqlColumn(), new MySqlQuoter(), new EmptyDescriptionGenerator())
        {
        }

        public override string AlterColumn { get { return "ALTER TABLE {0} MODIFY COLUMN {1}"; } }
        public override string DeleteConstraint { get { return "ALTER TABLE {0} DROP {1}{2}"; } }
        //public override string DeleteConstraint { get { return "ALTER TABLE {0} DROP FOREIGN KEY {1}"; } }

        public override string Generate(CreateTableExpression expression)
        {
            if (string.IsNullOrEmpty(expression.TableName)) throw new ArgumentNullException("expression", "expression.TableName cannot be empty");
            if (expression.Columns.Count == 0) throw new ArgumentException("You must specifiy at least one column");

            string errors = ValidateAdditionalFeatureCompatibility(expression.Columns.SelectMany(x => x.AdditionalFeatures));
            if (!string.IsNullOrEmpty(errors)) return errors;

            string quotedTableName = Quoter.QuoteTableName(expression.TableName);

            string tableDescription = string.IsNullOrEmpty(expression.TableDescription)
                ? string.Empty
                : string.Format(" COMMENT {0}", Quoter.QuoteValue(expression.TableDescription));

            return string.Format("CREATE TABLE {0} ({1}){2} ENGINE = INNODB",
                quotedTableName,
                Column.Generate(expression.Columns, quotedTableName),
                tableDescription);
        }

        public override string Generate(AlterTableExpression expression)
        {
            if (string.IsNullOrEmpty(expression.TableDescription))
                return base.Generate(expression);

            return string.Format("ALTER TABLE {0} COMMENT {1}", Quoter.QuoteTableName(expression.TableName), Quoter.QuoteValue(expression.TableDescription));
        }

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

        public override string Generate(DeleteConstraintExpression expression)
        {
            if (expression.Constraint.IsPrimaryKeyConstraint)
            {
                return string.Format(DeleteConstraint, Quoter.QuoteTableName(expression.Constraint.TableName), "PRIMARY KEY", "");
            }
            return string.Format(DeleteConstraint, Quoter.QuoteTableName(expression.Constraint.TableName), "INDEX ", Quoter.Quote(expression.Constraint.ConstraintName));
        }

        public override string Generate(DeleteForeignKeyExpression expression)
        {
            return string.Format(DeleteConstraint, Quoter.QuoteTableName(expression.ForeignKey.ForeignTable), "FOREIGN KEY ", Quoter.QuoteColumnName(expression.ForeignKey.Name));
        }

        public override string Generate(DeleteDefaultConstraintExpression expression)
        {
            return compatabilityMode.HandleCompatabilty("Default constraints are not supported");
        }
    }
}
