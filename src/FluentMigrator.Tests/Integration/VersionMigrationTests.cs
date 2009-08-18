using System.Data.SqlClient;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Versioning;
using Xunit;

namespace FluentMigrator.Tests.Integration
{
    public class VersionMigrationTests
    {
        [Fact]
        public void CanUseVersionInfo()
        {
            string connectionString = @"server=(local)\SQLEXPRESS;uid=;pwd=;Trusted_Connection=yes;database=FluentMigrator";
            var conventions = new MigrationConventions();
            var connection = new SqlConnection(connectionString);
            connection.Open();
            var processor = new SqlServerProcessor(connection, new SqlServerGenerator());
            var runner = new MigrationRunner(conventions, processor);

            //ensure table doesn't exist
            if(processor.TableExists(VersionInfo.TABLE_NAME))
                runner.Down(new VersionMigration());

            runner.Up(new VersionMigration());
            Assert.True(processor.TableExists(VersionInfo.TABLE_NAME));
            runner.Down(new VersionMigration());
            Assert.False(processor.TableExists(VersionInfo.TABLE_NAME));
        }
    }
}
