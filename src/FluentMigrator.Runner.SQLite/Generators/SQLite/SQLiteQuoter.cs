using FluentMigrator.Runner.Generators.Generic;

namespace FluentMigrator.Runner.Generators.SQLite
{
    // ReSharper disable once InconsistentNaming
    public class SQLiteQuoter : GenericQuoter
    {
        public override string FormatSystemMethods(SystemMethods value)
        {
            switch (value)
            {
                case SystemMethods.CurrentUTCDateTime:
                    return "CURRENT_TIMESTAMP";
                case SystemMethods.CurrentDateTime:
                    return "(datetime('now','localtime'))";
            }

            return base.FormatSystemMethods(value);
        }

        public override string QuoteSchemaName(string schemaName)
        {
            return string.Empty;
        }

        protected override string FormatByteArray(byte[] value)
        {
            var hex = new System.Text.StringBuilder((value.Length * 2) + 2);
            hex.Append("X'");
            foreach (var b in value)
            {
                hex.AppendFormat("{0:x2}", b);
            }
            hex.Append("'");

            return hex.ToString();
        }
    }
}
