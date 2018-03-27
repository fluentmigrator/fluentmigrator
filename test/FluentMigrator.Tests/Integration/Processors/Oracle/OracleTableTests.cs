using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Processors.Oracle;

using NUnit.Framework;

namespace FluentMigrator.Tests.Integration.Processors.Oracle
{
	[TestFixture]
	[Category( "Integration" )]
	public class OracleTableTests : OracleTableTestsBase {
		[SetUp]
		public void SetUp( ) {
			base.SetUp( new OracleDbFactory() );
		}
	}
}
