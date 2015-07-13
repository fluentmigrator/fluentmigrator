using FluentMigrator.Model;

namespace FluentMigrator.Runner.Generators.SqlServer
{
    internal class SqlServerCeColumn : SqlServerColumn
    {
        public SqlServerCeColumn(ITypeMap typeMap) : base(typeMap)
        {
        }

        protected override string FormatNullable(ColumnDefinition column)
        {
            return column.IsNullable.GetValueOrDefault() ? "NULL" : "NOT NULL";
        }
    }
}