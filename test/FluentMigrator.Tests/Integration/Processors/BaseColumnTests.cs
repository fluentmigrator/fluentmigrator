namespace FluentMigrator.Tests.Integration.Processors
{
    public abstract class BaseColumnTests
    {
        public abstract void CallingColumnExistsCanAcceptColumnNameWithSingleQuote();
        public abstract void CallingColumnExistsCanAcceptTableNameWithSingleQuote();
        public abstract void CallingColumnExistsReturnsFalseIfColumnDoesNotExist();
        public abstract void CallingColumnExistsReturnsFalseIfColumnDoesNotExistWithSchema();
        public abstract void CallingColumnExistsReturnsFalseIfTableDoesNotExist();
        public abstract void CallingColumnExistsReturnsFalseIfTableDoesNotExistWithSchema();
        public abstract void CallingColumnExistsReturnsTrueIfColumnExists();
        public abstract void CallingColumnExistsReturnsTrueIfColumnExistsWithSchema();
    }
}