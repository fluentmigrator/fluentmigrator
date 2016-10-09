using FluentMigrator.Runner.Processors.Oracle;

using Xunit;

namespace FluentMigrator.Tests.Integration.Processors.Oracle
{
    [Category("Integration")]
    public class OracleColumnTests : OracleColumnTestsBase {
		[SetUp]
		public void SetUp( ) {
			base.SetUp( new OracleDbFactory(  ) );
		}
    }
}
