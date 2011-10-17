using System;

namespace FluentMigrator
{
	public interface IMigrationMetadata
	{
		Type Type { get; }
		long Version { get; }
		bool Transactionless { get; }
	}
}