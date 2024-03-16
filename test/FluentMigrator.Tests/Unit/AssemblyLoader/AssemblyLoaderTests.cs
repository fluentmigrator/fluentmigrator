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

using FluentMigrator.Runner.Initialization.AssemblyLoader;
using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.AssemblyLoader
{
    [TestFixture]
    [Category("AssemblyLoader")]
    public class AssemblyLoaderTests
    {
        private AssemblyLoaderFactory _assemblyLoaderFactory;

        [SetUp]
        public void Setup()
        {
            _assemblyLoaderFactory = new AssemblyLoaderFactory();
        }

        [Test]
        public void ShouldBeAbleToLoadAssemblyByFileName()
        {
            var assemblyLoader = _assemblyLoaderFactory.GetAssemblyLoader(GetType().Assembly.Location);
            Assert.That(assemblyLoader, Is.InstanceOf(typeof(AssemblyLoaderFromFile)));
            Assert.That(assemblyLoader.Load(), Is.EqualTo(GetType().Assembly));
        }

        [Test]
        public void ShouldBeAbleToLoadAssemblyAssemblyName()
        {
            var assemblyLoader = _assemblyLoaderFactory.GetAssemblyLoader(GetType().Assembly.GetName().Name);
            Assert.That(assemblyLoader, Is.InstanceOf(typeof(AssemblyLoaderFromName)));
            Assert.That(assemblyLoader.Load(), Is.EqualTo(GetType().Assembly));
        }
    }
}
