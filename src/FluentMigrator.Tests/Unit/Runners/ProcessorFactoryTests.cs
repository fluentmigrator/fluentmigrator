using FluentMigrator.Runner;
using FluentMigrator.Runner.Processors;
using Xunit;

namespace FluentMigrator.Tests.Unit.Runners
{
	public class ProcessorFactoryTests
	{
		[Fact]
		public void CanRetrieveFactoryWithProcessorTypeString()
		{
			IMigrationProcessorFactory factory = ProcessorFactory.GetFactory("Sqlite");
			Assert.True(factory.GetType() == typeof(SqliteProcessorFactory));
		}
	}
}