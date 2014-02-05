using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using FluentMigrator.SchemaGen.SchemaReaders;
using FluentMigrator.SchemaGen.SchemaWriters;

namespace FluentMigrator.SchemaGen
{
    /// <summary>
    /// Entry point API. 
    /// </summary>
    /// <remarks>
    /// This EXE can be used as a DLL and 
    /// </remarks>
    public class GenDbSchema
    {
        private readonly IOptions options;

        public GenDbSchema(IOptions options)
        {
            this.options = options;
        }

        public void Run()
        {
            try
            {
                if (options.DbName != null)
                {
                    using (IDbConnection cnn = GetDbConnection(options.DbName))
                    {
                        cnn.Open();

                        // Simulate an empty database in DB #1 so the full scheme of DB #2 is generated.
                        IDbSchemaReader reader1 = new EmptyDbSchemaReader();
                        IDbSchemaReader reader2 = new SqlServerSchemaReader(cnn, options);

                        IMigrationWriter migrationWriter = new FmDiffMigrationWriter(options, reader1, reader2);
                        migrationWriter.WriteMigrations();
                    }
                }
                else if (options.DbName1 != null && options.DbName2 != null)
                {
                    using (IDbConnection cnn1 = GetDbConnection(options.DbName1))
                    using (IDbConnection cnn2 = GetDbConnection(options.DbName2))
                    {
                        cnn1.Open();
                        cnn2.Open();

                        IDbSchemaReader reader1 = new SqlServerSchemaReader(cnn1, options);
                        IDbSchemaReader reader2 = new SqlServerSchemaReader(cnn2, options);

                        IMigrationWriter writer1 = new FmDiffMigrationWriter(options, reader1, reader2);

                        writer1.WriteMigrations();
                    }
                }
                else
                {
                    throw new Exception("Specificy EITHER --db OR --db1 and --db2 database name options.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                if (ex.InnerException != null) Console.WriteLine(ex.InnerException.Message);
                //Console.WriteLine("Press any key to continue.");
                //Console.ReadKey();
                Environment.Exit(1);
            }
        }

        private static IDbConnection GetDbConnection(string dbName)
        {
            if (dbName.Contains("="))   // Is it a connection string?
            {
                return new SqlConnection(dbName);
            }
            else
            {
                // Use the App.config template to generate a connection string
                string cnnTemplate = ConfigurationManager.ConnectionStrings["template"].ConnectionString;
                string cnnString = cnnTemplate.Replace("{dbName}", dbName);
                return new SqlConnection(cnnString);
            }
        }



    }
}