using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.Sqlite;
using FluentMigrator.Runner.Processors.SqlServer;
using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.Runners
{
	[TestFixture]
	public class ProcessFactoryTests
	{
		[Test]
		public void CanRetrieveFactoryWithArgumentString()
		{
			IMigrationProcessorFactory factory = ProcessorFactory.GetFactory("Sqlite");
			Assert.IsTrue(factory.GetType() == typeof(SqliteProcessorFactory));
		}

		[Test]
		public void CanRetrieveSqlServerFactoryWithArgumentString()
		{
			IMigrationProcessorFactory factory = ProcessorFactory.GetFactory("SqlServer");
			Assert.IsTrue(factory.GetType() == typeof(SqlServerProcessorFactory));
		}
	}
}