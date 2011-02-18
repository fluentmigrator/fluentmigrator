using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using FluentMigrator.Runner.Generators.SQLite;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.SQLite
{
    public class SQLiteDataTests : BaseDataTests
    {
        protected SqliteGenerator generator;

        [SetUp]
        public void Setup()
        {
            generator = new SqliteGenerator();

        }

        [Test]
        public override void CanInsertData()
        {
            var expression = GeneratorTestHelper.GetInsertDataExpression();
            string sql = generator.Generate(expression);

            string expected = "INSERT INTO [TestTable] (Id,Name,Website) VALUES (1,'Justin','codethinked.com');";
            expected += "INSERT INTO [TestTable] (Id,Name,Website) VALUES (2,'Nate','kohari.org');";

            sql.ShouldBe(expected);
        }

        [Test]
        public override void CanDeleteData()
        {
            throw new NotImplementedException();
        }

        [Test]
        public override void CanInsertGuidData()
        {
            throw new NotImplementedException();
        }

        [Test]
        public override void CanUpdateData()
        {
            throw new NotImplementedException();
        }
    }
}
