using FluentMigrator.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FluentMigrator.Builders.Update.Column
{
    public interface IUpdateColumnFromSyntax : IFluentSyntax
    {
        IUpdateColumnOnTableSyntax From(string columnName);
    }
}
