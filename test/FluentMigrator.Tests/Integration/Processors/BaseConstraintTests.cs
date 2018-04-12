using NUnit.Framework;

namespace FluentMigrator.Tests.Integration.Processors
{
    [Category("Integration")]
    [Category("Constraint")]
    public abstract class BaseConstraintTests
    {
        public abstract void CallingConstraintExistsCanAcceptConstraintNameWithSingleQuote();
        public abstract void CallingConstraintExistsCanAcceptTableNameWithSingleQuote();
        public abstract void CallingConstraintExistsReturnsFalseIfConstraintDoesNotExist();
        public abstract void CallingConstraintExistsReturnsFalseIfConstraintDoesNotExistWithSchema();
        public abstract void CallingConstraintExistsReturnsFalseIfTableDoesNotExist();
        public abstract void CallingConstraintExistsReturnsFalseIfTableDoesNotExistWithSchema();
        public abstract void CallingConstraintExistsReturnsTrueIfConstraintExists();
        public abstract void CallingConstraintExistsReturnsTrueIfConstraintExistsWithSchema();
    }
}
