#region License
// 
// Copyright (c) 2007-2009, Sean Chambers <schambers80@gmail.com>
// Copyright (c) 2012, Daniel Lee
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

using FluentMigrator.Expressions;
using FluentMigrator.Runner.Generators.SqlServer;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.SqlServer2012
{
    [TestFixture]
    public class SqlServer2012SequenceTests
    {
    
        private IMigrationGenerator generator;

        [SetUp]
        public void Setup()
        {
            generator = new SqlServer2012Generator();
        }


        [Test]
        public void CanCreateSequence()
        {
            var expression = new CreateSequenceExpression
            {
                Sequence =
                {
                    Cache = 10,
                    Cycle = true,
                    Increment = 2,
                    MaxValue = 100,
                    MinValue = 0,
                    Name = "Sequence",
                    SchemaName = "TestSchema",
                    StartWith = 2
                }
            };
            var sql = generator.Generate(expression);
            sql.ShouldBe("CREATE SEQUENCE [TestSchema].[Sequence] INCREMENT BY 2 MINVALUE 0 MAXVALUE 100 START WITH 2 CACHE 10 CYCLE");
        }

        [Test]
        public void CanDeleteSequence()
        {
            var expression = new DeleteSequenceExpression { SchemaName = "TestSchema", SequenceName = "Sequence" };
            var sql = generator.Generate(expression);
            sql.ShouldBe("DROP SEQUENCE [TestSchema].[Sequence]");
        }
    }
}