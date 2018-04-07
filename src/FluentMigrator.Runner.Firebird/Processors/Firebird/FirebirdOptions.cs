
using System;
using System.Data.Common;

namespace FluentMigrator.Runner.Processors.Firebird
{
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

        public FirebirdOptions()
        {
            TransactionModel = FirebirdTransactionModel.None;
            TruncateLongNames = false;
            VirtualLock = false;
        }

        public static FirebirdOptions StandardBehaviour()
        {
            return new FirebirdOptions()
            {
                TransactionModel = FirebirdTransactionModel.None,
                TruncateLongNames = false,
                VirtualLock = false,
            };
        }

        public static FirebirdOptions CommitOnCheckFailBehaviour()
        {
            return new FirebirdOptions()
            {
                TransactionModel = FirebirdTransactionModel.AutoCommitOnCheckFail,
                TruncateLongNames = true,
                VirtualLock = true,
            };
        }

        public static FirebirdOptions AutoCommitBehaviour()
        {
            return new FirebirdOptions()
            {
                TransactionModel = FirebirdTransactionModel.AutoCommit,
                TruncateLongNames = true,
                VirtualLock = false,
            };
        }

        public FirebirdOptions ApplyProviderSwitches(string providerSwitches)
        {
            var csb = new DbConnectionStringBuilder {ConnectionString = providerSwitches};
            if (csb.TryGetValue("Force Quote", out var forceQuoteObj))
            {
                ForceQuote = ConvertToBoolean(forceQuoteObj);
            }

            return this;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        private static bool ConvertToBoolean(object value)
        {
            if (value is bool b)
                return b;
            if (value is string s)
                return ConvertToBoolean(s);
            return Convert.ToInt32(value) != 0;
        }

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
