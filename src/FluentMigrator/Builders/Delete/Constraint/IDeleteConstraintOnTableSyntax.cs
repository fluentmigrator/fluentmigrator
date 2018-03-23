namespace FluentMigrator.Builders.Delete.Constraint
{
    public interface IDeleteConstraintOnTableSyntax
    {
        IDeleteConstraintInSchemaOptionsSyntax FromTable(string tableName);
    }
}
