using System;
using System.Collections.Generic;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Model
{
	public class IndexDefinition : ICanBeConventional, ICanBeValidated
	{
		public virtual string Name { get; set; }
		public virtual string TableName { get; set; }
		public virtual bool IsUnique { get; set; }
        public bool IsClustered { get; set; }
        public virtual ICollection<IndexColumnDefinition> Columns { get; set; }
	    
	    public IndexDefinition()
		{
			Columns = new List<IndexColumnDefinition>();
		}

		public void ApplyConventions(MigrationConventions conventions)
		{
			if (String.IsNullOrEmpty(Name))
				Name = conventions.GetIndexName(this);
		}

		public virtual void CollectValidationErrors(ICollection<string> errors)
		{
			if (String.IsNullOrEmpty(Name))
				errors.Add(ErrorMessages.IndexNameCannotBeNullOrEmpty);

			if (String.IsNullOrEmpty(TableName))
				errors.Add(ErrorMessages.TableNameCannotBeNullOrEmpty);

			if (Columns.Count == 0)
				errors.Add(ErrorMessages.IndexMustHaveOneOrMoreColumns);

			foreach (IndexColumnDefinition column in Columns)
				column.CollectValidationErrors(errors);
		}
	}
}