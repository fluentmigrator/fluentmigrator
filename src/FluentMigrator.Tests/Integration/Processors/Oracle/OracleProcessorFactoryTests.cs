using System.Linq;
using System.Collections.Generic;
using System;

using FluentMigrator.Runner.Processors.Oracle;

using Xunit;

namespace FluentMigrator.Tests.Integration.Processors.Oracle
{
	[Category( "Integration" )]
	public class OracleProcessorFactoryTests : OracleProcessorFactoryTestsBase {
        [SetUp]
        public void SetUp()
        {
			base.SetUp( new OracleProcessorFactory() );
        }
	}
}