using System;

namespace FluentMigrator.Runner.Processors.Snowflake
{
    public class SnowflakeOptions : ICloneable
    {
        public bool QuoteIdentifiers { get; set; }

        public static SnowflakeOptions QuotingEnabled()
        {
            return new SnowflakeOptions
            {
                QuoteIdentifiers = true
            };
        }

        public static SnowflakeOptions QuotingDisabled()
        {
            return new SnowflakeOptions
            {
                QuoteIdentifiers = false
            };
        }

        /// <inheritdoc />
        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
