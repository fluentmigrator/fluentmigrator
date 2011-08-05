using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.ConnectionStringName
{
	[TestFixture]
	class ConnectionStringNameTests
	{
		[Test]
		public void ItFirstTriesConfigPath()
		{

		}

		[Test]
		public void ItFailsIfTheConfigPathWasSpecifiedButCouldntResolve()
		{

		}

		[Test]
		public void ItTriesAppConfigSecond()
		{

		}

		[Test]
		public void ItFailsSilentlyOnMissingAppConfig()
		{

		}

		// TODO: figure out how to test machine.config settings
	}
}
