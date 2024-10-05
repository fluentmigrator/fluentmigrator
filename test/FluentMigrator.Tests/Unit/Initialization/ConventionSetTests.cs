#region License
// Copyright (c) 2019, Fluent Migrator Project
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

using System.Linq;

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
            Assert.That(expr.SchemaName, Is.EqualTo("custom-schema"));
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
            Assert.That(expr.SchemaName, Is.EqualTo("custom-schema"));
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
            Assert.That(expr.SchemaName, Is.EqualTo(null));
        }

        [Test]
        public void RegisterOnlyOneConventionSetIfRegisteredBeforeAddCall()
        {
            const string schemaName = "RegTestSchemaA";

            var services = new ServiceCollection();

            services.AddScoped<IConventionSet>(provider =>
                new DefaultConventionSet(
                    defaultSchemaName: schemaName,
                    workingDirectory: null
                ));

            // The AddFluentMigratorCore should not register a 2nd IConventionSet

            services
                .AddFluentMigratorCore()
                .ConfigureRunner(rb => rb
                    .AddSQLite()
                    .ScanIn(this.GetType().Assembly).For.Migrations()
                );

            var serviceProvider = services.BuildServiceProvider(false);

            var conventionSets = serviceProvider.GetServices<IConventionSet>();

            Assert.That(conventionSets.Count(), Is.EqualTo(1));
        }

        [Test]
        public void GetCorrectSchemaFromCustomConventionSetIfRegisteredBeforeAddCall()
        {
            const string schemaName = "RegTestSchemaA";

            var services = new ServiceCollection();

            // register our IConventionSet before the call to AddFluentMigratorCore()
            services.AddScoped<IConventionSet>(provider =>
                new DefaultConventionSet(
                    defaultSchemaName: schemaName,
                    workingDirectory: null
                ));

            services
                .AddFluentMigratorCore()
                .ConfigureRunner(rb => rb
                    .AddSQLite()
                    .ScanIn(this.GetType().Assembly).For.Migrations()
                );

            var serviceProvider = services.BuildServiceProvider(false);

            var conventionSet = serviceProvider.GetRequiredService<IConventionSet>();
            ISchemaExpression expr = new CreateTableExpression();
            conventionSet.SchemaConvention.Apply(expr);

            Assert.That(expr.SchemaName, Is.EqualTo(schemaName));
        }

        [Test]
        public void GetCorrectSchemaFromCustomConventionSetIfRegisteredAfterAddCall()
        {
            const string schemaName = "RegTestSchemaA";

            var services = new ServiceCollection();

            services
                .AddFluentMigratorCore()
                .ConfigureRunner(rb => rb
                    .AddSQLite()
                    .ScanIn(this.GetType().Assembly).For.Migrations()
                );

            // register our IConventionSet after the call to AddFluentMigratorCore()
            services.AddScoped<IConventionSet>(provider =>
                new DefaultConventionSet(
                    defaultSchemaName: schemaName,
                    workingDirectory: null
                ));

            var serviceProvider = services.BuildServiceProvider(false);

            var conventionSet = serviceProvider.GetRequiredService<IConventionSet>();
            ISchemaExpression expr = new CreateTableExpression();
            conventionSet.SchemaConvention.Apply(expr);

            Assert.That(expr.SchemaName, Is.EqualTo(schemaName));
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
