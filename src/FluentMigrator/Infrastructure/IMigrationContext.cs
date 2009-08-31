using System;
using System.Collections.Generic;
using FluentMigrator.Expressions;

namespace FluentMigrator.Infrastructure
{
	public interface IMigrationContext
	{
		IMigrationConventions Conventions { get; }
		ICollection<IMigrationExpression> Expressions { get; set; }
	}
}