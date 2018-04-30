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
using FluentMigrator.Runner.Generators.SqlServer;
using NUnit.Framework;
using Shouldly;

namespace FluentMigrator.Tests.Unit.Generators.SqlServer2008
{
    [TestFixture]
    [Category("SqlServer2008")]
    [Category("Generator")]
    [Category("TypeMap")]
    public abstract class SqlServer2008TypeMapTests
    {
        private SqlServer2005TypeMap TypeMap { get; set; }

        [SetUp]
        public void Setup()
        {
            TypeMap = new SqlServer2008TypeMap();
        }

        [TestFixture]
        public class DateTimeTests : SqlServer2008TypeMapTests
        {
            [Test]
            public void ItMapsTimeToDatetime()
            {
                var template = TypeMap.GetTypeMap(DbType.Time, size: null, precision: null);

                template.ShouldBe("TIME");
            }

            [Test]
            public void ItMapsDateToDatetime()
            {
                var template = TypeMap.GetTypeMap(DbType.Date, size: null, precision: null);

                template.ShouldBe("DATE");
            }

            [Test]
            public void ItMapsDatetimeToDatetime()
            {
                var template = TypeMap.GetTypeMap(DbType.DateTime2, size: null, precision: null);

                template.ShouldBe("DATETIME2");
            }

            [Test]
            public void ItMapsDatetimeToDatetimeOffset()
            {
                var template = TypeMap.GetTypeMap(DbType.DateTimeOffset, size: null, precision: null);

                template.ShouldBe("DATETIMEOFFSET");
            }

            [Test]
            [TestCase(0)]
            [TestCase(7)]
            public void ItMapsDatetimeToDatetimeOffset(int precision)
            {
                var template = TypeMap.GetTypeMap(DbType.DateTimeOffset, precision, precision: null);

                template.ShouldBe($"DATETIMEOFFSET({precision})");
            }
        }
    }
}
