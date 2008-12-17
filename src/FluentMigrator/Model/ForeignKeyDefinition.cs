using System;
using System.Collections.Generic;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Model
{
	public class ForeignKeyDefinition : ICanBeValidated
	{
		public string ForeignTable { get; set; }
		public string PrimaryTable { get; set; }
		public ICollection<string> ForeignColumns { get; set; }
		public ICollection<string> PrimaryColumns { get; set; }

		public ForeignKeyDefinition()
		{
			ForeignColumns = new List<string>();
			PrimaryColumns = new List<string>();
		}

		public void CollectValidationErrors(ICollection<string> errors)
		{
			if (String.IsNullOrEmpty(ForeignTable))
				errors.Add(String.Format("The {0} does not have a valid foreign table name.", GetType().Name));

			if (String.IsNullOrEmpty(PrimaryTable))
				errors.Add(String.Format("The {0} does not have a valid primary table name.", GetType().Name));

			if (ForeignColumns.Count == 0)
				errors.Add(String.Format("The {0} does not have one or more foreign column names.", GetType().Name));

			if (PrimaryColumns.Count == 0)
				errors.Add(String.Format("The {0} does not have one or more primary column names.", GetType().Name));
		}
	}
}