using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Versioning;
using Xunit;

namespace FluentMigrator.Tests.Integration
{
    public class MigrationVersionRunnerTests
    {
        [Fact]
        public void CanLoadMigrations()
        {
            string connectionString = @"server=(local)\SQLEXPRESS;uid=;pwd=;Trusted_Connection=yes;database=FluentMigrator";
            var conventions = new MigrationConventions();
            var connection = new SqlConnection(connectionString);
            connection.Open();
            var processor = new SqlServerProcessor(connection, new SqlServerGenerator());

            var runner = new MigrationVersionRunner(conventions, processor, typeof(MigrationVersionRunnerTests));

            Assert.NotNull(runner.Migrations);
        }

        [Fact]
        public void CanLoadVersion()
        {
            string connectionString = @"server=(local)\SQLEXPRESS;uid=;pwd=;Trusted_Connection=yes;database=FluentMigrator";
            var conventions = new MigrationConventions();
            var connection = new SqlConnection(connectionString);
            connection.Open();
            var processor = new SqlServerProcessor(connection, new SqlServerGenerator());

            var runner = new MigrationVersionRunner(conventions, processor, typeof(MigrationVersionRunnerTests));

            Assert.NotNull(runner.Version);            
        }

        [Fact]
        public void CanRunMigration()
        {
            string connectionString = @"server=(local)\SQLEXPRESS;uid=;pwd=;Trusted_Connection=yes;database=FluentMigrator";
            var conventions = new MigrationConventions();
            var connection = new SqlConnection(connectionString);
            connection.Open();
            var processor = new SqlServerProcessor(connection, new SqlServerGenerator());

            var runner = new MigrationVersionRunner(conventions, processor, typeof(MigrationVersionRunnerTests));

            runner.UpgradeToVersion(2, false);

            Assert.Equal<long>((long)2, runner.Version.CurrentVersion);

            //now step down to 0
            long last = 0;
            runner.StepDown(runner.CurrentVersion, 0, out last);

            Assert.Equal<long>((long)0, runner.CurrentVersion);
        }
    }
}
