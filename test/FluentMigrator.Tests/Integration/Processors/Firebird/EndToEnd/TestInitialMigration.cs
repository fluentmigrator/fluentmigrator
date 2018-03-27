using FluentMigrator.Tests.Integration.Processors.Firebird.EndToEnd.SimpleMigration;
using NUnit.Framework;
using Shouldly;

namespace FluentMigrator.Tests.Integration.Processors.Firebird.EndToEnd
{
    namespace SimpleMigration
    {
        [Migration(1)]
        public class VersionOneSimpleTableMigration : Migration
        {
            public override void Up()
            {
                Create.Table("SIMPLE")
                    .WithColumn("ID").AsInt32().PrimaryKey()
                    .WithColumn("COL_STR").AsString(10);
            }

            public override void Down()
            {
                Delete.Table("SIMPLE");
            }
        }
    }

    [TestFixture]
    [Category("Integration")]
    [Category("Firebird")]
    public class TestInitialMigration : FbEndToEndFixture
    {
        [TestCase("SIMPLE")]
        [TestCase("VersionInfo")]
        public void Migrate_FirstVersion_ShouldCreateTable(string tableName)
        {
            Migrate(typeof(VersionOneSimpleTableMigration).Namespace);

            TableExists(tableName).ShouldBe(true, string.Format("Table {0} should have been created but it wasn't", tableName));
        }

        [TestCase("ID")]
        [TestCase("COL_STR")]
        public void Migrate_FirstVersion_ShouldCreateColumn(string columnName)
        {
            Migrate(typeof(VersionOneSimpleTableMigration).Namespace);

            ColumnExists("SIMPLE", columnName).ShouldBe(true, string.Format("Column {0} should have been created but it wasn't", columnName));
        }

        [TestCase("SIMPLE")]
        [TestCase("VersionInfo")]
        public void Rollback_FirstVersion_ShouldDropTable(string table)
        {
            var migrationsNamespace = typeof(VersionOneSimpleTableMigration).Namespace;
            Migrate(migrationsNamespace);

            Rollback(migrationsNamespace);

            TableExists(table).ShouldBe(false, string.Format("Table {0} should have been dropped but it wasn't", table));
        }
    }
}
