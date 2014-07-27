using FluentMigrator.Runner.Processors.Oracle;

using NUnit.Framework;

namespace FluentMigrator.Tests.Integration.Processors.Oracle
{
	[TestFixture]
	[Category( "Integration" )]
	public class OracleProcessorTests : OracleProcessorTestsBase {
        [SetUp]
        public void SetUp() {
	        base.SetUp( new OracleDbFactory() );
        }
	}
}
