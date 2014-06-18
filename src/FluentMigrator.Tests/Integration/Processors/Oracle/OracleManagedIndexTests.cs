using System.Linq;
using System.Collections.Generic;
using System;

using FluentMigrator.Runner.Processors.Oracle;

using NUnit.Framework;

namespace FluentMigrator.Tests.Integration.Processors.Oracle {
	[TestFixture]
	[Category( "Integration" )]
	public class OracleManagedIndexTests : OracleIndexTestsBase {
		[SetUp]
		public void SetUp( ) {
			base.SetUp( new OracleManagedDbFactory() );
		}
	}
}