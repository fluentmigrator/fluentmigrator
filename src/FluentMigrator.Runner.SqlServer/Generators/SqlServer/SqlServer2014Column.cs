using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentMigrator.Model;

namespace FluentMigrator.Runner.Generators.SqlServer
{
    internal class SqlServer2014Column : SqlServer2008Column
    {
        public SqlServer2014Column(ITypeMap typeMap, IQuoter quoter)
             : base(typeMap, quoter)
        {
            ClauseOrder.Add(FormatSparse);
        }
        protected override string FormatNullable(ColumnDefinition column)
        {
            if (column.IsNullable.HasValue == false && column.Type == null && !string.IsNullOrEmpty(column.CustomType))
            {
                return string.Empty;
            }

            if (column.IsNullable.HasValue == true && column.IsNullable == true && column.Type == null && !string.IsNullOrEmpty(column.CustomType))
            {
                return "NULL";
            }
            return base.FormatNullable(column);
        }
    }
}
