﻿using System;
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

            var expected = @"INSERT INTO `TestTable1` (`Id`, `Name`, `Website`) VALUES (1, 'Just''in', 'codethinked.com');";
            expected += @" INSERT INTO `TestTable1` (`Id`, `Name`, `Website`) VALUES (2, 'Na\\te', 'kohari.org')";

            sql.ShouldBe(expected);
        }

        [Test]
        public override void CanDeleteData()
        {
            var expression = GeneratorTestHelper.GetDeleteDataExpression();

            var sql = generator.Generate(expression);

            sql.ShouldBe("DELETE FROM `TestTable1` WHERE `Name` = 'Just''in' AND `Website` IS NULL");
        }

        [Test]
        public override void CanDeleteDataAllRows()
        {
            var expression = GeneratorTestHelper.GetDeleteDataAllRowsExpression();

            var sql = generator.Generate(expression);

            sql.ShouldBe("DELETE FROM `TestTable1` WHERE 1 = 1");
        }

        [Test]
        public override void CanDeleteDataMultipleRows()
        {
            var expression = GeneratorTestHelper.GetDeleteDataMultipleRowsExpression();

            var sql = generator.Generate(expression);

            sql.ShouldBe("DELETE FROM `TestTable1` WHERE `Name` = 'Just''in' AND `Website` IS NULL; DELETE FROM `TestTable1` WHERE `Website` = 'github.com'");
        }

        [Test]
        public override void CanInsertGuidData()
        {
            var expression = GeneratorTestHelper.GetInsertGUIDExpression();

            var sql = generator.Generate(expression);

            var expected = String.Format("INSERT INTO `TestTable1` (`guid`) VALUES ('{0}')", GeneratorTestHelper.TestGuid.ToString());

            sql.ShouldBe(expected);
        }

        [Test]
        public override void CanUpdateData()
        {
            var expression = GeneratorTestHelper.GetUpdateDataExpression();

            var sql = generator.Generate(expression);
            sql.ShouldBe("UPDATE `TestTable1` SET `Name` = 'Just''in', `Age` = 25 WHERE `Id` = 9 AND `Homepage` IS NULL");
        }
    }
}
