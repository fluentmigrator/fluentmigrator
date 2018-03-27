using System.Linq;
using System.Collections.Generic;
using System;

using FluentMigrator.Runner.Processors.Oracle;

using NUnit.Framework;

namespace FluentMigrator.Tests.Integration.Processors.Oracle {
	[TestFixture]
	[Category( "Integration" )]
	public class OracleManagedConstraintTests : OracleConstraintTestsBase {
		[SetUp]
		public void SetUp( ) {
			base.SetUp( new OracleManagedDbFactory() );
		}
	}
}