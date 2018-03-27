namespace FluentMigrator.Expressions
{
    using System.Collections.Generic;
    using Model;

    public class CreateSequenceExpression : MigrationExpressionBase
    {
        public CreateSequenceExpression()
        {
            Sequence = new SequenceDefinition();
        }

        public virtual SequenceDefinition Sequence { get; set; }

        public override void ExecuteWith(IMigrationProcessor processor)
        {
            processor.Process(this);
        }

        public override void CollectValidationErrors(ICollection<string> errors)
        {
            Sequence.CollectValidationErrors(errors);
        }

        public override string ToString()
        {
            return base.ToString() + Sequence.Name;
        }
    }
}