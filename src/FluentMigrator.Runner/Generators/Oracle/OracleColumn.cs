using System.Data;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators.Base;

namespace FluentMigrator.Runner.Generators.Oracle
{
	public class OracleColumn : ColumnBase
	{
      /// <summary>
      /// Determines if an exeption is thrown if a Identity column is specified
      /// </summary>
	   public bool ThrowExceptionIdentityNotSupported;

		public OracleColumn() : base(new OracleTypeMap(), new OracleQuoter())
		{
			int a = ClauseOrder.IndexOf(FormatDefaultValue);
			int b = ClauseOrder.IndexOf(FormatNullable);

			// Oracle requires DefaultValue before nullable
			if (a > b)
			{
				ClauseOrder[b] = FormatDefaultValue;
				ClauseOrder[a] = FormatNullable;
			}
		}

		protected override string FormatIdentity(ColumnDefinition column)
		{
         if (column.IsIdentity && ThrowExceptionIdentityNotSupported)
			{
				throw new DatabaseOperationNotSupportedExecption("Oracle does not support identity columns. Please use a SEQUENCE instead");
			}
			return string.Empty;
		}

		protected override string FormatSystemMethods(SystemMethods systemMethod)
		{
			switch (systemMethod)
			{
				case SystemMethods.NewGuid:
					return "sys_guid()";
            case SystemMethods.CurrentDateTime:
			      return "SYSDATE";
			}

			return null;
		}

      protected override string FormatDefaultValue(ColumnDefinition column)
      {
         if (column.DefaultValue is ColumnDefinition.UndefinedDefaultValue)
            return string.Empty;

         // see if this is for a system method
         if (column.DefaultValue is SystemMethods)
         {
            string method = FormatSystemMethods((SystemMethods)column.DefaultValue);
            if (string.IsNullOrEmpty(method))
               return string.Empty;

            return "DEFAULT " + method;
         }

         return "DEFAULT " + Quoter.QuoteValue(column.DefaultValue);
      }
	}
}
