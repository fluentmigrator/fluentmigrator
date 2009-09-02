using System.Data.SqlClient;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Processors.SqlServer;
using FluentMigrator.Tests.Helpers;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Integration.Processors
{
	[TestFixture]
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

		[Test]
		public void CallingTableExistsReturnsTrueIfTableExists()
		{
			using (var table = new SqlServerTestTable(Connection, "id int"))
				Processor.TableExists(table.Name).ShouldBeTrue();
		}

		[Test]
		public void CallingTableExistsReturnsFalseIfTableDoesNotExist()
		{
			Processor.TableExists("DoesNotExist").ShouldBeFalse();
		}

		[Test]
		public void CallingColumnExistsReturnsTrueIfColumnExists()
		{
			using (var table = new SqlServerTestTable(Connection, "id int"))
				Processor.ColumnExists(table.Name, "id").ShouldBeTrue();
		}

		[Test]
		public void CallingColumnExistsReturnsFalseIfTableDoesNotExist()
		{
			Processor.ColumnExists("DoesNotExist", "DoesNotExist").ShouldBeFalse();
		}

		[Test]
		public void CallingColumnExistsReturnsFalseIfColumnDoesNotExist()
		{
			using (var table = new SqlServerTestTable(Connection, "id int"))
				Processor.ColumnExists(table.Name, "DoesNotExist").ShouldBeFalse();
		}
	}
}