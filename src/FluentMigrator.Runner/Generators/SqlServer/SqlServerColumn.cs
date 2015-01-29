using FluentMigrator.Model;
using FluentMigrator.Runner.Extensions;
using FluentMigrator.Runner.Generators.Base;
using System.Linq;

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
            if (DefaultValueIsSqlFunction(column.DefaultValue))
                return "DEFAULT " + column.DefaultValue.ToString();

            var defaultValue = base.FormatDefaultValue(column);

            if (column.ModificationType == ColumnModificationType.Create && !string.IsNullOrEmpty(defaultValue))
                return "CONSTRAINT " + Quoter.QuoteConstraintName(GetDefaultConstraintName(column.TableName, column.Name)) + " " + defaultValue;

            return string.Empty;
        }

        public override string AddPrimaryKeyConstraint(string tableName, System.Collections.Generic.IEnumerable<ColumnDefinition> primaryKeyColumns)
        {
            string keyColumns = string.Join(", ", primaryKeyColumns.Select(x => Quoter.QuoteColumnName(x.Name)).ToArray());
            var isNonClustered = primaryKeyColumns.All(c => c.AdditionalFeatures.ContainsKey(SqlServerExtensions.ConstraintType) && 
                                                            (SqlServerConstraintType)c.AdditionalFeatures[SqlServerExtensions.ConstraintType] == SqlServerConstraintType.NonClustered);
            var clusterClause = isNonClustered ? "NONCLUSTERED " : "";
            return string.Format(", {0}PRIMARY KEY {2}({1})", GetPrimaryKeyConstraintName(primaryKeyColumns, tableName), keyColumns, clusterClause);
        }

        private static bool DefaultValueIsSqlFunction(object defaultValue)
        {
            return defaultValue is string && defaultValue.ToString().EndsWith("()");
        }

        protected override string FormatIdentity(ColumnDefinition column)
        {
            return column.IsIdentity ? GetIdentityString(column) : string.Empty;
        }

        private static string GetIdentityString(ColumnDefinition column)
        {
            return string.Format("IDENTITY({0},{1})",
                column.GetAdditionalFeature(SqlServerExtensions.IdentitySeed, 1),
                column.GetAdditionalFeature(SqlServerExtensions.IdentityIncrement, 1));
        }

        protected override string FormatSystemMethods(SystemMethods systemMethod)
        {
            switch (systemMethod)
            {
                case SystemMethods.NewGuid:
                    return "NEWID()";
                case SystemMethods.NewSequentialId:
                    return "NEWSEQUENTIALID()";
                case SystemMethods.CurrentDateTime:
                    return "GETDATE()";
                case SystemMethods.CurrentUTCDateTime:
                    return "GETUTCDATE()";
                case SystemMethods.CurrentUser:
                    return "CURRENT_USER";
            }

            return null;
        }

        public string FormatDefaultValue(object defaultValue)
        {
            if (DefaultValueIsSqlFunction(defaultValue))
                return defaultValue.ToString();

            if (defaultValue is SystemMethods)
                return FormatSystemMethods((SystemMethods)defaultValue);

            return Quoter.QuoteValue(defaultValue);
        }

        public static string GetDefaultConstraintName(string tableName, string columnName)
        {
            return string.Format("DF_{0}_{1}", tableName, columnName);
        }
    }
}
