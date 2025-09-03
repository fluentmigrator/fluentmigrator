using System;

namespace FluentMigrator.Runner.Processors.Snowflake
{
    /// <summary>
    /// Represents configuration options for the Snowflake database processor.
    /// </summary>
    /// <remarks>
    /// This class provides options to control the behavior of the Snowflake processor, 
    /// such as whether to quote identifiers. It also includes factory methods for 
    /// creating instances with specific configurations.
    /// </remarks>
    public class SnowflakeOptions : ICloneable
    {
        /// <summary>
        /// Gets or sets a value indicating whether identifiers should be quoted in SQL statements.
        /// </summary>
        /// <value>
        /// <c>true</c> if identifiers should be quoted; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Quoting identifiers ensures that reserved keywords or special characters in identifiers
        /// are properly escaped in SQL statements. This option is particularly useful when working
        /// with database objects that have non-standard names.
        /// </remarks>
        public bool QuoteIdentifiers { get; set; }

        /// <summary>
        /// Creates a new instance of <see cref="SnowflakeOptions"/> with identifier quoting enabled.
        /// </summary>
        /// <returns>
        /// A <see cref="SnowflakeOptions"/> instance where <see cref="SnowflakeOptions.QuoteIdentifiers"/> is set to <c>true</c>.
        /// </returns>
        /// <remarks>
        /// This method is used to configure the Snowflake database processor to quote identifiers,
        /// ensuring compatibility with case-sensitive or reserved keywords in Snowflake.
        /// </remarks>
        public static SnowflakeOptions QuotingEnabled()
        {
            return new SnowflakeOptions
            {
                QuoteIdentifiers = true
            };
        }

        /// <summary>
        /// Creates a new instance of <see cref="SnowflakeOptions"/> with quoting of identifiers disabled.
        /// </summary>
        /// <returns>
        /// A <see cref="SnowflakeOptions"/> instance where <see cref="SnowflakeOptions.QuoteIdentifiers"/> is set to <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This method is a factory method for creating a configuration where quoting of identifiers is explicitly disabled.
        /// It is commonly used when working with Snowflake databases that do not require quoted identifiers.
        /// </remarks>
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
