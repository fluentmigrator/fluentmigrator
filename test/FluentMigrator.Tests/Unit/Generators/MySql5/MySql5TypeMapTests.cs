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

using FluentMigrator.Runner.Generators.MySql;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Generators.MySql5
{
    [TestFixture]
    [Category("MySql5")]
    [Category("Generator")]
    [Category("TypeMap")]
    public class MySql5TypeMapTests
    {
        private MySql5TypeMap _typeMap;

        [SetUp]
        public void SetUp()
        {
            _typeMap = new MySql5TypeMap();
        }

        [Test]
        public void TimeIsTime()
        {
            _typeMap.GetTypeMap(DbType.Time, size: null, precision: null).ShouldBe("TIME");
        }

        [Test]
        public void DateIsDate()
        {
            _typeMap.GetTypeMap(DbType.Date, size: null, precision: null).ShouldBe("DATE");
        }

        [Test]
        public void DateTimeIsDateTime()
        {
            _typeMap.GetTypeMap(DbType.DateTime, size: null, precision: null).ShouldBe("DATETIME");
        }
    }
}
