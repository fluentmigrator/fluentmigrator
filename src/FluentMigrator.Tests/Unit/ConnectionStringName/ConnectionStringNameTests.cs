using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using FluentMigrator.Runner.Initialization;

namespace FluentMigrator.Tests.Unit.ConnectionStringName
{
	[TestFixture]
	class ConnectionStringNameTests
	{
		private static string GetPath(string relative) 
		{
			return string.Format(@"..\..\Unit\ConnectionStringName\Fixtures\{0}", relative);
		}

		private const string CONNECTION_NAME = "Test.Connection";

		[Test]
		public void ItFirstTriesConfigPath()
		{
			var sut = new NetConfigManager(GetPath(""), null);
			var result = sut.GetConnectionString(CONNECTION_NAME);
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
