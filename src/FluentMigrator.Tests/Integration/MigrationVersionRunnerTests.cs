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
        private string connectionString = @"server=(local)\SQLEXPRESS;uid=;pwd=;Trusted_Connection=yes;database=FluentMigrator";

        [Fact]
        public void CanLoadMigrations()
        {   
            var conventions = new MigrationConventions();
            var connection = new SqlConnection(connectionString);
            connection.Open();
            var processor = new SqlServerProcessor(connection, new SqlServerGenerator());

            var runner = new MigrationVersionRunner(conventions, processor, new MigrationLoader(conventions), typeof(MigrationVersionRunnerTests));

            Assert.NotNull(runner.Migrations);
        }

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

        [Fact]
        public void CanLoadVersion()
        {
            var conventions = new MigrationConventions();
            var connection = new SqlConnection(connectionString);
            connection.Open();
            var processor = new SqlServerProcessor(connection, new SqlServerGenerator());

            var runner = new MigrationVersionRunner(conventions, processor, new MigrationLoader(conventions), typeof(MigrationVersionRunnerTests));

            Assert.NotNull(runner.Version);            
        }

        [Fact]
        public void CanRunMigration()
        {
            var conventions = new MigrationConventions();
            var connection = new SqlConnection(connectionString);
            connection.Open();
            var processor = new SqlServerProcessor(connection, new SqlServerGenerator());

            var runner = new MigrationVersionRunner(conventions, processor, new MigrationLoader(conventions), typeof(MigrationVersionRunnerTests));

            runner.UpgradeToVersion(2, false);

            Assert.Equal<long>((long)2, runner.Version.CurrentVersion);

            //now step down to 0
            long last = 0;
            runner.StepDown(runner.CurrentVersion, 0, out last);

            Assert.Equal<long>((long)0, runner.CurrentVersion);
        }

        [Fact]
        public void CanUpdgradeToLatest()
        {
            var conventions = new MigrationConventions();
            var connection = new SqlConnection(connectionString);
            connection.Open();
            var processor = new SqlServerProcessor(connection, new SqlServerGenerator());

            var runner = new MigrationVersionRunner(conventions, processor, new MigrationLoader(conventions), typeof(MigrationVersionRunnerTests));

            runner.UpgradeToLatest(false);

            Assert.True(true); //made it this far..

            //now step down to 0
            long last = 0;
            runner.StepDown(runner.CurrentVersion, 0, out last);

            Assert.Equal<long>((long)0, runner.CurrentVersion);
        }
    }
}
