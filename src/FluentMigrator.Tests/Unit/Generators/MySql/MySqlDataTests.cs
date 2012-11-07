using System;
using System.Collections.Generic;
using System.Text;
using FluentMigrator.Expressions;
using NUnit.Framework;
using FluentMigrator.Runner.Generators.MySql;
using NUnit.Should;
using FluentMigrator.Model;

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

            var expected = "INSERT INTO `TestTable1` (`Id`, `Name`, `Website`) VALUES (1, 'Just''in', 'codethinked.com');";
            expected += "\r\nINSERT INTO `TestTable1` (`Id`, `Name`, `Website`) VALUES (2, 'Na\\\\te', 'kohari.org');";

            sql.ShouldBe(expected);
        }

        [Test]
        public void CanInsertBinaryData()
        {
            var expression = new InsertDataExpression
            {
                TableName = "TestTable1"
            };

            var row1 = new { Id = 1, Name = "Just'in", Value = Encoding.ASCII.GetBytes("Just'in") };
            var row2 = new { Id = 1, Name = "Na\te", Value = Encoding.ASCII.GetBytes("Na\te") };

            expression.Rows.AddRange(new IDataDefinition[]
                {
                    new ReflectedDataDefinition(row1),
                    new ReflectedDataDefinition(row2)
                }
            );

            var sql = generator.Generate(expression);
            
            var expected = "INSERT INTO `TestTable1` (`Id`, `Name`, `Value`) VALUES (1, 'Just''in', UNHEX('4A75737427696E'));";
            expected += "\r\nINSERT INTO `TestTable1` (`Id`, `Name`, `Value`) VALUES (1, 'Na	e', UNHEX('4E610965'));";

            sql.ShouldBe(expected);
        }

        [Test]
        public void CanUpdateBinaryData()
        {
            var expression = new UpdateDataExpression
            {
                TableName = "TestTable1"
            };

            var set = new { Name = "Just'in", Value = Encoding.ASCII.GetBytes("Just'in") };
            var where = new { Id = 1 };

            expression.Set.Add(new ReflectedDataDefinition(set));
            expression.Where.Add(new ReflectedDataDefinition(where));

            var sql = generator.Generate(expression);

            string expected = @"UPDATE `TestTable1` SET `Name` = 'Just''in', `Value` = UNHEX('4A75737427696E') WHERE `Id` = 1;";

            sql.ShouldBe(expected);
        }

        [Test]
        public override void CanDeleteData()
        {
            var expression = GeneratorTestHelper.GetDeleteDataExpression();

            var sql = generator.Generate(expression);

            sql.ShouldBe("DELETE FROM `TestTable1` WHERE `Name` = 'Just''in' AND `Website` IS NULL;");
        }

        [Test]
        public override void CanDeleteDataAllRows()
        {
            var expression = GeneratorTestHelper.GetDeleteDataAllRowsExpression();

            var sql = generator.Generate(expression);

            sql.ShouldBe("DELETE FROM `TestTable1` WHERE 1 = 1;");
        }

        [Test]
        public override void CanDeleteDataMultipleRows()
        {
            var expression = GeneratorTestHelper.GetDeleteDataMultipleRowsExpression();

            var sql = generator.Generate(expression);

            sql.ShouldBe("DELETE FROM `TestTable1` WHERE `Name` = 'Just''in' AND `Website` IS NULL;\r\nDELETE FROM `TestTable1` WHERE `Website` = 'github.com';");
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
            var expression = GeneratorTestHelper.GetUpdateDataExpression();

            var sql = generator.Generate(expression);
            sql.ShouldBe("UPDATE `TestTable1` SET `Name` = 'Just''in', `Age` = 25 WHERE `Id` = 9 AND `Homepage` IS NULL;");
        }

        [Test]
        public void CanUpdateDataForAllRows()
        {
            var expression = GeneratorTestHelper.GetUpdateDataExpressionWithAllRows();

            var sql = generator.Generate(expression);
            sql.ShouldBe("UPDATE `TestTable1` SET `Name` = 'Just''in', `Age` = 25 WHERE 1 = 1;");
        }
    }
}
