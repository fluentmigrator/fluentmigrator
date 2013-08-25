#region License
// 
// Copyright (c) 2007-2009, Sean Chambers <schambers80@gmail.com>
// Copyright (c) 2012, Daniel Lee
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

using System.Text;

namespace FluentMigrator.Runner.Generators.SqlServer
{
    public class SqlServer2012Generator : SqlServer2008Generator
    {
        public SqlServer2012Generator()
            :base(new SqlServerColumn(new SqlServer2008TypeMap()), new SqlServer2005DescriptionGenerator())
        {
        }

        public override string Generate(Expressions.CreateSequenceExpression expression)
        {
            var result = new StringBuilder(string.Format("CREATE SEQUENCE "));
            var seq = expression.Sequence;
            result.AppendFormat("{0}.{1}", Quoter.QuoteSchemaName(seq.SchemaName), Quoter.QuoteSequenceName(seq.Name));

            if (seq.Increment.HasValue)
            {
                result.AppendFormat(" INCREMENT BY {0}", seq.Increment);
            }

            if (seq.MinValue.HasValue)
            {
                result.AppendFormat(" MINVALUE {0}", seq.MinValue);
            }

            if (seq.MaxValue.HasValue)
            {
                result.AppendFormat(" MAXVALUE {0}", seq.MaxValue);
            }

            if (seq.StartWith.HasValue)
            {
                result.AppendFormat(" START WITH {0}", seq.StartWith);
            }

            if (seq.Cache.HasValue)
            {
                result.AppendFormat(" CACHE {0}", seq.Cache);
            }

            if (seq.Cycle)
            {
                result.Append(" CYCLE");
            }

            return result.ToString();
        }

        public override string Generate(Expressions.DeleteSequenceExpression expression)
        {
            var result = new StringBuilder(string.Format("DROP SEQUENCE "));
            result.AppendFormat("{0}.{1}", Quoter.QuoteSchemaName(expression.SchemaName), Quoter.QuoteSequenceName(expression.SequenceName));

            return result.ToString();
        }
    }
}