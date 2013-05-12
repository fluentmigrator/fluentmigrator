namespace FluentMigrator.Tests.Integration.Processors
{
    public abstract class BaseIndexTests
    {
        public abstract void CallingIndexExistsCanAcceptIndexNameWithSingleQuote();
        public abstract void CallingIndexExistsCanAcceptTableNameWithSingleQuote();
        public abstract void CallingIndexExistsReturnsFalseIfIndexDoesNotExist();
        public abstract void CallingIndexExistsReturnsFalseIfIndexDoesNotExistWithSchema();
        public abstract void CallingIndexExistsReturnsFalseIfTableDoesNotExist();
        public abstract void CallingIndexExistsReturnsFalseIfTableDoesNotExistWithSchema();
        public abstract void CallingIndexExistsReturnsTrueIfIndexExists();
        public abstract void CallingIndexExistsReturnsTrueIfIndexExistsWithSchema();
    }
}