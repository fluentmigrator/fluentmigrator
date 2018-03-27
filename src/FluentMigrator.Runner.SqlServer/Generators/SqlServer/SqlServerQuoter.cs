using FluentMigrator.Model;
using FluentMigrator.Runner.Generators.Generic;

namespace FluentMigrator.Runner.Generators.SqlServer
{
    public class SqlServerQuoter : GenericQuoter
    {
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
            if (value != null && value is ExplicitUnicodeString)
            {
                return string.Format("N{0}", FormatString(value.ToString()));
            }

            if (value != null && value is SystemMethods)
            {
                switch ((SystemMethods)value)
                {
                    case SystemMethods.NewGuid:
                        return "NEWID()";
                    case SystemMethods.NewSequentialId:
                        return "NEWSEQUENTIALID()";
                    case SystemMethods.CurrentDateTime:
                        return "GETDATE()";
                    case SystemMethods.CurrentUTCDateTime:
                        return "GETUTCDATE()";
                }
            }
            
            return base.QuoteValue(value);
        }
    }
}
