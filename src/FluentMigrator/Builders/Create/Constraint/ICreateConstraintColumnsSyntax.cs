namespace FluentMigrator.Builders.Create.Constraint
{
    public interface ICreateConstraintColumnsSyntax
    {
        void Column(string columnName);
        void Columns(string[] columnNames);
    }
}
