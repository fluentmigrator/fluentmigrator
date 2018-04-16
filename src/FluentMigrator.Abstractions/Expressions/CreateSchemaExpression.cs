using System;
using System.Collections.Generic;

using FluentMigrator.Infrastructure;

namespace FluentMigrator.Expressions
{
    /// <summary>
    /// Expression to create a schema
    /// </summary>
    public class CreateSchemaExpression : MigrationExpressionBase, ISupportAdditionalFeatures
    {
        /// <summary>
        /// Gets or sets the schema name
        /// </summary>
        public virtual string SchemaName { get; set; }

        /// <inheritdoc />
        public IDictionary<string, object> AdditionalFeatures { get; } = new Dictionary<string, object>();

        /// <inheritdoc />
        public override void CollectValidationErrors(ICollection<string> errors)
        {
            if (String.IsNullOrEmpty(SchemaName))
                errors.Add(ErrorMessages.SchemaNameCannotBeNullOrEmpty);
        }

        /// <inheritdoc />
        public override void ExecuteWith(IMigrationProcessor processor)
        {
            processor.Process(this);
        }

        /// <inheritdoc />
        public override IMigrationExpression Reverse()
        {
            return new DeleteSchemaExpression { SchemaName = SchemaName };
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return base.ToString() + SchemaName;
        }
    }
}
