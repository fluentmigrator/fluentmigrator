using System;

namespace FluentMigrator.Tests
{
    public static class IntegrationTestOptions
    {
        public static DatabaseServerOptions SqlServer2005 = new DatabaseServerOptions
            {
                ConnectionString = @"server=.\SQLEXPRESS;uid=;pwd=;Trusted_Connection=yes;database=FluentMigrator",
                IsEnabled = false
            };

        public static DatabaseServerOptions SqlServer2008 = new DatabaseServerOptions
            {
                ConnectionString = @"server=.\SQLEXPRESS;uid=;pwd=;Trusted_Connection=yes;database=FluentMigrator",
                IsEnabled = false
            };

        public static DatabaseServerOptions SqlServer2012 = new DatabaseServerOptions
        {
            ConnectionString = @"server=.\SQLEXPRESS;uid=test;pwd=test;Trusted_Connection=yes;database=FluentMigrator",
            IsEnabled = false 
        };

        public static DatabaseServerOptions SqlServerCe = new DatabaseServerOptions
            {
                ConnectionString = @"Data Source=TestDatabase.sdf",
                IsEnabled = false
            };

        public static DatabaseServerOptions Jet = new DatabaseServerOptions
        {
            ConnectionString = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=c:\temp\FMTest.mdb;",
            IsEnabled = false
        };

        public static DatabaseServerOptions SqlLite = new DatabaseServerOptions
            {
                ConnectionString = @"Data Source=:memory:;Version=3;New=True;",
                IsEnabled = false
            };

        public static DatabaseServerOptions MySql = new DatabaseServerOptions
            {
                ConnectionString = @"Database=FluentMigrator;Data Source=localhost;User Id=test;Password=test;",
                IsEnabled = false
            };

        public static DatabaseServerOptions Postgres = new DatabaseServerOptions
            {
                ConnectionString = "Server=127.0.0.1;Port=5432;Database=FluentMigrator;User Id=test;Password=test;",
                IsEnabled = false
            };

        public static DatabaseServerOptions Firebird = new DatabaseServerOptions
            {
                ConnectionString = String.Format("ServerType=0;User=SYSDBA;Password=masterkey;Database={0};Datasource=127.0.0.1;Port=3050;",
                    System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "fbtest.fdb")
                    ),
                IsEnabled = false
            };

        public static DatabaseServerOptions Oracle = new DatabaseServerOptions
        {
            // was not able to get TNS to work
            ConnectionString = "Data Source=XE;User Id=test;Password=test",
            IsEnabled = false
        };

        public class DatabaseServerOptions
        {
            public string ConnectionString;
            public bool IsEnabled = true;
        }
    }
}