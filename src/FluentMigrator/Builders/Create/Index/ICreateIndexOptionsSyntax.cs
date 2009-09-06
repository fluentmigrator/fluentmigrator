namespace FluentMigrator.Builders.Create.Index
{
	public interface ICreateIndexOptionsSyntax
	{
		ICreateIndexOptionsSyntax Unique();
		ICreateIndexOptionsSyntax NonClustered();
		ICreateIndexOptionsSyntax Clustered();
	}
}