namespace FluentMigrator.Infrastructure
{
	public interface ICanBeConventional
	{
		void ApplyConventions(IMigrationConventions conventions);
	}
}