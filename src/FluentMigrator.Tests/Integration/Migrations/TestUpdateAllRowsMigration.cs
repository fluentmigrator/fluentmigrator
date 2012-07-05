﻿using System;

namespace FluentMigrator.Tests.Integration.Migrations
{
    [Migration(4)]
    public class AddBirthDateToUser : Migration
    {
        public override void Up()
        {
            Alter.Table("Bar")
                .AddColumn("SomeDate")
                .AsDateTime()
                .Nullable();

            Update.Table("Bar")
                .Set(new { SomeDate = DateTime.Today })
                .AllRows();

            Alter.Table("Bar")
                .AlterColumn("SomeDate")
                .AsDateTime()
                .NotNullable();
        }

        public override void Down()
        {
            Delete.Column("SomeDate")
                .FromTable("Bar");
        }
    }
}
