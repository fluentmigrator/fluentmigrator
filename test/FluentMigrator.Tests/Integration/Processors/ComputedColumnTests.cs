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

using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators.SqlServer;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Integration.Processors
{
    [TestFixture]
    [Category("Integration")]
    [Category("SqlServer")]
    public class ComputedColumnTests
    {
        private SqlServer2000Generator _generator;

        [SetUp]
        public void Setup()
        {
            _generator = new SqlServer2000Generator();
        }

        [Test]
        public void CanGenerateSqlForComputedColumn()
        {
            // Create a column with a computed expression
            var column = new ColumnDefinition
            {
                Name = "Total",
                Expression = "Price * Quantity",
                ExpressionStored = false
            };

            // Create a table with the computed column
            var createTableExpression = new CreateTableExpression
            {
                TableName = "Products",
                Columns = { column }
            };

            // Generate SQL for the create table expression
            var sql = _generator.Generate(createTableExpression);

            // Verify the SQL contains the computed column expression
            sql.ShouldContain("CREATE TABLE [Products] ([Total] AS (Price * Quantity) NOT NULL)");
        }

        [Test]
        public void CanGenerateSqlForStoredComputedColumn()
        {
            // Create a column with a stored computed expression
            var column = new ColumnDefinition
            {
                Name = "Total",
                Expression = "Price * Quantity",
                ExpressionStored = true
            };

            // Create a table with the computed column
            var createTableExpression = new CreateTableExpression
            {
                TableName = "Products",
                Columns = { column }
            };

            // Generate SQL for the create table expression
            var sql = _generator.Generate(createTableExpression);

            // Verify the SQL contains the stored computed column expression
            sql.ShouldContain("CREATE TABLE [Products] ([Total] AS (Price * Quantity) PERSISTED NOT NULL)");
        }

        [Test]
        public void CanGenerateSqlForAlterColumnWithComputedExpression()
        {
            // Create a column with a computed expression
            var column = new ColumnDefinition
            {
                Name = "Total",
                Expression = "Price * Quantity",
                ExpressionStored = false,
                ModificationType = ColumnModificationType.Alter
            };

            // Create an alter column expression
            var alterColumnExpression = new AlterColumnExpression
            {
                TableName = "Products",
                Column = column
            };

            // Generate SQL for the alter column expression
            var sql = _generator.Generate(alterColumnExpression);

            // Verify the SQL contains the computed column expression
            sql.ShouldContain("ALTER TABLE [Products] ALTER COLUMN [Total] AS (Price * Quantity) NOT NULL");
        }

        [Test]
        public void CanGenerateSqlForAlterColumnWithStoredComputedExpression()
        {
            // Create a column with a stored computed expression
            var column = new ColumnDefinition
            {
                Name = "Total",
                Expression = "Price * Quantity",
                ExpressionStored = true,
                ModificationType = ColumnModificationType.Alter
            };

            // Create an alter column expression
            var alterColumnExpression = new AlterColumnExpression
            {
                TableName = "Products",
                Column = column
            };

            // Generate SQL for the alter column expression
            var sql = _generator.Generate(alterColumnExpression);

            // Verify the SQL contains the stored computed column expression
            sql.ShouldContain("ALTER TABLE [Products] ALTER COLUMN [Total] AS (Price * Quantity) PERSISTED NOT NULL");
        }
    }
}
