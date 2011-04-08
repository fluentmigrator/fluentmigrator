#region License
// 
// Copyright (c) 2007-2009, Sean Chambers <schambers80@gmail.com>
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

using FluentMigrator.Builders.Schema;
using FluentMigrator.Infrastructure;
using Moq;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Builders.Schema
{
	[TestFixture]
	public class SchemaExpressionRootTest
	{
		private Mock<IQuerySchema> _querySchemaMock;
		private Mock<IMigrationContext> _migrationContextMock;
		private string _testColumn;
		private string _testTable;
		private string _testSchema;
		private SchemaExpressionRoot _builder;

		[SetUp]
		public void SetUp()
		{
			_migrationContextMock = new Mock<IMigrationContext>();
			_querySchemaMock = new Mock<IQuerySchema>();
		    _testSchema = "testSchema";
			_testTable = "testTable";
			_testColumn = "testColumn";

			_migrationContextMock.Setup(x => x.QuerySchema).Returns(_querySchemaMock.Object).AtMostOnce();
			_builder = new SchemaExpressionRoot(_migrationContextMock.Object);
		}

		[Test]
		public void TestTableExists()
		{
			_querySchemaMock.Setup(x => x.TableExists(null, _testTable)).Returns(true).AtMostOnce();

			_builder.Table(_testTable).Exists().ShouldBeTrue();
			_migrationContextMock.VerifyAll();
		}

		[Test]
		public void TestColumnExists()
		{
			_querySchemaMock.Setup(x => x.ColumnExists(null, _testTable, _testColumn)).Returns(true).AtMostOnce();

			_builder.Table(_testTable).Column(_testColumn).Exists().ShouldBeTrue();
			_migrationContextMock.VerifyAll();
		}

        [Test]
        public void TestTableExistsWithSchema()
        {
            _querySchemaMock.Setup(x => x.TableExists(_testSchema, _testTable)).Returns(true).AtMostOnce();

            _builder.Schema(_testSchema).Table(_testTable).Exists().ShouldBeTrue();
            _migrationContextMock.VerifyAll();
        }

        [Test]
        public void TestColumnExistsWithSchema()
        {
            _querySchemaMock.Setup(x => x.ColumnExists(_testSchema, _testTable, _testColumn)).Returns(true).AtMostOnce();

            _builder.Schema(_testSchema).Table(_testTable).Column(_testColumn).Exists().ShouldBeTrue();
            _migrationContextMock.VerifyAll();
        }
	}
}
