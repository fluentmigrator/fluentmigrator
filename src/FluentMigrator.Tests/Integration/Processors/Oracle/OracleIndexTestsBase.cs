using System.Data;
using System.Linq;
using System.Collections.Generic;
using System;

using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Generators.Oracle;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.Oracle;
using FluentMigrator.Tests.Helpers;

using Xunit;

namespace FluentMigrator.Tests.Integration.Processors.Oracle {
	[Trait("Category", "Integration")]
	public abstract class OracleIndexTestsBase : BaseIndexTests, IDisposable
	{
		private const string SchemaName = "test";
		private IDbConnection Connection { get; set; }
		private OracleProcessor Processor { get; set; }
		private IDbFactory Factory { get; set; }

		protected OracleIndexTestsBase(IDbFactory dbFactory)
		{
			this.Factory = dbFactory;
			this.Connection = this.Factory.CreateConnection(IntegrationTestOptions.Oracle.ConnectionString);
			this.Processor = new OracleProcessor(this.Connection, new OracleGenerator(), new TextWriterAnnouncer(System.Console.Out), new ProcessorOptions(), this.Factory);
			this.Connection.Open();
		}

		public void Dispose()
		{
			this.Processor.Dispose();
		}

		[Fact]
		public override void CallingIndexExistsCanAcceptIndexNameWithSingleQuote()
		{
			using (var table = new OracleTestTable(this.Connection, null, this.Factory, "id int"))
			{
				table.WithIndexOn("ID", "UI'id");
				this.Processor.IndexExists(null, table.Name, "UI'id").ShouldBeTrue();
			}
		}

		[Fact]
		public override void CallingIndexExistsCanAcceptTableNameWithSingleQuote()
		{
			using (var table = new OracleTestTable("Test'Table", this.Connection, null, this.Factory, "id int"))
			{
				table.WithIndexOn("ID");
				this.Processor.IndexExists(null, table.Name, "UI_id").ShouldBeTrue();
			}
		}

		[Fact]
		public override void CallingIndexExistsReturnsFalseIfIndexDoesNotExist()
		{
			using (var table = new OracleTestTable(this.Connection, null, this.Factory, "id int"))
			{
				table.WithIndexOn("ID");
				this.Processor.IndexExists(null, table.Name, "DoesNotExist").ShouldBeFalse();
			}
		}

		[Fact]
		public override void CallingIndexExistsReturnsFalseIfIndexDoesNotExistWithSchema()
		{
			using (var table = new OracleTestTable(this.Connection, SchemaName, this.Factory, "id int"))
			{
				table.WithIndexOn("ID");
				this.Processor.IndexExists(SchemaName, table.Name, "DoesNotExist").ShouldBeFalse();
			}
		}

		[Fact]
		public override void CallingIndexExistsReturnsFalseIfTableDoesNotExist()
		{
			this.Processor.IndexExists(null, "DoesNotExist", "DoesNotExist").ShouldBeFalse();
		}

		[Fact]
		public override void CallingIndexExistsReturnsFalseIfTableDoesNotExistWithSchema()
		{
			this.Processor.IndexExists(SchemaName, "DoesNotExist", "DoesNotExist").ShouldBeFalse();
		}

		[Fact]
		public override void CallingIndexExistsReturnsTrueIfIndexExists()
		{
			using (var table = new OracleTestTable(this.Connection, null, this.Factory, "id int"))
			{
				table.WithIndexOn("ID");
				this.Processor.IndexExists(null, table.Name, "UI_id").ShouldBeTrue();
			}
		}

		[Fact]
		public override void CallingIndexExistsReturnsTrueIfIndexExistsWithSchema()
		{
			using (var table = new OracleTestTable(this.Connection, SchemaName, this.Factory, "id int"))
			{
				table.WithIndexOn("ID");
				this.Processor.IndexExists(SchemaName, table.Name, "UI_id").ShouldBeTrue();
			}
		}
	}
}