using System.Linq;
using System;

namespace FluentMigrator.T4
{
    public class Column
    {
        public string Name;
        public string PropertyName;
        public string PropertyType;
        public bool IsPK;
        public bool IsNullable;
        public bool IsAutoIncrement;
        public bool Ignore;
        public int Size;
        public int Precision;
        public string DefaultValue;
    }
}