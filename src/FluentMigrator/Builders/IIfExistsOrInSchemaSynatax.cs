namespace FluentMigrator.Builders
{
    public interface IIfExistsOrInSchemaSynatax : IInSchemaSyntax
    {
        IInSchemaSyntax IfExists();
    }
}