namespace FluentMigrator.Builders.Delete.DefaultConstraint
{
    public interface IDeleteDefaultConstraintOnColumnOrInSchemaSyntax : IDeleteDefaultConstraintOnColumnSyntax
    {
        IDeleteDefaultConstraintOnColumnSyntax InSchema(string schemaName);
    }
}