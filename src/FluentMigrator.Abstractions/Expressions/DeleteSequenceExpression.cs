using System;
using System.Collections.Generic;

using FluentMigrator.Infrastructure;

namespace FluentMigrator.Expressions
{
    /// <summary>
    /// Expression to delete a sequence
    /// </summary>
    public class DeleteSequenceExpression : MigrationExpressionBase, ISchemaExpression
    {
        /// <inheritdoc />
        public virtual string SchemaName { get; set; }

        /// <summary>
        /// Gets or sets the sequence name
        /// </summary>
        public virtual string SequenceName { get; set; }

        /// <inheritdoc />
        public override void CollectValidationErrors(ICollection<string> errors)
        {
            if (String.IsNullOrEmpty(SequenceName))
                errors.Add(ErrorMessages.SequenceNameCannotBeNullOrEmpty);
        }

        /// <inheritdoc />
        public override void ExecuteWith(IMigrationProcessor processor)
        {
            processor.Process(this);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return base.ToString() + SequenceName;
        }
    }
}
