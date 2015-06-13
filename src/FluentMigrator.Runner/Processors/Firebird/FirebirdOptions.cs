
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
    public class FirebirdOptions
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

        
        public bool UndoEnabled { get; set; }

        /// <summary>
        /// Which transaction model to use if any to work around firebird's DDL restrictions
        /// </summary>
        public FirebirdTransactionModel TransactionModel { get; set; }

        public FirebirdOptions()
        {
            TransactionModel = FirebirdTransactionModel.None;
            TruncateLongNames = false;
            VirtualLock = false;
            UndoEnabled = false;
        }

        public static FirebirdOptions StandardBehaviour()
        {
            return new FirebirdOptions()
            {
                TransactionModel = FirebirdTransactionModel.None,
                TruncateLongNames = false,
                VirtualLock = false,
                UndoEnabled = false
            };
        }

        public static FirebirdOptions CommitOnCheckFailBehaviour()
        {
            return new FirebirdOptions()
            {
                TransactionModel = FirebirdTransactionModel.AutoCommitOnCheckFail,
                TruncateLongNames = true,
                VirtualLock = true,
                UndoEnabled = true
            };
        }

        public static FirebirdOptions AutoCommitBehaviour()
        {
            return new FirebirdOptions()
            {
                TransactionModel = FirebirdTransactionModel.AutoCommit,
                TruncateLongNames = true,
                VirtualLock = false,
                UndoEnabled = true
            };
        }

        public static FirebirdOptions AutoCommitWithoutUndoBehaviour()
        {
            return new FirebirdOptions()
            {
                TransactionModel = FirebirdTransactionModel.AutoCommit,
                TruncateLongNames = true,
                VirtualLock = true,
                UndoEnabled = false
            };
        }
    }

}
