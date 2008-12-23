using System;
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
			MigrationRunner runner = new MigrationRunner(null, Processor);
			
			//psuedocode
			// foreach (IMigration migration in Project.Attributes)
			//		runner.Up(migration);
		}
	}
}