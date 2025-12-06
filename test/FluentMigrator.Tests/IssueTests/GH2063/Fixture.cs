using System.Globalization;
using System.IO;

using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

namespace FluentMigrator.Tests.IssueTests.GH2063
{
    [TestFixture]
    [Category("Issue")]
    [Category("GH-2063")]
    [SetCulture("nn-NO")] // Set the culture to Norwegian (Nynorsk) to test the issue with integer conversion
    public class Fixture
    {
        private string _sqliteDbFileName;
        private ServiceProvider _serviceProvider;

        [SetUp]
        public void SetUp()
        {
            CultureInfo.CurrentCulture.NumberFormat.NegativeSign = "−"; // Force the minus sign from the Norwegian (Nynorsk) culture

            _sqliteDbFileName = Path.GetTempFileName();
            _serviceProvider = new ServiceCollection()
                .AddFluentMigratorCore()
                .ConfigureRunner(
                    rb => rb
                        .AddSQLite()
                        .WithGlobalConnectionString($"Data Source={_sqliteDbFileName};Pooling=False;") // Must disable pooling otherwise SQLite won't release lock on DB
                        .ScanIn(typeof(Fixture).Assembly).For.Migrations())
                .Configure<TypeFilterOptions>(
                    opt =>
                    {
                        opt.Namespace = GetType().Namespace;
                        opt.NestedNamespaces = true;
                    })
                .BuildServiceProvider();
        }

        [TearDown]
        public void TearDown()
        {
            _serviceProvider.Dispose();
            File.Delete(_sqliteDbFileName);
        }

        [Test]
        public void IntegerShouldBeCorrectlyConvertedToStringInAllCultures()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
                runner.MigrateUp();
            }
        }
    }
}
