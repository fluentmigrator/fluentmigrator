using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentMigrator.Model;

namespace FluentMigrator.Expressions
{
    

    public class CreateConstraintExpression : MigrationExpressionBase
    {
        public ConstraintDefinition Constraint { get; private set; }


       

        /// <summary>
        /// Initializes a new instance of the <see cref="T:CreateConstraintExpression"/> class.
        /// </summary>
        public CreateConstraintExpression(ConstraintType type)
        {
            Constraint = new ConstraintDefinition(type);

        }

        public override void ExecuteWith(IMigrationProcessor processor)
        {
            processor.Process(this);
        }

        public override void CollectValidationErrors(ICollection<string> errors)
        {
            Constraint.CollectValidationErrors(errors);
        }

        public override void ApplyConventions(IMigrationConventions conventions)
        {
            Constraint.ApplyConventions(conventions);
        }

        public override string ToString()
        {
          
            return base.ToString() + Constraint.ConstraintName;
        }
    }
}
