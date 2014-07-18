using FluentMigrator.Runner.Generators.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FluentMigrator.Runner.Generators.DB2
{
    public class Db2Quoter : GenericQuoter
    {    
        public readonly char[] SpecialChars = "\"%'()*+|,{}-./:;<=>?^[]".ToCharArray();

        public override string FormatDateTime(DateTime value)
        {
            return ValueQuote + value.ToString("yyyy-MM-dd-HH.mm.ss") + ValueQuote;
        }

        public override string Quote(string name)
        {
            if (name.IndexOfAny(SpecialChars) != -1)
            {
                return base.Quote(name);
            }

            return name;
        }
    }
}
