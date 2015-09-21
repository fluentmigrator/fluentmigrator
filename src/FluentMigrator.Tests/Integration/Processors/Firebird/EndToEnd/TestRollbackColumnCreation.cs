using FluentMigrator.Tests.Integration.Processors.Firebird.EndToEnd.ImplicitlyCreatedFk;
using NUnit.Framework;
using Shouldly;

namespace FluentMigrator.Tests.Integration.Processors.Firebird.EndToEnd
{
    namespace ImplicitlyCreatedFk
    {
        [Migration(1)]
        public class CreateImplicitFk : Migration
        {
            public override void Up()
            {
                Execute.Sql("create table table1(id bigint primary key)");

                // the foreign key "table1_fk" doesn't explictly reference "id" of table1!
                Execute.Sql("create table table2(id bigint primary key, table1_fk bigint references table1)"); 
            }

            public override void Down()
            {
                Delete.Table("table2");
                Delete.Table("table1");
            }
        }

        [Migration(2)]
        public class CreateSillyColumnOnTable2 : Migration
        {
            public override void Up()
            {
                Create.Column("silly").OnTable("table2").AsString(30);
            }

            public override void Down()
            {
                Delete.Column("silly").FromTable("table2");
            }
        }
    }

    public class TestRollbackColumnCreation : FbEndToEndFixture
    {
        public TestRollbackColumnCreation()
            :base(typeof(CreateImplicitFk).Namespace)
        {            
        }

        [Test]
        public void Rollback_ColumnCreatedOnTableWithImplicitlyCreatedFk_CreatedColumnShouldBeDropped()
        {
            Migrate();

            Rollback();

            ColumnExists("table2", "silly").ShouldBe(false);
        }
    }
}
