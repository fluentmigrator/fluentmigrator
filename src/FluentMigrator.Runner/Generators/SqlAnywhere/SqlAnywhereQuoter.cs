using FluentMigrator.Model;
using FluentMigrator.Runner.Generators.Generic;
using System;

namespace FluentMigrator.Runner.Generators.SqlAnywhere
{
    public class SqlAnywhereQuoter : GenericQuoter
    {
        public override string FormatEnum(object value)
        {
            if (value is SystemMethods)
            {
                switch((SystemMethods)value)
                {
                    case SystemMethods.NewGuid:
                        return "NEWID()";
                    case SystemMethods.NewSequentialId:
                        return "AUTOINCREMENT";
                    case SystemMethods.CurrentDateTime:
                        return "TIMESTAMP";
                    case SystemMethods.CurrentUTCDateTime:
                        return "UTC TIMESTAMP";
                    case SystemMethods.CurrentUser:
                        return "LAST USER";
                    default:
                        throw new NotImplementedException("FormatEnum not implemented for SystemMethods." + value.ToString());
                }
            }

            return base.FormatEnum(value);
        }

        public override string OpenQuote { get { return "["; } }

        public override string CloseQuote { get { return "]"; } }

        public override string CloseQuoteEscapeString { get { return "]]"; } }

        public override string OpenQuoteEscapeString { get { return string.Empty; } }

        public override string QuoteSchemaName(string schemaName)
        {
            return (string.IsNullOrEmpty(schemaName)) ? "[dbo]" : Quote(schemaName);
        }

        public override string QuoteValue(object value)
        {
            if (value == null)
            {
                return "(null)";
            } 
            else if (value != null && value is ExplicitUnicodeString)
            {
                return string.Format("N{0}", FormatString(value.ToString()));
            }

            return base.QuoteValue(value);
        }
    }
}