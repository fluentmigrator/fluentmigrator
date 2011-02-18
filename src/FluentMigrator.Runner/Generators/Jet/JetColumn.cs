

namespace FluentMigrator.Runner.Generators.Jet
{
    using System;
    using System.Data;
    using FluentMigrator.Model;
    using FluentMigrator.Runner.Generators.Base;

	internal class JetColumn : ColumnBase
	{
		public JetColumn() : base(new JetTypeMap(), new ConstantFormatter())
		{
		}

		protected override string FormatType(ColumnDefinition column)
		{
			if (column.IsIdentity)
			{
				// In Jet an identity column always of type COUNTER which is a integer type
				if (column.Type != DbType.Int32)
				{
					// TODO display a warning
				}

				return "COUNTER";
			}
			return base.FormatType(column);
		}
		
		protected override string FormatIdentity(ColumnDefinition column)
		{
			return string.Empty;
		}

		protected override string FormatName(ColumnDefinition column)
		{
			return String.Format("[{0}]", column.Name);
		}

		protected override string FormatPrimaryKey(ColumnDefinition column)
		{
			return column.IsPrimaryKey ? "PRIMARY KEY" : string.Empty;
		}

		protected override string FormatSystemMethods(SystemMethods systemMethod)
		{
			throw new NotImplementedException();
		}
	}
}
