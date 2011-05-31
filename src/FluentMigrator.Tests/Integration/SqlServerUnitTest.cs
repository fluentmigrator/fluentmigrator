#region License
// 
// Copyright (c) 2011, Grant Archibald
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using NUnit.Framework;

namespace FluentMigrator.Tests.Integration
{
   /// <summary>
   /// A base class for SQL Server unit test that allow a temporary SQL Server database to be quickly created and dropped for each unit test
   /// </summary>
   /// <remarks>The database is dropped between each unit test to ensure the independance of</remarks>
   public class SqlServerUnitTest
   {
      /// <summary>
      /// The connection string that wil be sued to create temporary databses
      /// </summary>
      /// <remarks>The unit test user executing the test must have teh following rights
      /// <para>db_creator rights</para>
      /// <para>execute rights for master.dbo.sp_detach_db</para>
      /// </remarks>
      public string MasterConnectionString = @"Data Source=.\SqlExpress;Integrated Security=True";

      /// <summary>
      /// Teh connection string that the unit test can use to connect to the temporary unit test database
      /// </summary>
      public string ConnectionString;

      /// <summary>
      /// The blank temporary database that wll be used as a template to be copied for each unit test
      /// </summary>
      public string BlankDatabaseName;

      /// <summary>
      /// The SQL Server .mdf file where <seealso cref="BlankDatabaseName"/> is stored on the filesystem
      /// </summary>
      public string BlankTemplateDatabaseFile;

      /// <summary>
      /// The name of the unit test database where tests will be executed against
      /// </summary>
      public string TestDb;

      /// <summary>
      /// The SQL Server .mdf file where <seealso cref="TestDb"/> is stored on the filesystem
      /// </summary>
      private string _testDbFile;
      
      /// <summary>
      /// The path where SQL Server database files are loacted for the SQL Server instance
      /// </summary>
      public string DatabasePath;

      [TestFixtureSetUp]
      public void TestFixtureSetUp()
      {
         BlankDatabaseName = GenerateDbName();

         // Create a empty database that is used as template to attach for each unit test
         DatabasePath = CreateDatabase(BlankDatabaseName);

         BlankTemplateDatabaseFile = Path.Combine(DatabasePath, BlankDatabaseName + ".mdf");

         DetachDatabase(BlankDatabaseName);

         // Remove the log file
         File.Delete(BlankTemplateDatabaseFile.Replace(".mdf", ".ldf"));
      }

      [TestFixtureTearDown]
      public void TestFixtureTearDown()
      {
         File.Delete(BlankTemplateDatabaseFile);
      }

      /// <summary>
      /// Creates a new test dtaabase based on the detached databse
      /// </summary>
      [SetUp]
      public virtual void Setup()
      {
         TestDb = GenerateDbName();
         _testDbFile = Path.Combine(DatabasePath, TestDb + ".mdf");
         File.Copy(BlankTemplateDatabaseFile, _testDbFile);

         CreateDatabaseWithAttachedFile(TestDb, _testDbFile);

         // Setup connection string for unit test to connect to temp db
         ConnectionString = @"Data Source=.\SqlExpress;Integrated Security=True;Initial Catalog=" + TestDb;
      }

      [TearDown]
      public virtual void TearDown()
      {
         DropDatabase(TestDb);
      }

      /// <summary>
      /// Creates a new database file in the default file location
      /// </summary>
      /// <param name="databaseName">The name of teh database to be created</param>
      /// <returns></returns>
      private string CreateDatabase(string databaseName)
      {
         using (var conn = new SqlConnection(MasterConnectionString))
         {
            var cmd = conn.CreateCommand();
            cmd.CommandText = string.Format("CREATE DATABASE [{0}]", databaseName);
            conn.Open();

            cmd.ExecuteNonQuery();

            // Return teh location of where database files are located
            cmd.CommandText =
               @"SELECT SUBSTRING(physical_name, 1,CHARINDEX(N'master.mdf',
LOWER(physical_name)) - 1) DataFileLocation
FROM master.sys.master_files
WHERE database_id = 1 AND FILE_ID = 1";
            return cmd.ExecuteScalar() as string;
         }
      }

      /// <summary>
      /// Generates a database name using a leading character and a Guid
      /// </summary>
      /// <returns></returns>
      private static string GenerateDbName()
      {
         return "A" + Guid.NewGuid().ToString().Replace("-", "");
      }

      /// <summary>
      /// Detaches a database from SQL Server but leaves the data files (mdf/ldf) on the filesystem
      /// </summary>
      /// <param name="databaseName">The database to be detached</param>
      private void DetachDatabase(string databaseName)
      {
         try
         {
            using (var conn = new SqlConnection(MasterConnectionString))
            {
               conn.Open();
               var cmd = conn.CreateCommand();
               try
               {

                  cmd.CommandText = "master.dbo.sp_detach_db";
                  cmd.CommandType = CommandType.StoredProcedure;
                  cmd.Parameters.Add(new SqlParameter("dbname", databaseName));
                  cmd.ExecuteNonQuery();
               }
// ReSharper disable EmptyGeneralCatchClause
               catch
// ReSharper restore EmptyGeneralCatchClause
               {

               }
            }
         }
// ReSharper disable EmptyGeneralCatchClause
         catch
// ReSharper restore EmptyGeneralCatchClause
         {

         }

      }

      /// <summary>
      /// Deletes the database from SQL Server and the file system
      /// </summary>
      /// <param name="databaseName">The database to be dropped</param>
      private void DropDatabase(string databaseName)
      {
         try
         {
            using (var conn = new SqlConnection(MasterConnectionString))
            {
               conn.Open();
               var cmd = conn.CreateCommand();
               try
               {

                  cmd.CommandText = string.Format("alter database [{0}] set single_user with rollback immediate ",
                                                  databaseName);
                  cmd.ExecuteNonQuery();
               }
// ReSharper disable EmptyGeneralCatchClause
               catch
// ReSharper restore EmptyGeneralCatchClause
               {

               }

               try
               {
                  cmd.CommandText = string.Format("DROP DATABASE [{0}]", databaseName);

                  cmd.ExecuteNonQuery();
               }
// ReSharper disable EmptyGeneralCatchClause
               catch
// ReSharper restore EmptyGeneralCatchClause
               {

               }

            }
         }
// ReSharper disable EmptyGeneralCatchClause
         catch
// ReSharper restore EmptyGeneralCatchClause
         {

         }

      }

      /// <summary>
      /// Creates a new database by attaching an existing detached database file
      /// </summary>
      /// <param name="databaseName">The database to be created</param>
      /// <param name="databaseFile">The data file to be attached</param>
      private void CreateDatabaseWithAttachedFile(string databaseName, string databaseFile)
      {
         using (var conn = new SqlConnection(MasterConnectionString))
         {
            var cmd = conn.CreateCommand();
            cmd.CommandText = string.Format("CREATE DATABASE [{0}] ON (FILENAME = '{1}') FOR ATTACH", databaseName, databaseFile);
            conn.Open();

            cmd.ExecuteNonQuery();
         }
      }
      
   }
}