using FluentMigrator.Runner.Processors.Oracle;

using Xunit;

namespace FluentMigrator.Tests.Integration.Processors.Oracle
{
	[Trait("Category", "Integration")]
	public class OracleProcessorTests : OracleProcessorTestsBase {
        public OracleProcessorTests()
            : base(new OracleDbFactory())
        {
        }
	}
}

