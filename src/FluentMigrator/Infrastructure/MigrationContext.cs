using System;
using System.Collections.Generic;
using FluentMigrator.Expressions;

namespace FluentMigrator.Infrastructure
{
	public class MigrationContext : IMigrationContext
	{
		public virtual MigrationConventions Conventions { get; set; }
		public virtual ICollection<IMigrationExpression> Expressions { get; set; }

		public MigrationContext(MigrationConventions conventions)
		{
			Conventions = conventions;
			Expressions = new List<IMigrationExpression>();
		}
	}
}