using System.Data.Common;

using Amazon.RDS.Util;

using FluentMigrator.Example.Migrations;
using FluentMigrator.Runner;

using Microsoft.Extensions.DependencyInjection;

using Npgsql;

namespace FluentMigrator.Example.AwsAuroraMigrator
{
    class Program
    {
        static void Main(string[] args)
        {
            var hostname = Environment.GetEnvironmentVariable("DB_HOSTNAME");
            var username = Environment.GetEnvironmentVariable("DB_USERNAME");
            var port = int.TryParse(Environment.GetEnvironmentVariable("DB_PORT"), out var p) ? p : 5432;
            var database = Environment.GetEnvironmentVariable("DB_DATABASE");

            var csb = new NpgsqlConnectionStringBuilder();
            csb.Host = hostname;
            csb.Username = username;
            csb.Port = port;
            csb.Database = database;
            csb.SslMode = SslMode.Require;

            using (var dbDataSource = CreateDbDataSource(csb))
            using (var serviceProvider = CreateServices(dbDataSource))
            using (var scope = serviceProvider.CreateScope())
            {
                // Put the database update into a scope to ensure
                // that all resources will be disposed.
                UpdateDatabase(scope.ServiceProvider);
            }
        }

        /// <summary>
        /// Configure the dependency injection services
        /// </summary>
        private static ServiceProvider CreateServices(DbDataSource dbDataSource)
        {
            var dbProviderFactory = new DelegatingDbProviderFactory(dbDataSource, NpgsqlFactory.Instance);
            return new ServiceCollection()
                // Add common FluentMigrator services
                .AddFluentMigratorCore()
                .ConfigureRunner(rb => rb
                    // Add SQLite support to FluentMigrator
                    .AddPostgres(dbProviderFactory)
                    // Define the assembly containing the migrations
                    .ScanIn(typeof(AddGTDTables).Assembly).For.Migrations())
                // Enable logging to console in the FluentMigrator way
                .AddLogging(lb => lb.AddFluentMigratorConsole())
                // Build the service provider
                .BuildServiceProvider(false);
        }

        /// <summary>
        /// Update the database
        /// </summary>
        private static void UpdateDatabase(IServiceProvider serviceProvider)
        {
            // Instantiate the runner
            var runner = serviceProvider.GetRequiredService<IMigrationRunner>();

            // Execute the migrations
            runner.MigrateUp();
        }

        private static DbDataSource CreateDbDataSource(NpgsqlConnectionStringBuilder connectionStringBuilder)
        {
            return new NpgsqlDataSourceBuilder(connectionStringBuilder.ToString())
            .UsePeriodicPasswordProvider((settings, cancellationToken) => ValueTask.FromResult(RDSAuthTokenGenerator.GenerateAuthToken(connectionStringBuilder.Host, connectionStringBuilder.Port, connectionStringBuilder.Username)), TimeSpan.FromMinutes(10), TimeSpan.FromSeconds(5))
            .Build();
        }
    }
}
