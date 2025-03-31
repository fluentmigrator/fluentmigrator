#region License
// Copyright (c) 2007-2024, Fluent Migrator Project
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

using System.Data;

using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators.SqlServer;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Generators.SqlServer2012
{
    [TestFixture]
    [Category("Generator")]
    [Category("SqlServer2012")]
    public class SqlServer2012GeneratorTests
    {
        protected SqlServer2012Generator Generator;

        [SetUp]
        public void Setup()
        {
            Generator = new SqlServer2012Generator();
        }

        [Test]
        public void CanUseSystemMethodCurrentDateTimeOffsetAsADefaultValueForAColumn()
        {
            var expression = new CreateColumnExpression
            {
                Column = new ColumnDefinition
                {
                    Name = "NewColumn",
                    Type = DbType.DateTime,
                    DefaultValue = SystemMethods.CurrentDateTimeOffset
                },
                TableName = "NewTable"
            };

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [dbo].[NewTable] ADD [NewColumn] DATETIME NOT NULL CONSTRAINT [DF__NewColumn] DEFAULT SYSDATETIMEOFFSET();");
        }
    }
}
