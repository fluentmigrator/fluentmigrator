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

        public static DatabaseServerOptions SqlServer2014 = new DatabaseServerOptions
        {
            ConnectionString = @"server=.\MSSQLSERVER2014;uid=test;pwd=test;Trusted_Connection=yes;database=FluentMigrator",
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
                // Set ServerType=1 if you are using fbembed.dll
                ConnectionString = String.Format("ServerType=0;User=SYSDBA;Password=masterkey;Database={0};Datasource=127.0.0.1;Port=3050;",
                    System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "fbtest.fdb")
                    ),
                IsEnabled = false
            };

        public static DatabaseServerOptions Oracle = new DatabaseServerOptions
        {
            // was not able to get TNS to work
            ConnectionString = "Data Source=(DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP)(HOST = localhost)(PORT = 1521)) ) (CONNECT_DATA = (SERVICE_NAME = XE) ) );User ID=test;Password=test;",
            IsEnabled = false
        };

        public static DatabaseServerOptions Db2 = new DatabaseServerOptions
        {
            ConnectionString = "Database=;UserID=TEST;DataSource=;Password=Testing;DefaultCollection=TEST;DataCompression=True;ConnectTimeout=60;",
            IsEnabled = false
        };

        public static DatabaseServerOptions Hana = new DatabaseServerOptions
        {            
            ConnectionString = "Server=Server:Port;UserName=UserId;Password=Password;Current Schema='\"DbName\"'",
            IsEnabled = false
        };

        public class DatabaseServerOptions
        {
            public string ConnectionString;
            public bool IsEnabled = true;
        }
    }
}