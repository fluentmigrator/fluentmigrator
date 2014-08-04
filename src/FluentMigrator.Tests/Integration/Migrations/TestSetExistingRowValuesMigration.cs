using System;

namespace FluentMigrator.Tests.Integration.Migrations
{
   [Migration(5)]
   public class AddLastLoginDateToUser : Migration
   {
      public override void Up()
      {
         Alter.Table("Bar")
             .AddColumn("LastLoginDate")
             .AsDateTime()
             .NotNullable()
             .SetExistingRowsTo(DateTime.Today);
      }

      public override void Down()
      {
         Delete.Column("LastLoginDate")
             .FromTable("Bar");
      }
   }
}
