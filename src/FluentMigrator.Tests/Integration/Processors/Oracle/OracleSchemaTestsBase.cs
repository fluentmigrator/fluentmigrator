using System.Data;
using System.Linq;
using System.Collections.Generic;
using System;

using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Generators.Oracle;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.Oracle;

using Xunit;

namespace FluentMigrator.Tests.Integration.Processors.Oracle {
	[Trait("Category", "Integration")]
	public abstract class OracleSchemaTestsBase : BaseSchemaTests, IDisposable
	{
		private const string SchemaName = "test";
		private IDbConnection Connection { get; set; }
		private OracleProcessor Processor { get; set; }
		private IDbFactory Factory { get; set; }

		protected OracleSchemaTestsBase(IDbFactory dbFactory)
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
		public override void CallingSchemaExistsReturnsFalseIfSchemaDoesNotExist()
		{
			this.Processor.SchemaExists("DoesNotExist").ShouldBeFalse();
		}

		[Fact]
		public override void CallingSchemaExistsReturnsTrueIfSchemaExists()
		{
			this.Processor.SchemaExists(SchemaName).ShouldBeTrue();
		}
	}
}