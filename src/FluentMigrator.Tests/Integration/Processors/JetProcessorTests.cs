#region License
// Copyright (c) 2007-2009, Sean Chambers <schambers80@gmail.com>
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

using System.Data.OleDb;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Generators.Jet;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.Jet;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Integration.Processors
{
    [TestFixture]
    [Category("Integration")]
    public class JetProcessorTests
    {
        public OleDbConnection Connection { get; set; }
        public JetProcessor Processor { get; set; }
        [SetUp]
        public void SetUp()
        {
            Connection = new OleDbConnection(IntegrationTestOptions.Jet.ConnectionString);
            Processor = new JetProcessor(Connection, new JetGenerator(), new TextWriterAnnouncer(System.Console.Out), new ProcessorOptions());
            Connection.Open();
        }

        [Test]
        public void CallingTableExistsReturnsFalseIfTableDoesNotExist()
        {
            Processor.TableExists(null, "DoesNotExist").ShouldBeFalse();
        }

        [TearDown]
        public void TearDown()
        {
            Processor.CommitTransaction();
            Processor.Dispose();
        }
    }
}