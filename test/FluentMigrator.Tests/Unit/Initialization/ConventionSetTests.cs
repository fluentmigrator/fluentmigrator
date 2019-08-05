#region License
// Copyright (c) 2019, FluentMigrator Project
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

using FluentMigrator.Expressions;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Conventions;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.Initialization
{
    public class ConventionSetTests
    {
        [Test]
        public void CanLoadConventionSetFromAssembly()
        {
            var services = new ServiceCollection()
                .AddFluentMigratorCore()
                .ConfigureRunner(rb => rb.AddSQLite().ScanIn(this.GetType().Assembly).For.ConventionSet())
                .BuildServiceProvider(false);
            var conventionSet = services.GetRequiredService<IConventionSet>();
            ISchemaExpression expr = new CreateTableExpression();
            conventionSet.SchemaConvention.Apply(expr);
            Assert.AreEqual("custom-schema", expr.SchemaName);
        }

        [Test]
        public void CanLoadConventionSetFromAssemblyWithScanForAll()
        {
            var services = new ServiceCollection()
                .AddFluentMigratorCore()
                .ConfigureRunner(rb => rb.AddSQLite().ScanIn(this.GetType().Assembly).For.All())
                .BuildServiceProvider(false);
            var conventionSet = services.GetRequiredService<IConventionSet>();
            ISchemaExpression expr = new CreateTableExpression();
            conventionSet.SchemaConvention.Apply(expr);
            Assert.AreEqual("custom-schema", expr.SchemaName);
        }

        [Test]
        public void NotLoadingConventionSetFromAssemblyWithScanForMigrations()
        {
            var services = new ServiceCollection()
                .AddFluentMigratorCore()
                .ConfigureRunner(rb => rb.AddSQLite().ScanIn(this.GetType().Assembly).For.Migrations())
                .BuildServiceProvider(false);
            var conventionSet = services.GetRequiredService<IConventionSet>();
            ISchemaExpression expr = new CreateTableExpression();
            conventionSet.SchemaConvention.Apply(expr);
            Assert.AreEqual(null, expr.SchemaName);
        }

        // ReSharper disable once UnusedMember.Global
        public class CustomConventionSet : ConventionSet
        {
            public CustomConventionSet()
            {
                base.SchemaConvention = new DefaultSchemaConvention("custom-schema");
            }
        }
    }
}
