#region License
// 
// Copyright (c) 2007-2009, Sean Chambers <schambers80@gmail.com>
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

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
			Assembly assembly = typeof(IMigrationProcessorFactory).Assembly;

			Type processorType = FindMatchingProcessorIn(assembly, processorName);

			if (processorType == null)
				throw new ArgumentException("Processor Type not found");

			return Activator.CreateInstance(processorType) as IMigrationProcessorFactory;
		}

		public static string ListAvailableProcessorTypes()
		{
			IEnumerable<Type> processorTypes = typeof(IMigrationProcessorFactory).Assembly.GetExportedTypes()
				.Where(t => typeof(IMigrationProcessorFactory).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface);

			string processorList = string.Empty;
			foreach (Type processorType in processorTypes)
			{
				string name = processorType.Name;

				if (!string.IsNullOrEmpty(processorList))
					processorList = processorList + ", ";
				processorList += name.Substring( 0, name.IndexOf( "ProcessorFactory" ) ).ToLowerInvariant();
			}

			return processorList;
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