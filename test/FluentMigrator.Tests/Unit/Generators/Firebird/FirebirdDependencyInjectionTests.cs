#region License
// Copyright (c) 2026, Fluent Migrator Project
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
using FluentMigrator.Runner;
using FluentMigrator.Runner.Generators.Firebird;
using FluentMigrator.Runner.Processors.Firebird;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.Generators.Firebird
{
    [Category("DependencyInjection")]
    [Category("Firebird")]
    [TestFixture]
    public class FirebirdDependencyInjectionTests
    {
        [Test]
        public void CustomQuoterIsUsedForColumnNames()
        {
            var services = new ServiceCollection()
                .AddFluentMigratorCore()
                .ConfigureRunner(rb => rb.AddFirebird(FirebirdOptions.StandardBehaviour()))
                .AddScoped<FirebirdQuoter, LowercaseFirebirdQuoter>()
                .BuildServiceProvider(true);

            using (var scope = services.CreateScope())
            {
                var generator = scope.ServiceProvider.GetRequiredService<IMigrationGenerator>();
                var expression = new CreateTableExpression { TableName = "MyTable" };
                expression.Columns.Add(new ColumnDefinition { Name = "MyColumn", Type = DbType.Int32, TableName = "MyTable" });

                var statement = generator.Generate(expression);

                Assert.That(statement, Is.EqualTo("CREATE TABLE mytable (mycolumn INTEGER NOT NULL);"));
            }
        }

        /// <summary>
        /// Quoter that lowercases unquoted names instead of quoting them.
        /// </summary>
        private class LowercaseFirebirdQuoter : FirebirdQuoter
        {
            public LowercaseFirebirdQuoter(FirebirdOptions options)
                : base(options)
            {
            }

            /// <inheritdoc />
            public override string Quote(string name) => IsQuoted(name) ? name : name?.ToLowerInvariant();
        }
    }
}
