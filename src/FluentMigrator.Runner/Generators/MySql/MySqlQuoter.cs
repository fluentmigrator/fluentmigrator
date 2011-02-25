using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    }
}
