#region License
// Copyright (c) 2018, Fluent Migrator Project
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

#if NETFRAMEWORK
using System;
using System.Data.OleDb;
using System.IO;

using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors.Jet;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

namespace FluentMigrator.Tests.Integration.Processors.Jet
{
    [Category("Integration")]
    [Category("Jet")]
    public abstract class JetIntegrationTests
    {
        private string _tempDataDirectory;

        private string DatabaseFilename { get; set; }
        private ServiceProvider ServiceProvider { get; set; }
        private IServiceScope ServiceScope { get; set; }
        protected JetProcessor Processor { get; private set; }

        [OneTimeSetUp]
        public void ClassSetUp()
        {
            IntegrationTestOptions.Jet.IgnoreIfNotEnabled();

            if (Environment.Is64BitOperatingSystem)
                Assert.Inconclusive("Microsoft OLE DB Provider for Jet and Jet ODBC driver are available in 32-bit versions only https://learn.microsoft.com/en-us/office/troubleshoot/access/jet-odbc-driver-available-32-bit-version");

            var serivces = ServiceCollectionExtensions.CreateServices()
                .ConfigureRunner(builder => builder.AddJet())
                .AddScoped<IConnectionStringReader>(
                    _ => new PassThroughConnectionStringReader(IntegrationTestOptions.Jet.ConnectionString));
            ServiceProvider = serivces.BuildServiceProvider();
        }

        [OneTimeTearDown]
        public void ClassTearDown()
        {
            ServiceProvider?.Dispose();
        }

        [SetUp]
        public void SetUp()
        {
            if (!HostUtilities.TryGetJetCatalogType(out var jetCatalogType))
                Assert.Ignore("ADOX.Catalog could not be found - running from .NET Core?");

            _tempDataDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDataDirectory);
            AppDomain.CurrentDomain.SetData("DataDirectory", _tempDataDirectory);

            var csb = new OleDbConnectionStringBuilder(IntegrationTestOptions.Jet.ConnectionString);
            csb.DataSource = DatabaseFilename = HostUtilities.ReplaceDataDirectory(csb.DataSource);

            try
            {
                RecreateDatabase(jetCatalogType, csb.ConnectionString);
            }
            catch (Exception ex)
            {
                try
                {
                    Directory.Delete(_tempDataDirectory);
                }
                catch
                {
                    // Ignore errors
                }

                TestContext.Error.WriteLine(ex.ToString());
                Assert.Ignore(ex.Message);
            }

            ServiceScope = ServiceProvider.CreateScope();
            Processor = ServiceScope.ServiceProvider.GetRequiredService<JetProcessor>();
        }

        [TearDown]
        public void TearDown()
        {
            ServiceScope?.Dispose();
            Processor?.Dispose();

            if (!string.IsNullOrEmpty(_tempDataDirectory) && Directory.Exists(_tempDataDirectory))
            {
                try
                {
                    Directory.Delete(_tempDataDirectory, true);
                }
                catch
                {
                    // Ignore exceptions - we need to find out later why this happens
                }
            }
        }

        private void RecreateDatabase(Type jetCatalogType, string connString)
        {
            if (File.Exists(DatabaseFilename))
            {
                File.Delete(DatabaseFilename);
            }

            if (jetCatalogType != null)
            {
                dynamic cat = Activator.CreateInstance(jetCatalogType);
                cat.Create(connString);
            }
        }
    }
}
#endif