using FluentMigrator.Model;
using FluentMigrator.Runner.Extensions;
using FluentMigrator.Runner.Generators.Base;

namespace FluentMigrator.Runner.Generators.SqlAnywhere
{
    internal class SqlAnywhereColumn : ColumnBase
    {
        public SqlAnywhereColumn(ITypeMap typeMap)
            : base(typeMap, new SqlAnywhereQuoter())
        {
            // Add UNIQUE before IDENTITY and after PRIMARY KEY
            this.ClauseOrder.Insert(this.ClauseOrder.Count - 2, this.FormatUniqueConstraint);
        }

        protected override string FormatDefaultValue(ColumnDefinition column)
        {
            if (DefaultValueIsSqlFunction(column.DefaultValue))
                return "DEFAULT " + column.DefaultValue.ToString();

            var defaultValue = base.FormatDefaultValue(column);

            if (!string.IsNullOrEmpty(defaultValue))
                return defaultValue;

            return string.Empty;
        }

        private static bool DefaultValueIsSqlFunction(object defaultValue)
        {
            return defaultValue is string && defaultValue.ToString().EndsWith("()");
        }

        protected virtual string FormatUniqueConstraint(ColumnDefinition column)
        {
            // Define unique constraints on columns in addition to creating a unique index
            return column.IsUnique ? "UNIQUE" : string.Empty;
        }

        protected override string FormatIdentity(ColumnDefinition column)
        {
            return column.IsIdentity ? GetIdentityString(column) : string.Empty;
        }

        private static string GetIdentityString(ColumnDefinition column)
        {
            return "DEFAULT AUTOINCREMENT";
            // Seeding doesn't seem to be supported
                //column.GetAdditionalFeature(SqlServerExtensions.IdentitySeed, 1),
                //column.GetAdditionalFeature(SqlServerExtensions.IdentityIncrement, 1));
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
