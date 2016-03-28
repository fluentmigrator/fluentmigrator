namespace FluentMigrator.Builders.Create.Constraint
{
    public interface ICreateConstraintOnTableSyntax
    {
        ICreateConstraintWithSchemaOrColumnSyntax OnTable(string tableName);

        ICreateConstraintOnTableSyntax CheckIfExists(bool enabled = true);
    }
}
