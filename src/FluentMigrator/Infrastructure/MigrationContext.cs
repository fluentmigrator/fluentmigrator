using System;
using System.Collections.Generic;
using FluentMigrator.Expressions;

namespace FluentMigrator.Infrastructure
{
	public class MigrationContext : IMigrationContext
	{
		public virtual IMigrationConventions Conventions { get; set; }
		public virtual ICollection<IMigrationExpression> Expressions { get; set; }
		public virtual IQuerySchema QuerySchema { get; set; }

		public MigrationContext(IMigrationConventions conventions,IQuerySchema querySchema)
		{
			Conventions = conventions;
			Expressions = new List<IMigrationExpression>();
			QuerySchema = querySchema;
		}
	}
}