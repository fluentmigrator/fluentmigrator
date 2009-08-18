using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Processors;

namespace FluentMigrator.Tests.Unit.Runners
{
	public class MigratorConsole
	{
		public string ProcessorType;
		public IMigrationProcessor Processor;
		public string Connection;
		public bool Log;
		private string TargetAssembly;

		public MigratorConsole(string[] args)
		{
			ParseArguments(args);
			CreateProcessor();
			ExecuteMigrations();
		}

		private void CreateProcessor()
		{
			IMigrationProcessorFactory processorFactory = ProcessorFactory.GetFactory(ProcessorType);            
			Processor = processorFactory.Create(Connection);
		}

		private void ParseArguments(string[] args)
		{
			for (int i = 0; i < args.Length; i++)
			{
				if (args[i].Contains("/db"))
					ProcessorType = args[i + 1];

				if (args[i].Contains("/connection"))
					Connection = args[i + 1];

				if (args[i].Contains("/target"))
					TargetAssembly = args[i + 1];

				if (args[i].Contains("/log"))
					Log = true;
			}

			if (string.IsNullOrEmpty(ProcessorType))
				throw new ArgumentException("Database Type is required (/database)");
			if (string.IsNullOrEmpty(Connection))
				throw new ArgumentException("Connection String is required (/connection");
		}

		private void ExecuteMigrations()
		{
			var runner = new MigrationRunner(null, Processor);
			if (!Path.IsPathRooted(TargetAssembly))
			{
				TargetAssembly = Path.GetFullPath(TargetAssembly);
			}
			Assembly assembly = Assembly.LoadFile(TargetAssembly);
			Type[] types = assembly.GetTypes();

			var migrations = new Dictionary<long,IMigration>();
			foreach (Type type in types)
			{
				if (type.IsDefined(typeof(MigrationAttribute), false))
				{
					var attributes = (MigrationAttribute[]) type.GetCustomAttributes(typeof (MigrationAttribute), false);				    
					migrations.Add(attributes[0].Version, (IMigration)assembly.CreateInstance(type.FullName));
				}
			}			
			
			foreach (long key in migrations.Keys.OrderBy(k => k))
			{
				runner.Up(migrations[key]);
			}			
		}		
	}
}