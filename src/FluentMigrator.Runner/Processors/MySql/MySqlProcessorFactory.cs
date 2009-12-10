using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentMigrator.Runner.Generators;
using MySql.Data.MySqlClient;

namespace FluentMigrator.Runner.Processors.MySql
{
	public class MySqlProcessorFactory : IMigrationProcessorFactory
	{
		public IMigrationProcessor Create(string connectionString)
		{
			var connection = new MySqlConnection(connectionString);
			return new MySqlProcessor(connection, new MySqlGenerator());
		}
	}
}
