using System.Collections.Generic;
using System.Linq;
using System;

namespace FluentMigrator.T4
{
    public class TableIndex
    {
        public string Name;
        public List<IndexColumn> IndexColumns;
        public bool IsUnique;
        public string SQL;
    }
}