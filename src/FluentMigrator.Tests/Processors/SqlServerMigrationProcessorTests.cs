using System;
using System.Data;
using System.Data.SqlClient;
using FluentMigrator.Expressions;
using FluentMigrator.Runner.Processors;
using Xunit;

namespace FluentMigrator.Tests.Processors
{
	public class SqlServerMigrationProcessorTests
	{
		public SqlConnection Connection { get; set; }
		public SqlServerMigrationProcessor Processor { get; set; }

		public SqlServerMigrationProcessorTests()
		{
			Connection = new SqlConnection("server=(local);uid=;pwd=;Trusted_Connection=yes;database=FluentMigrator");
			Connection.Open();
			Processor = new SqlServerMigrationProcessor(Connection);
		}

		[Fact]
		public void CallingTableExistsReturnsTrueIfTableExists()
		{
			using (var table = new SqlServerTestTable(Connection, "id int"))
				Assert.True(Processor.TableExists(table.Name));
		}

		[Fact]
		public void CallingTableExistsReturnsFalseIfTableDoesNotExist()
		{
			Assert.False(Processor.TableExists("DoesNotExist"));
		}

		[Fact]
		public void CallingColumnExistsReturnsTrueIfColumnExists()
		{
			using (var table = new SqlServerTestTable(Connection, "id int"))
				Assert.True(Processor.ColumnExists(table.Name, "id"));
		}

		[Fact]
		public void CallingColumnExistsReturnsFalseIfTableDoesNotExist()
		{
			Assert.False(Processor.ColumnExists("DoesNotExist", "DoesNotExist"));
		}

		[Fact]
		public void CallingColumnExistsReturnsFalseIfColumnDoesNotExist()
		{
			using (var table = new SqlServerTestTable(Connection, "id int"))
				Assert.False(Processor.ColumnExists(table.Name, "DoesNotExist"));
		}
	}
}