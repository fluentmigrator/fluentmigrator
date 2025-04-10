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
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators.SQLite;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Generators.SQLite
{
    [TestFixture]
    [Category("Generator")]
    [Category("SQLite")]
    // ReSharper disable once InconsistentNaming
    public class SQLiteGeneratorUsesStrictTablesTests : SQLiteGeneratorTests
    {
        [SetUp]
        public new void Setup()
        {
            // ReSharper disable once RedundantArgumentDefaultValue
            var quoter = new SQLiteQuoter(false);
            var typeMap = new SQLiteTypeMap(true);
            Generator = new SQLiteGenerator(quoter, typeMap);
        }

        [Test]
        public override void CanUseSystemMethodCurrentDateTimeAsADefaultValueForAColumn()
        {
            var expression = new CreateTableExpression { TableName = "TestTable1" };
            expression.Columns.Add(new ColumnDefinition { Name = "DateTimeCol", Type = DbType.DateTime, DefaultValue = SystemMethods.CurrentDateTime});

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE \"TestTable1\" (\"DateTimeCol\" TEXT NOT NULL DEFAULT (datetime('now','localtime')));");
        }

        [Test]
        public override void CanUseSystemMethodCurrentUTCDateTimeAsDefaultValueForColumn()
        {
            var expression = new CreateTableExpression { TableName = "TestTable1" };
            expression.Columns.Add(new ColumnDefinition { Name = "DateTimeCol", Type = DbType.DateTime, DefaultValue = SystemMethods.CurrentUTCDateTime });

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE \"TestTable1\" (\"DateTimeCol\" TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP);");
        }
    }
}
