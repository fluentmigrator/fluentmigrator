using System;
using System.Text.RegularExpressions;

namespace FluentMigrator.Runner.Processors.Postgres
{
    public class PostgresOptions : ICloneable
    {
        /// <summary>
        /// Gets or sets a value indicating whether all names should be quoted unconditionally.
        /// </summary>
        public bool ForceQuote { get; set; } = true;

        public static PostgresOptions ParseProviderSwitches(string providerSwitches)
        {
            var retval = new PostgresOptions();

            var switchesParsed = Regex.Matches(providerSwitches ?? string.Empty, @"(?<key>[^=]+)=(?<value>[^\s]+)");
            foreach (Match match in switchesParsed)
            {
                if (!match.Success)
                {
                    continue;
                }

                var key = match.Groups["key"].Value;
                var value = match.Groups["value"].Value;

                if ("Force Quote".Equals(key, StringComparison.OrdinalIgnoreCase) && bool.TryParse(value, out var forceQuoteParsed))
                {
                    retval.ForceQuote = forceQuoteParsed;
                }
            }

            return retval;
        }

        /// <inheritdoc />
        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
