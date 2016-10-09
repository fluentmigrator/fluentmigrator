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
	public abstract class OracleColumnTestsBase : BaseColumnTests
	{
		private const string SchemaName = "test";
		private IDbConnection Connection { get; set; }
		private OracleProcessor Processor { get; set; }
		private IDbFactory Factory { get; set; }

		protected OracleColumnTestsBase(IDbFactory dbFactory)
		{
			this.Factory = dbFactory;
			this.Connection = this.Factory.CreateConnection(IntegrationTestOptions.Oracle.ConnectionString);
			this.Processor = new OracleProcessor(this.Connection, new OracleGenerator(), new TextWriterAnnouncer(System.Console.Out), new ProcessorOptions(), this.Factory);
			this.Connection.Open();
		}

		[TearDown]
		public void TearDown()
		{
			this.Processor.Dispose();
		}

		[Fact]
		public override void CallingColumnExistsCanAcceptColumnNameWithSingleQuote()
		{
			using (var table = new OracleTestTable(this.Connection, null, this.Factory, "\"i'd\" int"))
				this.Processor.ColumnExists(null, table.Name, "i'd").ShouldBeTrue();
		}

		[Fact]
		public override void CallingColumnExistsCanAcceptTableNameWithSingleQuote()
		{
			using( var table = new OracleTestTable( "Test'Table", this.Connection, null, this.Factory, "id int" ) )
				this.Processor.ColumnExists(null, table.Name, "DoesNotExist").ShouldBeFalse();
		}

		[Fact]
		public override void CallingColumnExistsReturnsFalseIfColumnDoesNotExist()
		{
			using (var table = new OracleTestTable(this.Connection, null, this.Factory, "id int"))
				this.Processor.ColumnExists(null, table.Name, "DoesNotExist").ShouldBeFalse();
		}

		[Fact]
		public override void CallingColumnExistsReturnsFalseIfColumnDoesNotExistWithSchema()
		{
			using (var table = new OracleTestTable(this.Connection, SchemaName, this.Factory, "id int"))
				this.Processor.ColumnExists(SchemaName, table.Name, "DoesNotExist").ShouldBeFalse();
		}

		[Fact]
		public override void CallingColumnExistsReturnsFalseIfTableDoesNotExist()
		{
			this.Processor.ColumnExists(null, "DoesNotExist", "DoesNotExist").ShouldBeFalse();
		}

		[Fact]
		public override void CallingColumnExistsReturnsFalseIfTableDoesNotExistWithSchema()
		{
			this.Processor.ColumnExists(SchemaName, "DoesNotExist", "DoesNotExist").ShouldBeFalse();
		}

		[Fact]
		public override void CallingColumnExistsReturnsTrueIfColumnExists()
		{
			using (var table = new OracleTestTable(this.Connection, null, this.Factory, "id int"))
				this.Processor.ColumnExists(null, table.Name, "id").ShouldBeTrue();
		}

		[Fact]
		public override void CallingColumnExistsReturnsTrueIfColumnExistsWithSchema()
		{
			using (var table = new OracleTestTable(this.Connection, SchemaName, this.Factory, "id int"))
				this.Processor.ColumnExists(SchemaName, table.Name, "id").ShouldBeTrue();
		}
	}
}