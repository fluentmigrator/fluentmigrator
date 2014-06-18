using System.Data;
using System.Linq;
using System.Collections.Generic;
using System;

using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Generators.Oracle;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.Oracle;
using FluentMigrator.Tests.Helpers;

using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Integration.Processors.Oracle {
	[Category("Integration")]
	public abstract class OracleTableTestsBase : BaseTableTests
	{
		private const string SchemaName = "test";
		private IDbConnection Connection { get; set; }
		private OracleProcessor Processor { get; set; }
		private IDbFactory Factory { get; set; }

		protected void SetUp(IDbFactory dbFactory)
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

		[Test]
		public override void CallingTableExistsCanAcceptTableNameWithSingleQuote()
		{
			using (var table = new OracleTestTable("Test'Table", this.Connection, null, this.Factory, "id int"))
				this.Processor.TableExists(null, table.Name).ShouldBeTrue();
		}

		[Test]
		public override void CallingTableExistsReturnsFalseIfTableDoesNotExist()
		{
			this.Processor.TableExists(null, "DoesNotExist").ShouldBeFalse();
		}

		[Test]
		public override void CallingTableExistsReturnsFalseIfTableDoesNotExistWithSchema()
		{
			this.Processor.TableExists(SchemaName, "DoesNotExist").ShouldBeFalse();
		}

		[Test]
		public override void CallingTableExistsReturnsTrueIfTableExists()
		{
			using (var table = new OracleTestTable(this.Connection, null, this.Factory, "id int"))
				this.Processor.TableExists(null, table.Name).ShouldBeTrue();
		}

		[Test]
		public override void CallingTableExistsReturnsTrueIfTableExistsWithSchema()
		{
			using (var table = new OracleTestTable(this.Connection, SchemaName, this.Factory, "id int"))
				this.Processor.TableExists(SchemaName, table.Name).ShouldBeTrue();
		}
	}
}