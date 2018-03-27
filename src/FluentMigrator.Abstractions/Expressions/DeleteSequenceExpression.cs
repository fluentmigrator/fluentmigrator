using System;
using System.Collections.Generic;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Expressions
{
    public class DeleteSequenceExpression : MigrationExpressionBase
    {
        public virtual string SchemaName { get; set; }
        public virtual string SequenceName { get; set; }

        public override void CollectValidationErrors(ICollection<string> errors)
        {
            if (String.IsNullOrEmpty(SequenceName))
                errors.Add(ErrorMessages.SequenceNameCannotBeNullOrEmpty);
        }

        public override void ExecuteWith(IMigrationProcessor processor)
        {
            processor.Process(this);
        }

        public override string ToString()
        {
            return base.ToString() + SequenceName;
        } 
    }
}