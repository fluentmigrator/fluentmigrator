using System.Reflection;

namespace FluentMigrator.Runner.Initialization.AssemblyLoader
{
	public class AssemblyLoaderFromName : IAssemblyLoader
	{
		private readonly string name;

		public AssemblyLoaderFromName(string name)
		{
			this.name = name;
		}

		public Assembly Load()
		{
			Assembly assembly = Assembly.Load(this.name);
			return assembly;
		}
	}
}