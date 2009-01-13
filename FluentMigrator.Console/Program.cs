using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
