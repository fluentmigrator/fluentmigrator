using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentMigrator.Runner.Generators.Generic;

namespace FluentMigrator.Runner.Generators.SQLite
{
    public class SqliteQuoter : GenericQuoter
    {
        public override string OpenQuote { get { return "'"; } }

        public override string CloseQuote { get { return "'"; } }
    }
}
