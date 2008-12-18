using System;

namespace FluentMigrator.Runner
{
	public interface IMigrationProcessorFactory
	{
		IMigrationProcessor Create(string connectionString);
	}
}