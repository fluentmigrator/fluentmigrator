using FluentMigrator.Runner;
using FluentMigrator.Runner.Conventions;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.VersionTableInfo;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Moq;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.IssueTests.GH1836
{
    [TestFixture]
    [Category("Issue")]
    [Category("GH-1836")]
    public class Fixture
    {
        [Test]
        public void VersionLoaderShouldUseCustomVersionTableMetaDataImplementationRegisteredBeforeFluentMigrator()
        {
            var processor = new Mock<IMigrationProcessor>();

            var services = new ServiceCollection();

            // add custom registration before 'AddFluentMigratorCore()' call
            services.AddScoped<IVersionTableMetaData, CustomVersionTableMetaData>();

            // add minimal FluentMigrator registrations
            services
                .AddFluentMigratorCore()
                .WithProcessor(processor)
                .ConfigureRunner(x => x.AddSQLite())
                .AddScoped<IVersionLoader, VersionLoader>();

            var serviceProvider = services.BuildServiceProvider();

            var loader = serviceProvider.GetRequiredService<IVersionLoader>();

            loader.VersionTableMetaData.GetType().ShouldBe(typeof(CustomVersionTableMetaData));
        }

        [Test]
        public void VersionLoaderShouldUseCustomVersionTableMetaDataImplementationRegisteredAfterFluentMigrator()
        {
            var processor = new Mock<IMigrationProcessor>();

            var services = new ServiceCollection();

            // add minimal FluentMigrator registrations
            services
                .AddFluentMigratorCore()
                .ConfigureRunner(x => x.AddSQLite())
                .WithProcessor(processor)
                .AddScoped<IVersionLoader, VersionLoader>();

            // add custom registration after 'AddFluentMigratorCore()' call
            services.AddScoped<IVersionTableMetaData, CustomVersionTableMetaData>();

            var serviceProvider = services.BuildServiceProvider();

            var loader = serviceProvider.GetRequiredService<IVersionLoader>();

            loader.VersionTableMetaData.GetType().ShouldBe(typeof(CustomVersionTableMetaData));
        }

        private class CustomVersionTableMetaData : DefaultVersionTableMetaData
        {
#pragma warning disable CS0618 // Type or member is obsolete
            public CustomVersionTableMetaData()
            {
            }

            public CustomVersionTableMetaData(string schemaName)
                : base(schemaName)
            {
            }
#pragma warning restore CS0618 // Type or member is obsolete

            public CustomVersionTableMetaData(IConventionSet conventionSet, IOptions<RunnerOptions> runnerOptions)
                : base(conventionSet, runnerOptions)
            {
            }

            public override string TableName => "CustomVersionInfo";
        }
    }
}
