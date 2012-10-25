using System.Linq;
using System;

namespace FluentMigrator.T4
{
    public class FKey
    {
        public string ThisTable;
        public string ThisColumn;
        public string OtherTable;
        public string OtherColumn;
        public string OtherClass;

        public string ToTable;
        public string FromColumn;
        public string ToColumn;
    }
}