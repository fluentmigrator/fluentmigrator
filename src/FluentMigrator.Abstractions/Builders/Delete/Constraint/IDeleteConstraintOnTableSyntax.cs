namespace FluentMigrator.Builders.Delete.Constraint
{
    public interface IDeleteConstraintOnTableSyntax
    {
        IInSchemaSyntax FromTable(string tableName);
    }
}
