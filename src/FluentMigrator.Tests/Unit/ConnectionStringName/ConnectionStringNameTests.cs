using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using FluentMigrator.Runner.Initialization;
using System.IO;

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
			var sut = new NetConfigManager(GetPath("WithConnectionString.config"), null);
			var result = sut.GetConnectionString(CONNECTION_NAME);
			Assert.That(result, Is.EqualTo("From Arbitrary Config"));
		}

		[Test]
		public void ItFailsIfTheConfigPathWasSpecifiedButCouldntResolveString()
		{
			var sut = new NetConfigManager(GetPath("WithWrongConnectionString.config"), null);
			Assert.Throws<ArgumentException>(() =>
				sut.GetConnectionString(CONNECTION_NAME));
		}

		[Test]
		public void ItFailsIfTheConfigPathWasSpecifiedButCouldntResolveFile()
		{
			var sut = new NetConfigManager(GetPath("WithWrongPath.config"), null);
			Assert.Throws<FileNotFoundException>(() =>
				sut.GetConnectionString(CONNECTION_NAME));
		}

		[Test]
		public void ItTriesAppConfigSecond()
		{
			var sut = new NetConfigManager("non-existent.config", "WithConnectionString.exe");
			var result = sut.GetConnectionString(CONNECTION_NAME);
			Assert.That(result, Is.EqualTo("From App Config"));
		}

		[Test]
		public void ItFailsSilentlyOnMissingAppConfig()
		{

		}

		[Test]
		public void ItFailsSilentlyOnMissingAppConfigConnectionString()
		{

		}

		// TODO: figure out how to test machine.config settings
	}
}
