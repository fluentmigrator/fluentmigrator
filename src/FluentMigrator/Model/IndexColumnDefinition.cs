using System;
using System.Collections.Generic;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Model
{
	public class IndexColumnDefinition : ICanBeValidated
	{
		public virtual string Name { get; set; }
		public virtual Direction Direction { get; set; }

		public virtual void CollectValidationErrors(ICollection<string> errors)
		{
			if (String.IsNullOrEmpty(Name))
				errors.Add(ErrorMessages.ColumnNameCannotBeNullOrEmpty);
		}
	}
}