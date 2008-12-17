using System;
using System.Collections.Generic;
using System.Data;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Model
{
	public class ColumnDefinition : ICanBeValidated
	{
		public virtual string Name { get; set; }
		public virtual DbType Type { get; set; }
		public virtual int Size { get; set; }
		public virtual object DefaultValue { get; set; }
		public virtual bool IsForeignKey { get; set; }
		public virtual bool IsIdentity { get; set; }
		public virtual bool IsIndexed { get; set; }
		public virtual bool IsPrimaryKey { get; set; }
		public virtual bool IsNullable { get; set; }
		public virtual bool IsUnique { get; set; }
	    public int Precision { get; set; }

	    public virtual void CollectValidationErrors(ICollection<string> errors)
		{
		}
	}
}