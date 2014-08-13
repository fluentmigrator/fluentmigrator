namespace FluentMigrator.Runner.Generators.DB2
{
    using System;

    using FluentMigrator.Runner.Generators.Generic;

    public class Db2Quoter : GenericQuoter
    {
        #region Fields

        public readonly char[] SpecialChars = "\"%'()*+|,{}-./:;<=>?^[]".ToCharArray();

        #endregion Fields

        #region Methods

        public override string FormatDateTime(DateTime value)
        {
            return this.ValueQuote + value.ToString("yyyy-MM-dd-HH.mm.ss") + this.ValueQuote;
        }

        public override string Quote(string name)
        {
            // Quotes are only included if the name contains a special character, in order to preserve case insensitivity where possible.
            return name.IndexOfAny(this.SpecialChars) != -1 ? base.Quote(name) : name;
        }

        #endregion Methods
    }
}