namespace FluentMigrator.Builders.Delete.Table
{
	public interface IDeleteTableFromSchemaSyntax
	{
		void InSchema(string schemaName);
	}
}