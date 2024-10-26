#region License
//
// Copyright (c) 2007-2024, Fluent Migrator Project
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

using Shouldly;

namespace FluentMigrator.Tests.Unit.Builders.Schema
{
    [TestFixture]
    [Category("Builder")]
    [Category("RootSchema")]
    public class SchemaExpressionRootTest
    {
        private Mock<IQuerySchema> _querySchemaMock;
        private Mock<IMigrationContext> _migrationContextMock;
        private string _testColumn;
        private string _testConstraint;
        private string _testIndex;
        private string _testTable;
        private string _testSchema;
        private string _testSequence;
        private SchemaExpressionRoot _builder;

        [SetUp]
        public void SetUp()
        {
            _migrationContextMock = new Mock<IMigrationContext>();
            _querySchemaMock = new Mock<IQuerySchema>();
            _testSchema = "testSchema";
            _testTable = "testTable";
            _testIndex = "testIndex";
            _testColumn = "testColumn";
            _testConstraint = "testConstraint";
            _testSequence = "testSequence";

            _migrationContextMock.Setup(x => x.QuerySchema).Returns(_querySchemaMock.Object);
            _builder = new SchemaExpressionRoot(_migrationContextMock.Object);
        }

        [Test]
        public void TestTableExists()
        {
            _querySchemaMock.Setup(x => x.TableExists(null, _testTable)).Returns(true);

            _builder.Table(_testTable).Exists().ShouldBeTrue();
            _querySchemaMock.Verify(x => x.TableExists(null, _testTable));
        }

        [Test]
        public void TestConstraintExists()
        {
            _querySchemaMock.Setup(x => x.ConstraintExists(null, _testTable, _testConstraint)).Returns(true);

            _builder.Table(_testTable).Constraint(_testConstraint).Exists().ShouldBeTrue();
            _querySchemaMock.Verify(x => x.ConstraintExists(null, _testTable, _testConstraint));
        }

        [Test]
        public void TestColumnExists()
        {
            _querySchemaMock.Setup(x => x.ColumnExists(null, _testTable, _testColumn)).Returns(true);

            _builder.Table(_testTable).Column(_testColumn).Exists().ShouldBeTrue();
            _querySchemaMock.Verify(x => x.ColumnExists(null, _testTable, _testColumn));
        }

        [Test]
        public void TestIndexExists()
        {
            _querySchemaMock.Setup(x => x.IndexExists(null, _testTable, _testIndex)).Returns(true);


            _builder.Table(_testTable).Index(_testIndex).Exists().ShouldBeTrue();
            _querySchemaMock.Verify(x => x.IndexExists(null, _testTable, _testIndex));
        }

        [Test]
        public void TestSequenceExists()
        {
            _querySchemaMock.Setup(x => x.SequenceExists(null, _testSequence)).Returns(true);

            _builder.Sequence(_testSequence).Exists().ShouldBeTrue();
            _querySchemaMock.Verify(x => x.SequenceExists(null, _testSequence));
        }

        [Test]
        public void TestTableExistsWithSchema()
        {
            _querySchemaMock.Setup(x => x.TableExists(_testSchema, _testTable)).Returns(true);

            _builder.Schema(_testSchema).Table(_testTable).Exists().ShouldBeTrue();
            _querySchemaMock.Verify(x => x.TableExists(_testSchema, _testTable));
        }

        [Test]
        public void TestColumnExistsWithSchema()
        {
            _querySchemaMock.Setup(x => x.ColumnExists(_testSchema, _testTable, _testColumn)).Returns(true);

            _builder.Schema(_testSchema).Table(_testTable).Column(_testColumn).Exists().ShouldBeTrue();
            _querySchemaMock.Verify(x => x.ColumnExists(_testSchema, _testTable, _testColumn));
        }

        [Test]
        public void TestConstraintExistsWithSchema()
        {
            _querySchemaMock.Setup(x => x.ConstraintExists(_testSchema, _testTable, _testConstraint)).Returns(true);

            _builder.Schema(_testSchema).Table(_testTable).Constraint(_testConstraint).Exists().ShouldBeTrue();
            _querySchemaMock.Verify(x => x.ConstraintExists(_testSchema, _testTable, _testConstraint));
        }

        [Test]
        public void TestIndexExistsWithSchema()
        {
            _querySchemaMock.Setup(x => x.IndexExists(_testSchema, _testTable, _testIndex)).Returns(true);

            _builder.Schema(_testSchema).Table(_testTable).Index(_testIndex).Exists().ShouldBeTrue();
            _querySchemaMock.Verify(x => x.IndexExists(_testSchema, _testTable, _testIndex));
        }

        [Test]
        public void TestSequenceExistsWithSchema()
        {
            _querySchemaMock.Setup(x => x.SequenceExists(_testSchema, _testSequence)).Returns(true);

            _builder.Schema(_testSchema).Sequence(_testSequence).Exists().ShouldBeTrue();
            _querySchemaMock.Verify(x => x.SequenceExists(_testSchema, _testSequence));
        }
    }
}
