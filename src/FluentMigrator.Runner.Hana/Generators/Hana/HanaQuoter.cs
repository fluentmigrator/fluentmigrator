using System;
using FluentMigrator.Runner.Generators.Generic;

namespace FluentMigrator.Runner.Generators.Hana
{
    public class HanaQuoter : GenericQuoter
    {
        public override string OpenQuote { get { return "\""; } }

        public override string CloseQuote { get { return "\""; } }

        public override string QuoteValue(object value)
        {
            if (value != null && value is ExplicitUnicodeString)
            {
                return string.Format("N{0}", FormatString(value.ToString()));
            }

            return base.QuoteValue(value);
        }
    }

}