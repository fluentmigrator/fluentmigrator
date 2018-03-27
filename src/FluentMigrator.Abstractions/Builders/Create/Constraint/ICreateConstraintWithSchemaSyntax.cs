namespace FluentMigrator.Builders.Create.Constraint
{
    public interface ICreateConstraintWithSchemaSyntax
    {
        ICreateConstraintColumnsSyntax WithSchema(string schemaName);
    }
}
