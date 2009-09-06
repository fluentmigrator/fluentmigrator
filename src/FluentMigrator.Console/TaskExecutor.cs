using FluentMigrator.Runner;

namespace FluentMigrator.Console
{
	public class TaskExecutor
	{
		private IMigrationVersionRunner _migrationVersionRunner;
		private long _version;
		private int _steps;

		public TaskExecutor(IMigrationVersionRunner processor, long version, int steps)
		{
			_migrationVersionRunner = processor;
			_steps = steps;
			_version = version;
		}

		public void Execute(string task)
		{
			switch (task)
			{
				case null:
				case "":
				case "migrate":
				case "migrate:up":
					if (_version != 0)
						_migrationVersionRunner.MigrateUp(_version);
					else
						_migrationVersionRunner.MigrateUp();
					break;
				case "rollback":
					if (_steps == 0)
						_steps = 1;
					_migrationVersionRunner.Rollback(_steps);
					break;
				case "migrate:down":
					_migrationVersionRunner.MigrateDown(_version);
					break;
			}
		}
	}
}
