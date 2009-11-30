using System;
using System.Linq;
using System.Reflection;

namespace FluentMigrator.Runner.Processors
{
	public static class ProcessorFactory
	{
		public static IMigrationProcessorFactory GetFactory(string processorName)
		{
			Assembly assembly = typeof(IMigrationProcessorFactory).Assembly;

			Type processorType = FindMatchingProcessorIn(assembly, processorName);

			if (processorType == null)
				throw new ArgumentException("Processor Type not found");

			return Activator.CreateInstance(processorType) as IMigrationProcessorFactory;
		}

		private static Type FindMatchingProcessorIn(Assembly assembly, string processorName)
		{
			string fullProcessorName = string.Format("{0}ProcessorFactory", processorName);

			return assembly.GetExportedTypes()
				.Where(t => typeof(IMigrationProcessorFactory).IsAssignableFrom(t)
					&& t.Name.ToLower() == fullProcessorName.ToLower())
				.SingleOrDefault();
		}
	}
}