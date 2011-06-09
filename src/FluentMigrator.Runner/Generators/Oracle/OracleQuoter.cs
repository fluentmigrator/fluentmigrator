using System;
using FluentMigrator.Runner.Generators.Generic;

namespace FluentMigrator.Runner.Generators.Oracle
{
    public class OracleQuoter : GenericQuoter
    {
       public bool CaseSensitiveNames { get; set; }

        //Not what to do here. Orcale quotes are " but if we do that then the column and table names become case
        //sensitive. For now we will just set it to not quote
       public override string OpenQuote { get { return CaseSensitiveNames ? "\"" : string.Empty; } }

       public override string CloseQuote { get { return CaseSensitiveNames ? "\"" : string.Empty; } }

        public override string QuoteValue(object value)
        {
           if ( value is Byte[])
           {
              return "hextoraw('" + BitConverter.ToString((Byte[])value).Replace("-","") + "')";
           }

           if (value is Guid)
           {
              return "hextoraw('" + ((Guid)value).ToString().Replace("-", "").ToUpper() + "')";
           }

           return base.QuoteValue(value);
        }

        public override string FormatDateTime(DateTime value)
        {
           return "to_date('" + (value).ToString("yyyy-MM-dd HH:mm:ss") + "', 'YYYY-MM-DD HH24:MI:SS')";
        }
        
    }
}
