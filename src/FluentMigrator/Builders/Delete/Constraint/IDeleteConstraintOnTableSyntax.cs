namespace FluentMigrator.Builders.Delete.Constraint
{
    public interface IDeleteConstraintOnTableSyntax
    {
        void FromTable(string tableName);
    }
}
