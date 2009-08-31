using FluentMigrator.Tests.Unit.Runners;

namespace FluentMigrator.Console
{
	class Program
	{
		static void Main(string[] args)
		{
			var console = new MigratorConsole(args);			
		}
	}
}
