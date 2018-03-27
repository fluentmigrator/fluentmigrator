using System;
using System.Collections.Generic;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Expressions
{
    public class CreateSchemaExpression : MigrationExpressionBase
    {
        public virtual string SchemaName { get; set; }

        public override void CollectValidationErrors(ICollection<string> errors)
        {
            if (String.IsNullOrEmpty(SchemaName))
                errors.Add(ErrorMessages.SchemaNameCannotBeNullOrEmpty);
        }

        public override void ExecuteWith(IMigrationProcessor processor)
        {
            processor.Process(this);
        }

        public override IMigrationExpression Reverse()
        {
            return new DeleteSchemaExpression { SchemaName = SchemaName };
        }

        public override string ToString()
        {
            return base.ToString() + SchemaName;
        }
    }
}
