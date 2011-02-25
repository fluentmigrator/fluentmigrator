using FluentMigrator.Runner.Generators.Generic;

namespace FluentMigrator.Runner.Generators
{
	public class PostgreQuoter : GenericQuoter
	{
		public override string OpenQuote { get { return "\""; } }
		public override string CloseQuote { get { return "\""; } }
		public override string FormatBool(bool value) { return value ? "true" : "false"; }
	}
}