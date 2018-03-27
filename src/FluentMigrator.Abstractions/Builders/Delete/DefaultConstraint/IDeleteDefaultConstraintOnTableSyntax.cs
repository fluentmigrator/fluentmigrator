using FluentMigrator.Infrastructure;

namespace FluentMigrator.Builders.Delete.DefaultConstraint
{
    public interface IDeleteDefaultConstraintOnTableSyntax : IFluentSyntax
    {
        IDeleteDefaultConstraintOnColumnOrInSchemaSyntax OnTable(string tableName);
    }
}