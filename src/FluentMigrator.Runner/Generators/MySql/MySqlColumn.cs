

namespace FluentMigrator.Runner.Generators.MySql
{
    using System;
    using FluentMigrator.Model;
    using FluentMigrator.Runner.Generators.Base;
    using FluentMigrator.Runner.Generators.Generic;

	internal class MySqlColumn : ColumnBase
	{
		public MySqlColumn() : base(new MySqlTypeMap(), new MySqlQuoter())
		{
		}


		protected override string FormatIdentity(ColumnDefinition column)
		{
			return column.IsIdentity ? "AUTO_INCREMENT" : string.Empty;
		}

        protected override string FormatSystemMethods(SystemMethods systemMethod)
        {
            throw new NotImplementedException();
        }
	}
}
