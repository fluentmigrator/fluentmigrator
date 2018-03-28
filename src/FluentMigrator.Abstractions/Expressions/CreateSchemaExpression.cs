using System;
using System.Collections.Generic;

using FluentMigrator.Infrastructure;

namespace FluentMigrator.Expressions
{
    public class CreateSchemaExpression : MigrationExpressionBase, ISupportAdditionalFeatures
    {
        public virtual string SchemaName { get; set; }

        public IDictionary<string, object> AdditionalFeatures { get; } = new Dictionary<string, object>();

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
