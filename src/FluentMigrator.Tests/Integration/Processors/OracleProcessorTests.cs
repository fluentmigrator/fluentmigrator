

using System;
using System.Data;
using System.Data.Common;
using FluentMigrator.Runner.Processors.Oracle;
using NUnit.Framework;

namespace FluentMigrator.Tests.Integration.Processors
{
	using Runner.Processors;

	[TestFixture]
	public class OracleProcessorTests
	{
		private const string connectionString = "user id=FluentMigrator;password=FluentMigrator;Data Source=XE";

		[Test]
		[Explicit]
		public void TestQuery()
		{
			IDbFactory oracleFactory = new OracleDbFactory();
			var connection = oracleFactory.CreateConnection(connectionString);

			string sql = "Select * from Users";
			DataSet ds = new DataSet();
			using (var command = oracleFactory.CreateCommand(sql, connection))
			{
			    var adapter = oracleFactory.CreateDataAdapter(command);
				adapter.Fill(ds);
			}

			Assert.Greater(ds.Tables.Count,0);
			Assert.Greater(ds.Tables[0].Columns.Count,0);
		}
	}
}
