using FluentMigrator.Runner.Generators.Generic;

namespace FluentMigrator.Runner.Generators.Firebird
{
    public class FirebirdQuoter : GenericQuoter
    {
        public override string OpenQuote { get { return "\""; } }
        public override string CloseQuote { get { return "\""; } }

        public override string FormatDateTime(System.DateTime value)
        {
            return ValueQuote + (value).ToString("yyyy-MM-dd HH:mm:ss") + ValueQuote;
        }
    }
}