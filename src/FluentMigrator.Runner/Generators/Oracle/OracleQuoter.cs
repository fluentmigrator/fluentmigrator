using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentMigrator.Runner.Generators.Generic;

namespace FluentMigrator.Runner.Generators.Oracle
{
    public class OracleQuoter : GenericQuoter
    {
        //Not what to do here. Orcale quotes are " but if we do that then the column and table names become case
        //sensitive. For now we will just set it to not quote
        public override string OpenQuote { get { return string.Empty; } }

        public override string CloseQuote { get { return string.Empty; } }

        
    }
}
