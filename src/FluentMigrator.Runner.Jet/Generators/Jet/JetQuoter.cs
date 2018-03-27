using System;
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
            return ValueQuote + (value).ToString("yyyy-MM-dd HH:mm:ss") + ValueQuote;
        }
    }
}
