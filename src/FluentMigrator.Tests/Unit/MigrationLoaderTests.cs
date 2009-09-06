using System.Reflection;
using System.Collections.Generic;
using FluentMigrator.Infrastructure;
using FluentMigrator.Runner;
using FluentMigrator.Tests.Integration.Migrations;
using NUnit.Framework;
using NUnit.Should;
using System.Linq;

namespace FluentMigrator.Tests.Unit
{
	[TestFixture]
	public class MigrationLoaderTests
	{
		[Test]
		public void CanFindMigrationsInAssembly()
		{
			var conventions = new MigrationConventions();
			var loader = new MigrationLoader(conventions);
			var asm = Assembly.GetExecutingAssembly();
			IEnumerable<MigrationMetadata> migrationList = loader.FindMigrationsIn(asm);

			//if this works, there will be at least one migration class because i've included on in this code file
			var en = migrationList.GetEnumerator();
			int count = 0;
			while (en.MoveNext())
				count++;

			count.ShouldBeGreaterThan(0);
		}

        [Test]
        public void CanFindMigrationsInNamespace()
        {
            var conventions = new MigrationConventions();
            var loader = new MigrationLoader(conventions);
            var asm = Assembly.GetExecutingAssembly();
            var migrationList = loader.FindMigrationsIn(asm, "FluentMigrator.Tests.Integration.Migrations.Interleaved.Pass1");
            migrationList.Select(x => x.Type).ShouldNotContain(typeof(VersionedMigration));
            migrationList.Count().ShouldBeGreaterThan(0);
        }
	}
}
