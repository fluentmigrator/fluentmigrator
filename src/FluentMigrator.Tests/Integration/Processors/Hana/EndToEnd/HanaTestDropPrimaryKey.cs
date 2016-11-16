using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Generators.Hana;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.Hana;
using FluentMigrator.Tests.Integration.Processors.Firebird.EndToEnd;
using NUnit.Framework;
using Sap.Data.Hana;
using Shouldly;

namespace FluentMigrator.Tests.Integration.Processors.Hana.EndToEnd
{
    [TestFixture]
    [Category("Integration")]
    [Category("Hana")]
    public class TestRollbackColumnCreation : HanaEndToEndFixture
    {
        public HanaConnection Connection { get; set; }
        public HanaProcessor Processor { get; set; }

        [SetUp]
        public void SetUp()
        {
            Connection = new HanaConnection(IntegrationTestOptions.Hana.ConnectionString);
            Processor = new HanaProcessor(Connection, new HanaGenerator(), new TextWriterAnnouncer(System.Console.Out), new ProcessorOptions(), new HanaDbFactory());
            Connection.Open();
            Processor.BeginTransaction();
        }

        [TearDown]
        public void TearDown()
        {
            Processor.CommitTransaction();
            Processor.Dispose();
        }

        [Test]
        public void Delete_ColumnCreateOnTableWithExplicitPk_ColumnShouldBeDropped()
        {
            DeleteTableIfExists("Teste", "Teste1");
                        
            Migrate(typeof(ImplicitlyCreatedFkForHana.CreateImplicitFk).Namespace);
        }

        private void DeleteTableIfExists(params string[] tableNames)
        {
            foreach (var tableName in tableNames)
            {
                if (Processor.TableExists(null, tableName))
                    Processor.Execute($"DROP TABLE \"{tableName}\"");    
            }

        }
    }

    namespace ImplicitlyCreatedFkForHana
    {
        [Migration(1)]
        public class CreateImplicitFk : Migration
        {
            public override void Up()
            {

                Create.Table("Teste1")
                    .WithColumn("Id").AsInt32().PrimaryKey("PK_TST").Identity()
                    .WithColumn("Nome").AsString(100);

                Create.Table("Teste")
                    .WithColumn("Id").AsInt32().PrimaryKey()
                        .ForeignKey("Teste1","Id")
                    .WithColumn("Nome").AsString();

                Delete.PrimaryKey("").FromTable("Teste");
            }

            public override void Down()
            {
                Delete.Table("Teste");
                Delete.Table("Teste1");
            }
        }
    }
}
