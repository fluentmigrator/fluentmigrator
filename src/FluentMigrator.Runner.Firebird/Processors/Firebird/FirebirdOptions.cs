
using System;
using System.Data.Common;

namespace FluentMigrator.Runner.Processors.Firebird
{
    /// <summary>
    /// Represents the transaction model used by the Firebird database processor.
    /// </summary>
    /// <summary>
    /// Automatically commits the transaction if a check fails.
    /// </summary>
    /// <summary>
    /// Automatically commits the transaction after each operation.
    /// </summary>
    /// <summary>
    /// Disables automatic transaction management.
    /// </summary>
    public enum FirebirdTransactionModel
    {
        /// <summary>
        /// Automatically starts a new transaction when a virtual lock check fails
        /// </summary>
        AutoCommitOnCheckFail,

        /// <summary>
        /// Automaticaly commits every processed statement
        /// </summary>
        AutoCommit,

        /// <summary>
        /// Don't manage transactions
        /// </summary>
        None
    }

    /// <summary>
    /// Represents configuration options for the Firebird database processor.
    /// </summary>
    /// <remarks>
    /// This class provides various settings to customize the behavior of the Firebird processor,
    /// including transaction models, name truncation, and other Firebird-specific features.
    /// </remarks>
    public class FirebirdOptions : ICloneable
    {
        /// <summary>
        /// Maximum internal length of names in firebird is 31 characters
        /// </summary>
        public static readonly int MaxNameLength = 31;

