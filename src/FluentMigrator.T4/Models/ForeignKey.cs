using System.Linq;
using System;

namespace FluentMigrator.T4
{
    public class ForeignKey
    {
        public string OtherTable;
        public string OtherColumn;
        public string OtherClass;

        public string ThisTable;
        public string ThisColumn;
        public string ToTable;
        
        public string FromColumn;
        public string ToColumn;
    }
}