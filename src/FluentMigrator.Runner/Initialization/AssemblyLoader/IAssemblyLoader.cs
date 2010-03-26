using System;
using System.Reflection;

namespace FluentMigrator.Runner.Initialization.AssemblyLoader
{
	public interface IAssemblyLoader
	{
		Assembly Load();
	}
}
