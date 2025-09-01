using System;
using FluentMigrator.Runner.Generators.Generic;

namespace FluentMigrator.Runner.Generators.Jet
{
    /// <summary>
    /// The Jet SQL quoter for FluentMigrator.
    /// </summary>
    public class JetQuoter : GenericQuoter
    {
        /// <inheritdoc />
        public override string OpenQuote => "[";

        /// <inheritdoc />
        public override string CloseQuote => "]";

        /// <inheritdoc />
        public override string CloseQuoteEscapeString => string.Empty;

        /// <inheritdoc />
        public override string OpenQuoteEscapeString => string.Empty;

        /// <inheritdoc />
        public override string FormatDateTime(DateTime value)
        {
            return ValueQuote + (value).ToString("yyyy-MM-dd HH:mm:ss") + ValueQuote;
        }

        /// <inheritdoc />
        public override string QuoteSchemaName(string schemaName)
        {
            return string.Empty;
        }
    }
}
