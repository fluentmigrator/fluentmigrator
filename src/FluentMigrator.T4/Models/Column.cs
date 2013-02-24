using System.Linq;
using System;

namespace FluentMigrator.T4
{
    public class Column
    {
        public string Name;
        public string PropertyName;
        public System.Data.DbType? PropertyType;
        public string CustomType { get; set; }
        public bool IsPrimaryKey;
        public bool IsNullable;
        public bool IsAutoIncrement;
        public bool Ignore;
        public int Size;
        public int Precision;
        public string DefaultValue;
    }
}