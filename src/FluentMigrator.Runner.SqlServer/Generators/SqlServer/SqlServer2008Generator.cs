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

using System.Linq;

using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure.Extensions;
using FluentMigrator.Model;
using FluentMigrator.SqlServer;

namespace FluentMigrator.Runner.Generators.SqlServer
{
    public class SqlServer2008Generator : SqlServer2005Generator
    {
        public SqlServer2008Generator()
            : base(new SqlServer2008Column(new SqlServer2008TypeMap()), new SqlServer2005DescriptionGenerator())
        {
        }

        protected SqlServer2008Generator(IColumn column, IDescriptionGenerator descriptionGenerator)
            :base(column, descriptionGenerator)
        {
        }

        public virtual string GetWithNullsDistinctString(IndexDefinition index)
        {
            bool? GetNullsDistinct(IndexColumnDefinition column)
            {
                return column.GetAdditionalFeature(SqlServerExtensions.IndexColumnNullsDistinct, (bool?) null);
            }

            var nullDistinctColumns = index.Columns.Where(c => GetNullsDistinct(c) != null).ToList();
            if (nullDistinctColumns.Count != 0 && !index.IsUnique)
            {
                compatabilityMode.HandleCompatabilty("With nulls distinct can only be used for unique indexes");
                return string.Empty;
            }

            var conditions = nullDistinctColumns
                .Where(x => (GetNullsDistinct(x) ?? true) == false)
                .Select(c => $"{Quoter.QuoteColumnName(c.Name)} IS NOT NULL");

            var condition = string.Join(" AND ", conditions);
            if (condition.Length == 0)
                return string.Empty;

            return $" WHERE {condition}";
        }

        public override string Generate(CreateIndexExpression expression)
        {
            var sql = base.Generate(expression);
            sql += GetWithNullsDistinctString(expression.Index);
            return sql;
        }
    }
}
