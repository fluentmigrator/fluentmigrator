using System;
using System.Collections.Generic;

namespace FluentMigrator.Expressions
{
	public abstract class MigrationExpressionBase : IMigrationExpression
	{
		public abstract void ExecuteWith(IMigrationProcessor processor);
		public abstract void CollectValidationErrors(ICollection<string> errors);

		public virtual IMigrationExpression Reverse()
		{
			throw new NotSupportedException(String.Format("The {0} cannot be automatically reversed", GetType().Name));
		}

	   public virtual void ApplyConventions( IMigrationConventions conventions )
	   {
         // By default do nothing, if an expression convention supports this, they should override
	   }

	   public override string ToString()
		{
			return GetType().Name.Replace("Expression", "") + " ";
		}
	}
}