

namespace FluentMigrator.Runner.Generators.MySql
{
    using System;
    using FluentMigrator.Model;
    using FluentMigrator.Runner.Generators.Base;

	internal class MySqlColumn : ColumnBase
	{
		public MySqlColumn() : base(new MySqlTypeMap(), new ConstantFormatter())
		{
		}

		protected override string FormatIdentity(ColumnDefinition column)
		{
			return column.IsIdentity ? "AUTO_INCREMENT" : string.Empty;
		}

		protected override string FormatPrimaryKey(ColumnDefinition column)
		{
			return column.IsPrimaryKey ? string.Format(", PRIMARY KEY (`{0}`)", column.Name) : string.Empty;
		}

        protected override string FormatSystemMethods(SystemMethods systemMethod)
        {
            throw new NotImplementedException();
        }
	}
}
