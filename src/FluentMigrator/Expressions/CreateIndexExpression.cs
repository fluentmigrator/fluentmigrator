using System;
using System.Collections.Generic;
using FluentMigrator.Model;

namespace FluentMigrator.Expressions
{
	public class CreateIndexExpression : IMigrationExpression
	{
		public string TableName { get; set; }
		
		private readonly IList<IndexColumnDefinition> columns = new List<IndexColumnDefinition>();
		public IList<IndexColumnDefinition> Columns
		{
			get
			{
				return columns;
			}
		}

		#region IMigrationExpression Members

		public void CollectValidationErrors(ICollection<string> errors)
		{
			if (String.IsNullOrEmpty(TableName))
				errors.Add(String.Format("The {0} does not have a valid table name", GetType().Name));

			foreach (IndexColumnDefinition indexColumn in columns)
			{
				indexColumn.CollectValidationErrors(errors);
			}			
		}

		public void ExecuteWith(IMigrationProcessor processor)
		{
			processor.Process(this);
		}

		#endregion		
	}
}