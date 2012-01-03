using FluentMigrator.Model;
using FluentMigrator.Runner.Generators.Base;

namespace FluentMigrator.Runner.Generators.SqlServer
{
    internal class SqlServerColumn : ColumnBase
    {
        public SqlServerColumn(ITypeMap typeMap)
            : base(typeMap, new SqlServerQuoter())
        {
        }

        protected override string FormatDefaultValue(ColumnDefinition column)
        {
            if (column.DefaultValue is string && column.DefaultValue.ToString().EndsWith("()"))
                return "DEFAULT " + column.DefaultValue.ToString();

            var defaultValue = base.FormatDefaultValue(column);

            if (column.ModificationType == ColumnModificationType.Create && !string.IsNullOrEmpty(defaultValue))
                return "CONSTRAINT " + GetDefaultConstraintName(column.TableName, column.Name) + defaultValue;

            return string.Empty;
        }

        protected override string FormatIdentity(ColumnDefinition column)
        {
            return column.IsIdentity ? "IDENTITY(1,1)" : string.Empty;
        }

        protected override string FormatSystemMethods(SystemMethods systemMethod)
        {
            switch (systemMethod)
            {
                case SystemMethods.NewGuid:
                    return "NEWID()";
                case SystemMethods.CurrentDateTime:
                    return "GETDATE()";
            }

            return null;
        }

        public static string GetDefaultConstraintName(string tableName, string columnName) {
            return string.Format("DF_{0}_{1} ", tableName, columnName);
        }
    }
}
