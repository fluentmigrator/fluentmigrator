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

using FluentMigrator.Expressions;
using FluentMigrator.Model;
using System.Linq;
using System.Text;

namespace FluentMigrator.Runner.Generators.SqlServer
{
    public class SqlServer2008Generator : SqlServer2005Generator
    {
        public SqlServer2008Generator()
            : base(new SqlServerColumn(new SqlServer2008TypeMap()), new SqlServer2005DescriptionGenerator())
        {
        }

        public SqlServer2008Generator(IColumn column, IDescriptionGenerator descriptionGenerator)
            :base(column, descriptionGenerator)
        {
        }

        public override string GetWithNullsDistinctString(IndexDefinition index)
        {
            if (index.Columns.Where(c => c.IsNullDistinct.HasValue).Any() && !index.IsUnique)
            {
                compatabilityMode.HandleCompatabilty("With nulls distinct can only be used for unique indexes");
            }

            return string.Empty;
        }

        public override string Generate(CreateIndexExpression expression)
        {
            string sql = base.Generate(expression);

            if (expression.Index.IsUnique)
            {
                bool isFirstColumn = true;
                bool appendFilterToSql = false;
                StringBuilder filterSql = new StringBuilder();
                filterSql.AppendFormat(" WHERE");

                foreach (var column in expression.Index.Columns)
                {
                    if (column.IsNullDistinct.HasValue && column.IsNullDistinct.Value == false)
                    {
                        if (!isFirstColumn)
                            filterSql.AppendFormat(" AND");

                        filterSql.AppendFormat(" {0} IS NOT NULL", Quoter.QuoteColumnName(column.Name));
                        isFirstColumn = false;
                        appendFilterToSql = true;
                    }
                }

                if (appendFilterToSql)
                    sql += filterSql.ToString();
            }

            return sql;
        }

    }
}
