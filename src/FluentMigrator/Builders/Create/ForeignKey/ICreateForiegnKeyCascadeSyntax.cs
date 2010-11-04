using System.Data;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Builders.Create.ForeignKey
{
	public interface ICreateForiegnKeyCascadeSyntax : IFluentSyntax
	{
		ICreateForiegnKeyCascadeSyntax OnDelete(Rule rule);
		ICreateForiegnKeyCascadeSyntax OnUpdate(Rule rule);
		void OnDeleteOrUpdate(Rule rule);
	}
}
