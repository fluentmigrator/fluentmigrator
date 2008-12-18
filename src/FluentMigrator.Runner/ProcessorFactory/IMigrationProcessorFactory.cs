using System;
using FluentMigrator.Processors;

namespace FluentMigrator.Runner
{
	public interface IMigrationProcessorFactory
	{
		IMigrationProcessor Create(string connectionString);
	}
}