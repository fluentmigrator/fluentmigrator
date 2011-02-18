using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using FluentMigrator.Runner.Generators.MySql;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.MySql
{
    


    public class MySqlDataTests : BaseDataTests
    {
        protected MySqlGenerator generator;
        
        [SetUp]
        public void Setup()
        {
            generator = new MySqlGenerator();
        }

        [Test]
        public override void CanInsertData()
        {
            var expression = GeneratorTestHelper.GetInsertDataExpression();
            var sql = generator.Generate(expression);

            var expected = @"INSERT INTO `TestTable1` (`Id`,`Name`,`Website`) VALUES (1,'Just''in','codethinked.com');";
            expected += @"INSERT INTO `TestTable1` (`Id`,`Name`,`Website`) VALUES (2,'Na\\te','kohari.org');";

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
            var expression = GeneratorTestHelper.GetInsertGUIDExpression();

            var sql = generator.Generate(expression);

            var expected = String.Format("INSERT INTO `TestTable1` (`guid`) VALUES ('{0}');", GeneratorTestHelper.TestGuid.ToString());

            sql.ShouldBe(expected);
        }

        [Test]
        public override void CanUpdateData()
        {
            throw new NotImplementedException();
        }
    }
}
