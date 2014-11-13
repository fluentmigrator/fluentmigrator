using FluentMigrator.Runner.Generators.Generic;

namespace FluentMigrator.Runner.Generators.MySql
{
    public class MySqlQuoter : GenericQuoter
    {
        public override string OpenQuote { get { return "`"; } }

        public override string CloseQuote { get { return "`"; } }

        public override string QuoteValue(object value)
        {
            return base.QuoteValue(value).Replace(@"\", @"\\");
        }

        public override string FromTimeSpan(System.TimeSpan value)
        {
            return System.String.Format("{0}{1:00}:{2:00}:{3:00}{0}"
                , ValueQuote
                , value.Hours + (value.Days * 24)
                , value.Minutes
                , value.Seconds);
        }
    }
}
