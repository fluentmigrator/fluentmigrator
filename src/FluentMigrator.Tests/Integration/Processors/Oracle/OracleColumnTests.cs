using FluentMigrator.Runner.Processors.Oracle;

using Xunit;

namespace FluentMigrator.Tests.Integration.Processors.Oracle
{
    [Trait("Category", "Integration")]
    public class OracleColumnTests : OracleColumnTestsBase {
		public void SetUp( ) {
			base.SetUp( new OracleDbFactory(  ) );
		}
    }
}
