using System.Collections.Generic;

using FluentMigrator.Infrastructure;
using FluentMigrator.Model;
using FluentMigrator.Validation;

namespace FluentMigrator.Expressions
{
    /// <summary>
    /// The expression to create a constraint
    /// </summary>
    public class CreateConstraintExpression : MigrationExpressionBase, ISupportAdditionalFeatures, IConstraintExpression, IValidationChildren
    {
        /// <inheritdoc />
        public virtual ConstraintDefinition Constraint { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateConstraintExpression"/> class.
        /// </summary>
        public CreateConstraintExpression(ConstraintType type)
        {
            // ReSharper disable once VirtualMemberCallInConstructor
            Constraint = new ConstraintDefinition(type);
        }

        /// <inheritdoc />
        public IDictionary<string, object> AdditionalFeatures => Constraint.AdditionalFeatures;

        /// <inheritdoc />
        public override void ExecuteWith(IMigrationProcessor processor)
        {
            processor.Process(this);
        }

        /// <inheritdoc />
        public override IMigrationExpression Reverse()
        {
            //constraint type is private in ConstraintDefinition
            return new DeleteConstraintExpression(Constraint.IsPrimaryKeyConstraint ? ConstraintType.PrimaryKey : ConstraintType.Unique)
            {
                Constraint = Constraint
            };
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return base.ToString() + Constraint.ConstraintName;
        }

        /// <inheritdoc />
        IEnumerable<object> IValidationChildren.Children
        {
            get { yield return Constraint; }
        }
    }
}
