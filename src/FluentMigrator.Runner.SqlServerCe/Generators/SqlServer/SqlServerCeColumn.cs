using FluentMigrator.Model;

namespace FluentMigrator.Runner.Generators.SqlServer
{
    internal class SqlServerCeColumn : SqlServer2000Column
    {
        public SqlServerCeColumn(ITypeMap typeMap, IQuoter quoter)
            : base(typeMap, quoter)
        {
        }

        /// <inheritdoc />
        protected override string FormatNullable(ColumnDefinition column)
        {
            if (column.IsNullable.GetValueOrDefault())
                return column.ModificationType == ColumnModificationType.Alter ? "NULL" : string.Empty;

            return "NOT NULL";
        }
    }
}
