using System;
using System.Collections.Generic;
using System.Data;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Model
{
	public class ColumnDefinition : ICanBeValidated
	{
		public string Name { get; set; }
		public DbType Type { get; set; }
		public int Size { get; set; }
		public object DefaultValue { get; set; }
		public bool IsForeignKey { get; set; }
		public bool IsIdentity { get; set; }
		public bool IsIndexed { get; set; }
		public bool IsPrimaryKey { get; set; }
		public bool IsNullable { get; set; }
		public bool IsUnique { get; set; }

		public void CollectValidationErrors(ICollection<string> errors)
		{
		}
	}
}