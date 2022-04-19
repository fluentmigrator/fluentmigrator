#region License
// Copyright (c) 2018, FluentMigrator Project
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

using System.IO;

using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

namespace FluentMigrator.Tests.IssueTests.GH0904
{
    [TestFixture]
    [Category("Issue")]
    [Category("GH-0904")]
    [Category("SQLite")]
    public class Fixture
    {
        private string _sqliteDbFileName;
        private ServiceProvider _serviceProvider;

        [SetUp]
        public void SetUp()
        {
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
        public void ProfileMustNotCauseNullReferenceException()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
                runner.MigrateUp();
            }
        }
    }
}
