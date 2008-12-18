using System;

namespace FluentMigrator.Infrastructure
{
	public interface ICanBeConventional
	{
		void ApplyConventions(MigrationConventions conventions);
	}
}