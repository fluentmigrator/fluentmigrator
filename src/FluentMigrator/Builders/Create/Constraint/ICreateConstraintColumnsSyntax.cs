namespace FluentMigrator.Builders.Create.Constraint
{
    public interface ICreateConstraintColumnsSyntax
    {
        ICreateConstraintOptionsSyntax Column(string columnName);
        ICreateConstraintOptionsSyntax Columns(string[] columnNames);
    }
}
