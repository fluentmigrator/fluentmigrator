using System;

namespace FluentMigrator.Builders.Create.Index
{
    public interface ICreateIndexForTableSyntax
    {
        ICreateIndexOnColumnSyntax OnTable(string name);
    }
}