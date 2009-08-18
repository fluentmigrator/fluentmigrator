using System.Reflection;

namespace FluentMigrator.Runner.Processors
{
	public static class ProcessorFactory
	{
		public static IMigrationProcessorFactory GetFactory(string processorName)
		{
			Assembly assembly = typeof(IMigrationProcessorFactory).Assembly;
			string type = string.Format("FluentMigrator.Runner.Processors.{0}ProcessorFactory", processorName);

			return assembly.CreateInstance(type, true) as IMigrationProcessorFactory;
		}
	}
}