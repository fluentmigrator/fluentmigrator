namespace FluentMigrator.Runner.Initialization.AssemblyLoader
{
	public class AssemblyLoaderFactory
	{
		public static IAssemblyLoader GetAssemblyLoader(string name)
		{
			if (name.ToLower().Contains(".dll"))
			{
				return new AssemblyLoaderFromFile(name);
			}
			else
			{
				return new AssemblyLoaderFromName(name);
			}
		}
	}
}