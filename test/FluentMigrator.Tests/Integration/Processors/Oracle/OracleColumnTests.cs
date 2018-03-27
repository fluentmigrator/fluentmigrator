using FluentMigrator.Runner.Processors.Oracle;

using NUnit.Framework;

namespace FluentMigrator.Tests.Integration.Processors.Oracle
{
    [TestFixture]
    [Category("Integration")]
    public class OracleColumnTests : OracleColumnTestsBase {
		[SetUp]
		public void SetUp( ) {
			base.SetUp( new OracleDbFactory(  ) );
		}
    }
}
