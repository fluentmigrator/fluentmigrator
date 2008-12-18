namespace FluentMigrator.Builders.Create
{
    public interface ICreateIndexOnColumnSyntax
    {
        ICreateIndexColumnOptionsSyntax OnColumn(string name);
    }

    public interface ICreateIndexColumnOptionsSyntax
    {
        ICreateIndexOnColumnSyntax Ascending();
        ICreateIndexOnColumnSyntax Descending();
    }
}