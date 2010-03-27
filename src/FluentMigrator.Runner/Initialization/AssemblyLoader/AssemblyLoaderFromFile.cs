using System.IO;
using System.Reflection;

namespace FluentMigrator.Runner.Initialization.AssemblyLoader
{
	public class AssemblyLoaderFromFile : IAssemblyLoader
	{
		readonly string name;

		public AssemblyLoaderFromFile(string name)
		{
			this.name = name;
		}

		public Assembly Load()
		{
			string fileName = this.name;
			if (!Path.IsPathRooted(fileName))
			{
				fileName = Path.GetFullPath(this.name);
			}
			Assembly assembly = Assembly.LoadFile(fileName);
			return assembly;
		}
	}
}