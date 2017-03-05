namespace FluentMigrator.Tests.Integration.Processors
{
    public abstract class BaseSequenceTests : IntegrationTestBase
    {
        public abstract void CallingSequenceExistsReturnsFalseIfSequenceDoesNotExist();
        public abstract void CallingSequenceExistsReturnsFalseIfSequenceDoesNotExistWithSchema();
        public abstract void CallingSequenceExistsReturnsTrueIfSequenceExists();
        public abstract void CallingSequenceExistsReturnsTrueIfSequenceExistsWithSchema();
    }
}