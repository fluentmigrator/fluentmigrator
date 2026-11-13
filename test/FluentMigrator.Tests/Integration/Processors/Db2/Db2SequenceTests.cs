#region License
//
// Copyright (c) 2026, Fluent Migrator Project
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

#if !NETFRAMEWORK
using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors.DB2;
using FluentMigrator.Tests.Helpers;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Integration.Processors.Db2
{
    [TestFixture]
    [Category("Integration")]
    [Category("Db2")]
    public class Db2SequenceTests : BaseSequenceTests
    {
        private ServiceProvider ServiceProvider { get; set; }
        private IServiceScope ServiceScope { get; set; }
        private Db2Processor Processor { get; set; }

        [Test]
        public override void CallingSequenceExistsReturnsFalseIfSequenceDoesNotExist()
        {
            Processor.SequenceExists(null, "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingSequenceExistsReturnsFalseIfSequenceDoesNotExistWithSchema()
        {
            Processor.SequenceExists("TstSchma", "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingSequenceExistsReturnsTrueIfSequenceExists()
        {
            Processor.Execute("CREATE SEQUENCE TstSeq");
            try
            {
                Processor.SequenceExists(null, "TstSeq").ShouldBeTrue();
            }
            finally
            {
                Processor.Execute("DROP SEQUENCE TstSeq");
            }
        }

        [Test]
        public override void CallingSequenceExistsReturnsTrueIfSequenceExistsWithSchema()
        {
            Processor.Execute("CREATE SCHEMA TstSchma");
            Processor.Execute("CREATE SEQUENCE TstSchma.TstSeq");
            try
            {
                Processor.SequenceExists("TstSchma", "TstSeq").ShouldBeTrue();
                Processor.SequenceExists("OtherSchma", "TstSeq").ShouldBeFalse();
            }
            finally
            {
                Processor.Execute("DROP SEQUENCE TstSchma.TstSeq");
                Processor.Execute("DROP SCHEMA TstSchma RESTRICT");
            }
        }

        [Test]
        public void CallingSequenceExistsReturnsFalseForTheSequenceOfAnIdentityColumn()
        {
            // Db2 names an identity column's sequence SQL<timestamp>; any table's will do.
            Processor.Execute("CREATE TABLE TstIdentity (ID INT NOT NULL GENERATED ALWAYS AS IDENTITY)");
            try
            {
                var name = Processor.Read(
                    "SELECT S.SEQNAME FROM SYSCAT.COLIDENTATTRIBUTES A JOIN SYSCAT.SEQUENCES S ON S.SEQID = A.SEQID"
                  + " WHERE A.TABNAME = 'TSTIDENTITY'").Tables[0].Rows[0][0].ToString();

                Processor.SequenceExists(null, name).ShouldBeFalse();
            }
            finally
            {
                Processor.Execute("DROP TABLE TstIdentity");
            }
        }

        [OneTimeSetUp]
        public void ClassSetUp()
        {
            IntegrationTestOptions.Db2.IgnoreIfNotEnabled();

            var services = ServiceCollectionExtensions.CreateServices()
                .ConfigureRunner(builder => builder.AddDb2())
                .AddScoped<IConnectionStringReader>(
                    _ => new PassThroughConnectionStringReader(IntegrationTestOptions.Db2.ConnectionString));
            ServiceProvider = services.BuildServiceProvider();
        }

        [OneTimeTearDown]
        public void ClassTearDown()
        {
            ServiceProvider?.Dispose();
        }

        [SetUp]
        public void SetUp()
        {
            ServiceScope = ServiceProvider.CreateScope();
            Processor = ServiceScope.ServiceProvider.GetRequiredService<Db2Processor>();
        }

        [TearDown]
        public void TearDown()
        {
            ServiceScope?.Dispose();
            Processor?.Dispose();
        }
    }
}
#endif
