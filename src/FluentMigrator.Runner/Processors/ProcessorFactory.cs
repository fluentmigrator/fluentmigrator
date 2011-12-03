using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FluentMigrator.Runner.Processors
{
	public static class ProcessorFactory
	{
		public static IMigrationProcessorFactory GetFactory(string processorName)
		{
			foreach (var factory in Factories)
			{
				var type = factory.GetType();
				var name = type.Name;
				if (name.StartsWith(processorName, StringComparison.OrdinalIgnoreCase))
				{
					return factory;
				}
			}

			return null;
		}

		public static string ListAvailableProcessorTypes()
		{
			IEnumerable<Type> processorTypes = typeof(IMigrationProcessorFactory).Assembly.GetExportedTypes()
				.Where(t => typeof(IMigrationProcessorFactory).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface);

			string processorList = string.Empty;
			foreach (Type processorType in processorTypes.OrderBy(x => x.Name))
			{
				string name = processorType.Name;

				if (!string.IsNullOrEmpty(processorList))
					processorList = processorList + ", ";
				processorList += name.Substring(0, name.IndexOf("ProcessorFactory")).ToLowerInvariant();
			}

			return processorList;
		}

		private static object factoriesLock = new object();
		private static List<IMigrationProcessorFactory> factories;
		public static IEnumerable<IMigrationProcessorFactory> Factories
		{
			get
			{
				if (factories == null)
				{
					lock (factoriesLock)
					{
						if (factories == null)
						{
							Assembly assembly = typeof(IMigrationProcessorFactory).Assembly;
							var types = assembly.GetExportedTypes().Where(t => typeof(IMigrationProcessorFactory).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract).ToList();
							factories = new List<IMigrationProcessorFactory>(types.Count);
							foreach (var type in types)
							{
								var instance = Activator.CreateInstance(type) as IMigrationProcessorFactory;
								if (instance != null)
								{
									factories.Add(instance);
								}
							}
						}
					}
				}

				return factories.OrderBy(x => x.Name);
			}
		}
	}
}