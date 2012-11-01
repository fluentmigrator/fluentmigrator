using System.Collections.Generic;
using System.Linq;
using System;

namespace FluentMigrator.T4
{
    public class Tables : List<Table>
    {
        public Table GetTable(string tableName)
        {
            return this.Single(x => string.Compare(x.Name, tableName, true) == 0);
        }

        public Table this[string tableName]
        {
            get
            {
                return this.GetTable(tableName);
            }
        }

    }
}