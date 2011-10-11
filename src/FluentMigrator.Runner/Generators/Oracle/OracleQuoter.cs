using System;
using FluentMigrator.Runner.Generators.Generic;

namespace FluentMigrator.Runner.Generators.Oracle
{
    public class OracleQuoter : GenericQuoter
    {
        public override string FormatDateTime(DateTime value)
        {
            var result = string.Format("to_date({0}{1}{0}, {0}yyyy-mm-dd hh24:mi:ss{0})", ValueQuote, value.ToString("yyyy-MM-dd HH:mm:ss")); //ISO 8601 DATETIME FORMAT (EXCEPT 'T' CHAR)
            return result;
        }

        //Not what to do here. Orcale quotes are " but if we do that then the column and table names become case
        //sensitive. For now we will just set it to not quote
        public override string OpenQuote { get { return string.Empty; } }

        public override string CloseQuote { get { return string.Empty; } }
    }
}