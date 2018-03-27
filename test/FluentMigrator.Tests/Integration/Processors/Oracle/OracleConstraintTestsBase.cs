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
	public abstract class OracleConstraintTestsBase : BaseConstraintTests
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
		public override void CallingConstraintExistsCanAcceptConstraintNameWithSingleQuote()
		{
			using (var table = new OracleTestTable(this.Connection, null, this.Factory, "id int"))
			{
				table.WithUniqueConstraintOn("ID","UC'id");
				this.Processor.ConstraintExists(null, table.Name, "UC'id").ShouldBeTrue();
			}
		}

		[Test]
		public override void CallingConstraintExistsCanAcceptTableNameWithSingleQuote()
		{
			using( var table = new OracleTestTable( "Test'Table", this.Connection, null, this.Factory, "id int" ) )
			{
				table.WithUniqueConstraintOn("ID");
				this.Processor.ConstraintExists(null, table.Name, "UC_id").ShouldBeTrue();
			}
		}

		[Test]
		public override void CallingConstraintExistsReturnsFalseIfConstraintDoesNotExist()
		{
			using (var table = new OracleTestTable(this.Connection, null, this.Factory, "id int"))
			{
				table.WithUniqueConstraintOn("ID");
				this.Processor.ConstraintExists(null, table.Name, "DoesNotExist").ShouldBeFalse();
			}
		}

		[Test]
		public override void CallingConstraintExistsReturnsFalseIfConstraintDoesNotExistWithSchema()
		{
			using (var table = new OracleTestTable(this.Connection, SchemaName, this.Factory, "id int"))
			{
				table.WithUniqueConstraintOn("ID");
				this.Processor.ConstraintExists(SchemaName, table.Name, "DoesNotExist").ShouldBeFalse();
			}
		}

		[Test]
		public override void CallingConstraintExistsReturnsFalseIfTableDoesNotExist()
		{
			this.Processor.ConstraintExists(null, "DoesNotExist", "DoesNotExist").ShouldBeFalse();
		}

		[Test]
		public override void CallingConstraintExistsReturnsFalseIfTableDoesNotExistWithSchema()
		{
			this.Processor.ConstraintExists(SchemaName, "DoesNotExist", "DoesNotExist").ShouldBeFalse();
		}

		[Test]
		public override void CallingConstraintExistsReturnsTrueIfConstraintExists()
		{
			using (var table = new OracleTestTable(this.Connection, null, this.Factory, "id int"))
			{
				table.WithUniqueConstraintOn("ID");
				this.Processor.ConstraintExists(null, table.Name, "UC_id").ShouldBeTrue();
			}
		}

		[Test]
		public override void CallingConstraintExistsReturnsTrueIfConstraintExistsWithSchema()
		{
			using (var table = new OracleTestTable(this.Connection, SchemaName, this.Factory, "id int"))
			{
				table.WithUniqueConstraintOn("ID");
				this.Processor.ConstraintExists(SchemaName, table.Name, "UC_id").ShouldBeTrue();
			}
		}
	}
}