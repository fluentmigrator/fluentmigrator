using FluentMigrator.Runner.Generators.Generic;

namespace FluentMigrator.Runner.Generators.SQLite
{
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
    }
}
