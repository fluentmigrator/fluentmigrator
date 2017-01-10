using System.Data;
using FluentMigrator.Infrastructure;
#if COREFX
using Rule = FluentMigrator.DataRule;
#endif

namespace FluentMigrator.Builders.Create.ForeignKey
{
    public interface ICreateForeignKeyCascadeSyntax : IFluentSyntax
    {
        ICreateForeignKeyCascadeSyntax OnDelete(Rule rule);
        ICreateForeignKeyCascadeSyntax OnUpdate(Rule rule);
        void OnDeleteOrUpdate(Rule rule);
    }
}
