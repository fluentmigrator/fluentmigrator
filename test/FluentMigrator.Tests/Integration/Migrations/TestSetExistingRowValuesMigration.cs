using System;

namespace FluentMigrator.Tests.Integration.Migrations
{
   [Migration(5)]
   public class AddLastLoginDateToUser : Migration
   {
      public override void Up()
      {
         // SQLite doesn't support altering tables so we can't set the LastLoginDate
         // to be nullable and then change it to nullable once populated so we allow
         // this for non-SQLite setups but for SQLite we just set the column as nullable
         IfDatabase(t => t != ProcessorIdConstants.SQLite).Alter.Table("Bar")
             .AddColumn("LastLoginDate")
             .AsDateTime()
             .NotNullable()
             .SetExistingRowsTo(DateTime.Today);

        IfDatabase(ProcessorIdConstants.SQLite).Alter.Table("Bar")
             .AddColumn("LastLoginDate")
             .AsDateTime()
             .Nullable()
             .SetExistingRowsTo(DateTime.Today);
        }

      public override void Down()
      {
         Delete.Column("LastLoginDate")
             .FromTable("Bar");
      }
   }
}
