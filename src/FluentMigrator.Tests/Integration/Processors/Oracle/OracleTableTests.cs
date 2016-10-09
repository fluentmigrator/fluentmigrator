using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Processors.Oracle;

using Xunit;

namespace FluentMigrator.Tests.Integration.Processors.Oracle
{
	[Category( "Integration" )]
	public class OracleTableTests : OracleTableTestsBase {
		[SetUp]
		public void SetUp( ) {
			base.SetUp( new OracleDbFactory() );
		}
	}
}
