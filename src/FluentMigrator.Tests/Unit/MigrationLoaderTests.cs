using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using FluentMigrator.Infrastructure;
using FluentMigrator.Runner;
using Xunit;

namespace FluentMigrator.Tests.Unit
{
    public class MigrationLoaderTests
    {
        [Fact]
        public void CanFindMigrationsInAssembly()
        {
            var conventions = new MigrationConventions();
            var loader = new MigrationLoader(conventions);
            var asm = Assembly.GetExecutingAssembly();
            IEnumerable<MigrationMetadata> migrationList = loader.FindMigrationsIn(asm);
            
            //if this works, there will be at least one migration class because i've included on in this code file
            var en = migrationList.GetEnumerator();
            int count = 0;
            while (en.MoveNext()) count++;
            Assert.True(count>0);
        }
    }

    [Migration(2)]
    public class VersionedMigration : Migration
    {
        public override void Up()
        {
            Create.Table("VersionedMigration")
                .WithColumn("Id").AsInt32().Identity().NotNullable().PrimaryKey()
                .WithColumn("PooBah").AsString(32).Nullable();
        }

        public override void Down()
        {
            Delete.Table("VersionedMigration");
        }
    }
}
