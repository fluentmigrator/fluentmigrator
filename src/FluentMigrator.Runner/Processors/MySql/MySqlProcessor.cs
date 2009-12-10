using System;
using System.Data;
using MySql.Data.MySqlClient;

namespace FluentMigrator.Runner.Processors.MySql
{
	public class MySqlProcessor : ProcessorBase
	{
		public MySqlConnection Connection { get; set; }

		public MySqlProcessor(MySqlConnection connection, IMigrationGenerator generator)
		{
			this.generator = generator;
			Connection = connection;
		}

		public override bool TableExists(string tableName)
		{
			return Exists("select count(*) from sqlite_master where name='{0}'", tableName);
		}

		public override void Execute(string template, params object[] args)
		{
			if (Connection.State != ConnectionState.Open) Connection.Open();

			using (var command = new MySqlCommand(String.Format(template, args), Connection))
			{
				command.ExecuteNonQuery();
			}
		}

		public override bool Exists(string template, params object[] args)
		{
			if (Connection.State != ConnectionState.Open) Connection.Open();

			using (var command = new MySqlCommand(String.Format(template, args), Connection))
			using (var reader = command.ExecuteReader())
			{
				try
				{
					if (!reader.Read()) return false;
					if (int.Parse(reader[0].ToString()) <= 0) return false;
					return true;
				}
				catch
				{
					return false;
				}
			}
		}

		public override DataSet ReadTableData(string tableName)
		{
			return Read("select * from {0}", tableName);
		}

		protected override void Process(string sql)
		{
			if (Connection.State != ConnectionState.Open) Connection.Open();

			using (var command = new MySqlCommand(sql, Connection))
				command.ExecuteNonQuery();
		}

		public override DataSet Read(string template, params object[] args)
		{
			if (Connection.State != ConnectionState.Open) Connection.Open();

			DataSet ds = new DataSet();
			using (var command = new MySqlCommand(String.Format(template, args), Connection))
			using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
			{
				adapter.Fill(ds);
				return ds;
			}
		}
	}
}