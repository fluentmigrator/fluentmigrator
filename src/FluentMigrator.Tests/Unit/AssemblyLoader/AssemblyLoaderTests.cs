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
			Assert.IsInstanceOfType(typeof(AssemblyLoaderFromFile),assemblyLoaderFromFile);
			Assembly assemblyFromFile = assemblyLoaderFromFile.Load();

			IAssemblyLoader assemblyLoaderFromName = AssemblyLoaderFactory.GetAssemblyLoader(assemblyName);
			Assert.IsInstanceOfType(typeof(AssemblyLoaderFromName), assemblyLoaderFromName);
			Assembly assemblyFromName = assemblyLoaderFromName.Load();

			Assert.AreEqual(assemblyFromFile.FullName,assemblyFromName.FullName );
		}


	}
}
