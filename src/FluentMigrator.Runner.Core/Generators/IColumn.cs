using FluentMigrator.Model;
using System.Collections.Generic;

namespace FluentMigrator.Runner.Generators
{
    public interface IColumn
    {
        string Generate(ColumnDefinition column);
        string Generate(IEnumerable<ColumnDefinition> columns, string tableName);
    }
}
