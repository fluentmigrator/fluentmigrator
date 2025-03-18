#region License
//
// Copyright (c) 2018, Fluent Migrator Project
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

using System.Data;

using FluentMigrator.Expressions;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors.Firebird;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Integration.Processors.Firebird
{
    [TestFixture]
    [Category("Integration")]
    [Category("Firebird")]
    public class TestDisposing
    {
        private readonly FirebirdLibraryProber _prober = new FirebirdLibraryProber();
        private TemporaryDatabase _temporaryDatabase;

        private ServiceProvider ServiceProvider { get; set; }

        [Test]
        public void Dispose_WasCommitted_ShouldNotRollback()
        {
            var createTable = new CreateTableExpression { TableName = "silly" };
            createTable.Columns.Add(new Model.ColumnDefinition { Name = "one", Type = DbType.Int32 });

            using (var scope = ServiceProvider.CreateScope())
            {
                using (var processor = scope.ServiceProvider.GetRequiredService<FirebirdProcessor>())
                {
                    processor.Process(createTable);

                    // this will close the connection
                    processor.CommitTransaction();

                    // and this will reopen it again causing Dispose->RollbackTransaction not to throw
                    var tableExists = processor.TableExists("", createTable.TableName);
                    tableExists.ShouldBeTrue();
                }
            }

            using (var scope = ServiceProvider.CreateScope())
            {
                using (var processor = scope.ServiceProvider.GetRequiredService<FirebirdProcessor>())
                {
                    // Check that the table still exists after dispose
                    processor.TableExists("", createTable.TableName).ShouldBeTrue();
                }
            }
        }

        [SetUp]
        public void SetUp()
        {
            IntegrationTestOptions.Firebird.IgnoreIfNotEnabled();

            var services = FbDatabase.CreateFirebirdServices(_prober, out _temporaryDatabase);

            ServiceProvider = services.BuildServiceProvider();
        }

        [TearDown]
        public void TearDown()
        {
            ServiceProvider?.Dispose();
            _temporaryDatabase?.Dispose();
        }
    }
}
