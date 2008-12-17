using System;
using System.Collections.Generic;
using FluentMigrator.Expressions;

namespace FluentMigrator.Infrastructure
{
	public class MigrationContext : IMigrationContext
	{
		public MigrationConventions Conventions { get; private set; }
		public ICollection<IMigrationExpression> Expressions { get; private set; }

		public MigrationContext(MigrationConventions conventions)
		{
			Conventions = conventions;
			Expressions = new List<IMigrationExpression>();
		}
	}
}