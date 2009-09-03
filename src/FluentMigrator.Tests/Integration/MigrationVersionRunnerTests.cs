<<<<<<< HEAD
﻿using System.Data.SqlClient;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Processors.SqlServer;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Integration
{
	[TestFixture]
	public class MigrationVersionRunnerTests
	{
		private string connectionString = @"server=(local)\SQLEXPRESS;uid=;pwd=;Trusted_Connection=yes;database=FluentMigrator";

		[Test]
		public void CanLoadMigrations()
		{
			var conventions = new MigrationConventions();
			var connection = new SqlConnection(connectionString);
			connection.Open();
			var processor = new SqlServerProcessor(connection, new SqlServerGenerator());

=======
﻿using FluentMigrator.Runner;
using Xunit;
using Xunit.Extensions;

namespace FluentMigrator.Tests.Integration
{
    public class MigrationVersionRunnerTests
    {
        [Theory]
        [Sqlite]
        [SqlServer]
        public void CanLoadMigrations(IMigrationProcessor processor)
        {   
            var conventions = new MigrationConventions();
            
>>>>>>> b903629e0b0bf79e322fc338e596cdc6d596059e
			var runner = new MigrationVersionRunner(conventions, processor, new MigrationLoader(conventions), typeof(MigrationVersionRunnerTests));

			runner.Migrations.ShouldNotBeNull();
		}

<<<<<<< HEAD
		[Test]
		public void CanLoadVersion()
		{
			var conventions = new MigrationConventions();
			var connection = new SqlConnection(connectionString);
			connection.Open();
			var processor = new SqlServerProcessor(connection, new SqlServerGenerator());

=======
		//[Fact]
		//public void CanLoadMigrationsByCallingAssembly()
		//{
		//    var conventions = new MigrationConventions();
		//    var connection = new SqlConnection(connectionString);
		//    connection.Open();
		//    var processor = new SqlServerProcessor(connection, new SqlServerGenerator());

		//    var runner = new MigrationVersionRunner(conventions, processor);

		//    Assert.NotNull(runner.Migrations);
		//}

        [Theory]
        [Sqlite]
        [SqlServer]
        public void CanLoadVersion(IMigrationProcessor processor)
        {
            var conventions = new MigrationConventions();
            
>>>>>>> b903629e0b0bf79e322fc338e596cdc6d596059e
			var runner = new MigrationVersionRunner(conventions, processor, new MigrationLoader(conventions), typeof(MigrationVersionRunnerTests));

			runner.Version.ShouldNotBeNull();
		}

<<<<<<< HEAD
		[Test]
		public void CanRunMigration()
		{
			var conventions = new MigrationConventions();
			var connection = new SqlConnection(connectionString);
			connection.Open();
			var processor = new SqlServerProcessor(connection, new SqlServerGenerator());
=======
        [Theory]
        [Sqlite]
        [SqlServer]
        public void CanRunMigration(IMigrationProcessor processor)
        {
            var conventions = new MigrationConventions();
>>>>>>> b903629e0b0bf79e322fc338e596cdc6d596059e

			var runner = new MigrationVersionRunner(conventions, processor, new MigrationLoader(conventions), typeof(MigrationVersionRunnerTests));

			runner.UpgradeToVersion(2, false);
			runner.Version.CurrentVersion.ShouldBe((long)2);

			//now step down to 0
			long last = 0;
			runner.StepDown(runner.CurrentVersion, 0, out last);
			runner.CurrentVersion.ShouldBe((long) 0);
		}

<<<<<<< HEAD
		[Test]
		public void CanUpdgradeToLatest()
		{
			var conventions = new MigrationConventions();
			var connection = new SqlConnection(connectionString);
			connection.Open();
			var processor = new SqlServerProcessor(connection, new SqlServerGenerator());
=======
        [Theory]
        [Sqlite]
        [SqlServer]
        public void CanUpdgradeToLatest(IMigrationProcessor processor)
        {
            var conventions = new MigrationConventions();
>>>>>>> b903629e0b0bf79e322fc338e596cdc6d596059e

			var runner = new MigrationVersionRunner(conventions, processor, new MigrationLoader(conventions), typeof(MigrationVersionRunnerTests));

			runner.UpgradeToLatest(false);

			//now step down to 0
			long last;
			runner.StepDown(runner.CurrentVersion, 0, out last);
			runner.CurrentVersion.ShouldBe((long)0);
		}
	}
}
