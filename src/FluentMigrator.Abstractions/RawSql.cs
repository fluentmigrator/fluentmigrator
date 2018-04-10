using System;

namespace FluentMigrator
{
    public class RawSql
    {
        private RawSql(string underlyingSql)
        {
            Value = underlyingSql ?? throw new ArgumentNullException(nameof(underlyingSql));
        }

        public string Value { get; }

        public static RawSql Insert(string sqlToRun)
        {
            return new RawSql(sqlToRun);
        }
    }
}
