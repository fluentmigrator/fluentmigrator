using FluentMigrator.Runner.Generators.Generic;

namespace FluentMigrator.Runner.Generators.Postgres
{
	public class PostgresQuoter : GenericQuoter
	{
		public override string FormatBool(bool value) { return value ? "true" : "false"; }

        public override string QuoteSchemaName(string schemaName)
        {
            if (string.IsNullOrEmpty(schemaName))
                schemaName = "public";
            return base.QuoteSchemaName(schemaName);
        }

		public string UnQuoteSchemaName(string quoted)
		{
			if (string.IsNullOrEmpty(quoted))
				return "public";

			return UnQuote(quoted);
		}
	}
}