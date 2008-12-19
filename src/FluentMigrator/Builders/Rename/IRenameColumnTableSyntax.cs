using FluentMigrator.Builders.Rename.Column;

namespace FluentMigrator.Builders.Rename
{
    public interface IRenameColumnTableSyntax
    {
        IRenameColumnToSyntax OnTable(string tableName);
    }
}