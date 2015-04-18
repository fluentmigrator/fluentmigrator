using System;
using FluentMigrator.Runner.Generators.Generic;

namespace FluentMigrator.Runner.Generators.Oracle
{
    public class OracleQuoterQuotedIdentifier : GenericQuoter
    {
        public override string FormatDateTime(DateTime value)
        {
            var result = string.Format("to_date({0}{1}{0}, {0}yyyy-mm-dd hh24:mi:ss{0})", ValueQuote, value.ToString("yyyy-MM-dd HH:mm:ss")); //ISO 8601 DATETIME FORMAT (EXCEPT 'T' CHAR)
            return result;
        }

        public override string FromTimeSpan(TimeSpan value)
        {
            return String.Format("{0}{1} {2}:{3}:{4}.{5}{0}"
                , ValueQuote
                , value.Days
                , value.Hours
                , value.Minutes
                , value.Seconds
                , value.Milliseconds);
        }

        public override string FormatGuid(Guid value)
        {
            return string.Format("{0}{1}{0}", ValueQuote, BitConverter.ToString(value.ToByteArray()).Replace("-", string.Empty));
        }
    }
}