using FluentMigrator.Infrastructure;

namespace FluentMigrator.Builders.Delete.DefaultConstraint
{
    public interface IDeleteDefaultConstraintOnColumnSyntax : IFluentSyntax
    {
        void OnColumn(string columnName);
    }
}