using System;
using System.Data;
using FluentMigrator.Model;
using System.Collections.Generic;

namespace FluentMigrator.Runner.Generators
{
    public interface IColumn
    {
        string Generate(ColumnDefinition column);
        string Generate(IEnumerable<ColumnDefinition> columns, string tableName);
        string GenerateForeignKeyName(ForeignKeyDefinition foreignKey);
        string FormatForeignKey(ForeignKeyDefinition foreignKey, Func<ForeignKeyDefinition, string> fkNameGeneration);
        string FormatCascade(string onWhat, Rule rule);
    }
}