        /// <summary>
        /// Firebird only supports constraint, table, column, etc. names up to 31 characters
        /// </summary>
        public bool TruncateLongNames { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether key names should be packed to optimize their length.
        /// </summary>
        /// <remarks>
        /// When enabled, this option modifies key names to fit within the maximum allowed length
        /// for Firebird database objects, which is <see cref="FirebirdOptions.MaxNameLength"/> characters.
        /// This is particularly useful when working with long key names that might otherwise exceed
        /// the database's constraints.
        /// </remarks>
        public bool PackKeyNames { get; set; }

        /// <summary>
        /// Virtually lock tables and columns touched by DDL statements in a transaction
        /// </summary>
        public bool VirtualLock { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether all names should be quoted unconditionally.
        /// </summary>
        public bool ForceQuote { get; set; }

        /// <summary>
        /// Which transaction model to use if any to work around firebird's DDL restrictions
        /// </summary>
        public FirebirdTransactionModel TransactionModel { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FluentMigrator.Runner.Processors.Firebird.FirebirdOptions"/> class.
        /// </summary>
        /// <remarks>
        /// This constructor sets default values for the Firebird options:
        /// <list type="bullet">
        /// <item><description><see cref="TransactionModel"/> is set to <see cref="FluentMigrator.Runner.Processors.Firebird.FirebirdTransactionModel.None"/>.</description></item>
        /// <item><description><see cref="TruncateLongNames"/> is set to <c>false</c>.</description></item>
        /// <item><description><see cref="VirtualLock"/> is set to <c>false</c>.</description></item>
        /// </list>
        /// </remarks>
        public FirebirdOptions()
        {
            TransactionModel = FirebirdTransactionModel.None;
            TruncateLongNames = false;
            VirtualLock = false;
        }

        /// <summary>
        /// Creates a new instance of <see cref="FirebirdOptions"/> with the standard behavior settings.
        /// </summary>
        /// <remarks>
        /// The standard behavior includes the following settings:
        /// <list type="bullet">
        /// <item><description><see cref="FirebirdTransactionModel.None"/> as the transaction model.</description></item>
        /// <item><description>Long names are not truncated (<see cref="TruncateLongNames"/> is <c>false</c>).</description></item>
        /// <item><description>Virtual lock is disabled (<see cref="VirtualLock"/> is <c>false</c>).</description></item>
        /// </list>
        /// </remarks>
        /// <returns>A new instance of <see cref="FirebirdOptions"/> configured with the standard behavior.</returns>
        public static FirebirdOptions StandardBehaviour()
        {
            return new FirebirdOptions()
            {
                TransactionModel = FirebirdTransactionModel.None,
                TruncateLongNames = false,
                VirtualLock = false,
            };
        }

        /// <summary>
        /// Creates a new instance of <see cref="FluentMigrator.Runner.Processors.Firebird.FirebirdOptions"/> 
        /// configured to automatically commit the transaction if a check fails.
        /// </summary>
        /// <returns>
        /// A <see cref="FluentMigrator.Runner.Processors.Firebird.FirebirdOptions"/> instance with the following settings:
        /// <list type="bullet">
        /// <item><description><see cref="TransactionModel"/> is set to <see cref="FluentMigrator.Runner.Processors.Firebird.FirebirdTransactionModel.AutoCommitOnCheckFail"/>.</description></item>
        /// <item><description><see cref="TruncateLongNames"/> is set to <c>true</c>.</description></item>
        /// <item><description><see cref="VirtualLock"/> is set to <c>true</c>.</description></item>
        /// </list>
        /// </returns>
        public static FirebirdOptions CommitOnCheckFailBehaviour()
        {
            return new FirebirdOptions()
            {
                TransactionModel = FirebirdTransactionModel.AutoCommitOnCheckFail,
                TruncateLongNames = true,
                VirtualLock = true,
            };
        }

        /// <summary>
        /// Creates a new instance of <see cref="FluentMigrator.Runner.Processors.Firebird.FirebirdOptions"/> 
        /// with the <see cref="FluentMigrator.Runner.Processors.Firebird.FirebirdTransactionModel.AutoCommit"/> 
        /// transaction model.
        /// </summary>
        /// <remarks>
        /// This method configures the Firebird options to automatically commit the transaction 
        /// after each operation. Additionally, it enables truncation of long names and disables 
        /// virtual locking.
        /// </remarks>
        /// <returns>
        /// A configured <see cref="FluentMigrator.Runner.Processors.Firebird.FirebirdOptions"/> instance 
        /// with automatic commit behavior.
        /// </returns>
        public static FirebirdOptions AutoCommitBehaviour()
        {
            return new FirebirdOptions()
            {
                TransactionModel = FirebirdTransactionModel.AutoCommit,
                TruncateLongNames = true,
                VirtualLock = false,
            };
        }

        /// <summary>
        /// Applies the specified provider switches to the current <see cref="FirebirdOptions"/> instance.
        /// </summary>
        /// <param name="providerSwitches">
        /// A connection string containing provider-specific switches to configure the Firebird options.
        /// </param>
        /// <returns>
        /// The current <see cref="FirebirdOptions"/> instance with the applied settings.
        /// </returns>
        /// <remarks>
        /// This method parses the provided connection string to extract and apply specific options.
        /// For example, it can configure the <see cref="ForceQuote"/> property based on the "Force Quote" switch.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// Thrown if the provided connection string is invalid or cannot be parsed.
        /// </exception>
        public FirebirdOptions ApplyProviderSwitches(string providerSwitches)
        {
            var csb = new DbConnectionStringBuilder {ConnectionString = providerSwitches};
            if (csb.TryGetValue("Force Quote", out var forceQuoteObj))
            {
                ForceQuote = ConvertToBoolean(forceQuoteObj);
            }

            return this;
        }

        /// <summary>
        /// Creates a shallow copy of the current <see cref="FluentMigrator.Runner.Processors.Firebird.FirebirdOptions"/> instance.
        /// </summary>
        /// <returns>
        /// A new <see cref="object"/> that is a shallow copy of the current instance.
        /// </returns>
        /// <remarks>
        /// This method uses <see cref="object.MemberwiseClone"/> to create a shallow copy of the current object.
        /// It is useful for duplicating the configuration options without affecting the original instance.
        /// </remarks>
        public object Clone()
        {
            return MemberwiseClone();
        }

        /// <summary>
        /// Converts the specified value to a boolean representation.
        /// </summary>
        /// <param name="value">The value to convert. Can be of type <see cref="bool"/>, <see cref="string"/>, or a numeric type.</param>
        /// <returns>
        /// <c>true</c> if the value represents a truthy value (e.g., <c>true</c>, "yes", "true", "1"); otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="FormatException">
        /// Thrown if the value is a string that cannot be interpreted as a boolean.
        /// </exception>
        /// <remarks>
        /// This method supports multiple input types for flexibility:
        /// <list type="bullet">
        /// <item><description>If the value is a <see cref="bool"/>, it is returned as-is.</description></item>
        /// <item><description>If the value is a <see cref="string"/>, it is evaluated case-insensitively against "yes", "true", or "1".</description></item>
        /// <item><description>If the value is numeric, it is considered <c>true</c> if non-zero.</description></item>
        /// </list>
        /// </remarks>
        private static bool ConvertToBoolean(object value)
        {
            if (value is bool b)
                return b;
            if (value is string s)
                return ConvertToBoolean(s);
            return Convert.ToInt32(value) != 0;
        }

        /// <summary>
        /// Converts a string representation of a boolean value to its <see cref="bool"/> equivalent.
        /// </summary>
        /// <param name="value">The string to convert.</param>
        /// <returns>
        /// <c>true</c> if the string represents a boolean value of "yes", "true", or "1" (case-insensitive); otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="value"/> is <c>null</c>.</exception>
        /// <remarks>
        /// This method performs a case-insensitive comparison to determine if the input string matches
        /// common representations of a boolean "true" value.
        /// </remarks>
        private static bool ConvertToBoolean(string value)
        {
            if (string.Equals(value, "yes", StringComparison.OrdinalIgnoreCase))
                return true;
            if (string.Equals(value, "true", StringComparison.OrdinalIgnoreCase))
                return true;
            if (string.Equals(value, "1", StringComparison.OrdinalIgnoreCase))
                return true;
            return false;
        }
    }
}
