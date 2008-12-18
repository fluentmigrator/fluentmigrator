using System;
using System.Collections.Generic;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Model
{
	public class IndexColumnDefinition : ICanBeValidated
	{
		public string ColumnName { get; set; }
		public Direction Direction { get; set; }

		public void CollectValidationErrors(ICollection<string> errors)
		{
			if (String.IsNullOrEmpty(ColumnName))
				errors.Add(ErrorMessages.ColumnNameCannotBeNullOrEmpty);
		}
	}
}