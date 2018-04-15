namespace FluentMigrator.Tests.Unit.Generators
{
    public abstract class BaseSequenceTests
    {
        public abstract void CanCreateSequenceWithCustomSchema();
        public abstract void CanCreateSequenceWithDefaultSchema();
        public abstract void CanDropSequenceWithCustomSchema();
        public abstract void CanDropSequenceWithDefaultSchema();
    }
}