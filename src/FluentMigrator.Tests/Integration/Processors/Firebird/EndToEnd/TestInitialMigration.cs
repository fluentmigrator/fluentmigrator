using FluentMigrator.Tests.Integration.Processors.Firebird.EndToEnd.SimpleMigration;
using NUnit.Framework;

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
    public class TestInitialMigration : FbEndToEndFixture
    {
        public TestInitialMigration()
            :base(typeof(VersionOneSimpleTableMigration).Namespace)
        {            
        }

        [TestCase("SIMPLE")]
        [TestCase("VersionInfo")]
        public void Migrate_FirstVersion_ShouldCreateTable(string tableName)
        {
            Migrate();

            Assert.That(TableExists(tableName), Is.True, "Table {0} should have been created but it wasn't", tableName);
        }

        [TestCase("ID")]
        [TestCase("COL_STR")]
        public void Migrate_FirstVersion_ShouldCreateColumn(string columnName)
        {
            Migrate();

            Assert.That(ColumnExists("SIMPLE", columnName), Is.True, "Column {0} should have been created but it wasn't", columnName);
        }

        [TestCase("SIMPLE")]
        [TestCase("VersionInfo")]
        public void Rollback_FirstVersion_ShouldDropTable(string table)
        {
            Migrate();

            Rollback();

            Assert.That(TableExists(table), Is.False, "Table {0} should have been dropped but it wasn't", table);
        }
    }
}
