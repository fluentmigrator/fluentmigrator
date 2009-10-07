using System;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Builders.Execute
{
    public interface IExecuteExpressionRoot : IFluentSyntax
    {
        void Sql(string sqlStatement);
    }
}