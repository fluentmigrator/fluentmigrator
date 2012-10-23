using System;
using System.Globalization;
using FluentMigrator.Runner.Generators.Generic;

namespace FluentMigrator.Runner.Generators.Jet
{
    public class JetQuoter : GenericQuoter
    {
        public override string OpenQuote { get { return "["; } }

        public override string CloseQuote { get { return "]"; } }

        public override string CloseQuoteEscapeString { get { return string.Empty; } }

        public override string OpenQuoteEscapeString { get { return string.Empty; } }

        public override string FormatDateTime(DateTime value)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}{1:yyyy-MM-dd HH:mm:ss}{0}", ValueQuote, (value));
        }
    }
}
