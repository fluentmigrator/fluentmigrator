

namespace FluentMigrator.Runner.Generators.SqlServer
{
    using FluentMigrator.Model;
    using FluentMigrator.Runner.Generators.Base;
    using FluentMigrator.Runner.Generators.Generic;

	class SqlServerColumn : ColumnBase
	{
		public SqlServerColumn(ITypeMap typeMap) : base(typeMap, new SqlServerQuoter())
		{
		}

		protected override string FormatDefaultValue(ColumnDefinition column)
		{
			var defaultValue = base.FormatDefaultValue(column);

			if(!string.IsNullOrEmpty(defaultValue))
				return string.Format("CONSTRAINT DF_{0}_{1} ", column.TableName, column.Name) + defaultValue;

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
	}
}
