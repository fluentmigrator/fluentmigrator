using System;
using FluentMigrator.Runner.Generators.Generic;

namespace FluentMigrator.Runner.Generators.Oracle
{
    public class OracleQuoter : GenericQuoter
    {
        public override string FormatDateTime(DateTime value)
        {
            var result = string.Format("to_date({0}{1}{0}, {0}yyyy-mm-dd hh24:mi:ss{0})", ValueQuote, value.ToString("yyyy-MM-dd HH:mm:ss")); //ISO 8601 DATETIME FORMAT (EXCEPT 'T' CHAR)
            return result;
        }

		public override string QuoteColumnName(string columnName)
		{
			return columnName;
		}

		public override string QuoteConstraintName(string constraintName)
		{
			return constraintName;
		}

		public override string QuoteIndexName(string indexName)
		{
			return indexName;
		}

		public override string QuoteSchemaName(string schemaName)
		{
			return schemaName;
		}

		public override string QuoteTableName(string tableName)
		{
			return tableName;
		}
		public override string QuoteSequenceName(string sequenceName)
		{
			return sequenceName;
		}

        public override string FromTimeSpan(TimeSpan value)
        {
            return String.Format("{0}{1} {2}:{3}:{4}.{5}{0}"
                , ValueQuote
                , value.Days
                , value.Hours
                , value.Minutes
                , value.Seconds
                , value.Milliseconds);
        }
    }

}