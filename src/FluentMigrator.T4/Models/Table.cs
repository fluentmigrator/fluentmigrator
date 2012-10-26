using System.Collections.Generic;
using System.Linq;
using System;

namespace FluentMigrator.T4
{
    public class Table
    {
        public List<Column> Columns;
        public List<TableIndex> Indices;
        public List<ForeignKey> FKeys;
        public string Name;
        public string Schema;
        public bool IsView;
        public string CleanName;
        public string ClassName;
        public string SequenceName;
        public bool Ignore;
        public string SQL;

        public Column[] PK
        {
            get
            {
                return this.Columns.Where(x => x.IsPK).ToArray();
            }
        }

        public Column GetColumn(string columnName)
        {
            return this.Columns.Single(x => string.Compare(x.Name, columnName, true) == 0);
        }

        public Column this[string columnName]
        {
            get
            {
                return this.GetColumn(columnName);
            }
        }

        public TableIndex GetIndex(string indexName)
        {
            return this.Indices.Single(x => string.Compare(x.Name, indexName, true) == 0);
        }
    }
}