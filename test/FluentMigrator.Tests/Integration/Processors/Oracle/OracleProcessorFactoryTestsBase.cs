using System.Linq;
using System.Collections.Generic;
using System;

using FluentMigrator.Runner;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Generators.Oracle;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.Oracle;

using NUnit.Framework;

namespace FluentMigrator.Tests.Integration.Processors.Oracle {
	[Category( "Integration" )]
	public abstract class OracleProcessorFactoryTestsBase
	{
		private IMigrationProcessorFactory factory;
		private string connectionString;
		private IAnnouncer announcer;
		private ProcessorOptions options;

		protected void SetUp(IMigrationProcessorFactory processorFactory)
		{
			this.factory = processorFactory;
			this.connectionString = "Data Source=localhost/XE;User Id=Something;Password=Something";
			this.announcer = new NullAnnouncer();
			this.options = new ProcessorOptions();
		}

		[TestCase("")]
		[TestCase(null)]
		public void CreateProcessorWithNoProviderSwitchesShouldUseOracleQuoter(string providerSwitches)
		{
			this.options.ProviderSwitches = providerSwitches;
			var processor = this.factory.Create(this.connectionString, this.announcer, this.options);
			Assert.That(((OracleProcessor)processor).Quoter, Is.InstanceOf<OracleQuoter>());
		}

		[TestCase("QuotedIdentifiers=true")]
		[TestCase("QuotedIdentifiers=TRUE;")]
		[TestCase("QuotedIDENTIFIERS=TRUE;")]
		[TestCase("QuotedIdentifiers=true;somethingelse=1")]
		[TestCase("somethingelse=1;QuotedIdentifiers=true")]
		[TestCase("somethingelse=1;QuotedIdentifiers=true;sometingOther='special thingy'")]
		public void CreateProcessorWithProviderSwitchIndicatingQuotedShouldUseOracleQuoterQuotedIdentifier(string providerSwitches)
		{
			this.options.ProviderSwitches = providerSwitches;
			var processor = this.factory.Create(this.connectionString, this.announcer, this.options);
			Assert.That(((OracleProcessor)processor).Quoter, Is.InstanceOf<OracleQuoterQuotedIdentifier>());
		}
	}
}