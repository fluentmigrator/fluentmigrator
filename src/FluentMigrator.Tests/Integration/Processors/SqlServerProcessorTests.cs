using System.Data.SqlClient;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Tests.Helpers;
using Xunit;

namespace FluentMigrator.Tests.Integration.Processors
{
	public class SqlServerProcessorTests
	{
		public SqlConnection Connection { get; set; }
		public SqlServerProcessor Processor { get; set; }

		public SqlServerProcessorTests()
		{
			Connection = new SqlConnection(@"server=(local)\sqlexpress;uid=;pwd=;Trusted_Connection=yes;database=FluentMigrator");
			Connection.Open();

			Processor = new SqlServerProcessor(Connection, new SqlServerGenerator());
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