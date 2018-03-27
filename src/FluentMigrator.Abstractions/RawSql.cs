using System;

namespace FluentMigrator
{
    public class RawSql
    {
        private readonly string _underlyingSql;

        private RawSql(string underlyingSql)
        {
            if (underlyingSql == null) throw new ArgumentNullException("underlyingSql");
            _underlyingSql = underlyingSql;
        }

        public string Value
        {
            get
            {
                return _underlyingSql;
            }
        }

        public static RawSql Insert(string sqlToRun)
        {
            return new RawSql(sqlToRun);
        }
    }
}
