using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Processors.Oracle;
using Xunit;

namespace FluentMigrator.Tests.Integration.Processors.Oracle
{
	[TestFixture]
	[Category( "Integration" )]
	public class OracleSchemaTests : OracleSchemaTestsBase {
		[SetUp]
		public void SetUp( ) {
			base.SetUp( new OracleDbFactory() );
		}
	}
}
