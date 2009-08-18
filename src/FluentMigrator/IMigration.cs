using System;
using FluentMigrator.Infrastructure;

namespace FluentMigrator
{
	public interface IMigration
	{
		void GetUpExpressions(IMigrationContext context);
		void GetDownExpressions(IMigrationContext context);
	}
}
