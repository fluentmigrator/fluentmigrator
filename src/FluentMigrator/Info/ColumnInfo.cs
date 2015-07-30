using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace FluentMigrator.Info
{
    public class ColumnInfo
    {
        public string Name { get; set; }

        public DbType DbType { get; set; }

        public bool NotNull { get; set; }
    }
}
