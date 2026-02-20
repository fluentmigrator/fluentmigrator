#region License
// Copyright (c) 2019, Fluent Migrator Project
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

using System;
using System.Data.Common;


using FluentMigrator.Runner;
using FluentMigrator.Runner.Processors.Postgres;

using Microsoft.Extensions.DependencyInjection;

using Moq;

using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.Processors.Postgres
{
    [Category("DependencyInjection")]
    [Category("Postgres")]
    [TestFixture]
    public class PostgresDependencyInjectionTests
    {
        [Test]
        public void ProcessorUsesCustomDbProviderFactoryForExecute() {
            ProcessorUsesCustomDbProviderFactoryFor(p => p.Execute("Any SQL statement"));
        }

        [Test]
        public void VerifyBeginTransactionCreatesConnectionForeginTransaction() {
            ProcessorUsesCustomDbProviderFactoryFor(p => p.BeginTransaction());
        }

        private void ProcessorUsesCustomDbProviderFactoryFor(Action<PostgresProcessor> action)
        {
            var dbProviderMock = new Mock<DbProviderFactory>();
            var commandMock = new Mock<DbCommand>();
            var connectionMock = new Mock<DbConnection>();
            dbProviderMock.Setup(db => db.CreateCommand()).Returns(commandMock.Object);
            dbProviderMock.Setup(db => db.CreateConnection()).Returns(connectionMock.Object);
            var services = new ServiceCollection()
                .AddFluentMigratorCore()
                .ConfigureRunner(rb => rb.AddPostgres(dbProviderMock.Object))
                .BuildServiceProvider(true);

            using (var scope = services.CreateScope())
            {
                var t = scope.ServiceProvider.GetRequiredService<PostgresProcessor>();
                action(t);
                dbProviderMock.Verify(db => db.CreateConnection());
            }
        }
    }
}
