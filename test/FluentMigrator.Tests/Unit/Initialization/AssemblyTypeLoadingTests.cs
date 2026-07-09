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

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using FluentMigrator.Runner;
using FluentMigrator.Runner.Infrastructure;
using FluentMigrator.Runner.Initialization;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Initialization
{
    [TestFixture]
    [Category("Initialization")]
    public class AssemblyTypeLoadingTests
    {
        [Test]
        public void AssemblyTypeSourceReturnsLoadableTypesWhenSomeExportedTypesFail()
        {
            var typeSource = new AssemblyTypeSource(
                new TestAssemblySource(new PartiallyLoadableAssembly(typeof(LoadableMigration), null)));

            typeSource.GetTypes().ShouldBe(new[] { typeof(LoadableMigration) });
        }

        [Test]
        public void MigrationSourceItemReturnsLoadableMigrationCandidatesWhenSomeExportedTypesFail()
        {
            var sourceItem = new AssemblyMigrationSourceItem(
                new[] { new PartiallyLoadableAssembly(typeof(LoadableMigration), null) });

            sourceItem.MigrationTypeCandidates.ShouldBe(new[] { typeof(LoadableMigration) });
        }

        [Test]
        public void MigrationRunnerConventionsAccessorFindsConventionsWhenSomeExportedTypesFail()
        {
            var accessor = new AssemblySourceMigrationRunnerConventionsAccessor(
                null,
                new TestAssemblySource(new PartiallyLoadableAssembly(typeof(CustomMigrationRunnerConventions), null)));

            accessor.MigrationRunnerConventions.ShouldBeOfType<CustomMigrationRunnerConventions>();
        }

        [Migration(1)]
        public class LoadableMigration : Migration
        {
            public override void Up()
            {
            }

            public override void Down()
            {
            }
        }

        public class CustomMigrationRunnerConventions : MigrationRunnerConventions
        {
        }

        private class TestAssemblySource : IAssemblySource
        {
            public TestAssemblySource(params Assembly[] assemblies)
            {
                Assemblies = assemblies;
            }

            public IReadOnlyCollection<Assembly> Assemblies { get; }
        }

        private class PartiallyLoadableAssembly : Assembly
        {
            private readonly Type[] _types;

            public PartiallyLoadableAssembly(params Type[] types)
            {
                _types = types;
            }

            public override Type[] GetExportedTypes()
            {
                throw new ReflectionTypeLoadException(
                    _types,
                    new Exception[] { new FileNotFoundException("Could not load dependent assembly.") });
            }
        }
    }
}
