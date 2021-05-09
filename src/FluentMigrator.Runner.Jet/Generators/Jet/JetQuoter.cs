using System;
using FluentMigrator.Runner.Generators.Generic;
using FluentMigrator.Runner.Initialization;

using Microsoft.Extensions.Options;

namespace FluentMigrator.Runner.Generators.Jet
{
    public class JetQuoter : GenericQuoter
    {
        public JetQuoter(IOptions<QuoterOptions> options)
            : base(options)
        {
        }

        public override string OpenQuote { get { return "["; } }

        public override string CloseQuote { get { return "]"; } }

        public override string CloseQuoteEscapeString { get { return string.Empty; } }

        public override string OpenQuoteEscapeString { get { return string.Empty; } }

        public override string FormatDateTime(DateTime value)
        {
            return ValueQuote + (value).ToString("yyyy-MM-dd HH:mm:ss") + ValueQuote;
        }

        public override string QuoteSchemaName(string schemaName)
        {
            return string.Empty;
        }
    }
}
