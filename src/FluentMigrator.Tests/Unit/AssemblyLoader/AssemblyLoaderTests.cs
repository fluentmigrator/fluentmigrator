#region License
// 
// Copyright (c) 2007-2009, Sean Chambers <schambers80@gmail.com>
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
using System.Collections;
using System.Reflection;
using FluentMigrator.Runner.Initialization.AssemblyLoader;
using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.AssemblyLoader
{
	[TestFixture]
	public class AssemblyLoaderTests
	{

		[Test]
		public void TestLoaders()
		{
			string filename = GetType().Assembly.Location;
			string assemblyName = GetType().Assembly.GetName().Name;
			IAssemblyLoader assemblyLoaderFromFile = AssemblyLoaderFactory.GetAssemblyLoader(filename);
            Assert.IsInstanceOf(typeof(AssemblyLoaderFromFile), assemblyLoaderFromFile);
			Assembly assemblyFromFile = assemblyLoaderFromFile.Load();

			IAssemblyLoader assemblyLoaderFromName = AssemblyLoaderFactory.GetAssemblyLoader(assemblyName);
            Assert.IsInstanceOf(typeof(AssemblyLoaderFromName), assemblyLoaderFromName);
			Assembly assemblyFromName = assemblyLoaderFromName.Load();

			Assert.AreEqual(assemblyFromFile.FullName,assemblyFromName.FullName );
		}


	}
}
