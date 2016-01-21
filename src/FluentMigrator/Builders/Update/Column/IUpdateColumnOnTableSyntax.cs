using FluentMigrator.Infrastructure;

namespace FluentMigrator.Builders.Update.Column
{
    public interface IUpdateColumnOnTableSyntax : IFluentSyntax
    {
        void OnTable(string name);
    }
}
