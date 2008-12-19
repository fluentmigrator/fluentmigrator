using System.Data.SQLite;

namespace FluentMigrator.Runner.Processors
{
	public class SqliteProcessor: ProcessorBase
	{
		public SQLiteConnection Connection { get; set; }

		public SqliteProcessor(SQLiteConnection connection, IMigrationGenerator generator)
		{
			this.generator = generator;
			Connection = connection;
		}
        
		protected override void Process(string sql)
		{
			using (var command = new SQLiteCommand(sql, Connection))
				command.ExecuteNonQuery();
		}
	}
}