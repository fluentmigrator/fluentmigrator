
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using FluentMigrator.Model;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Generators.SqlServer;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.SqlServer;
using FluentMigrator.SchemaGen.SchemaReaders;
using FluentMigrator.SchemaGen.SchemaWriters;

namespace FluentMigrator.SchemaGen
{
    class Program
    {
        private static void Main(string[] args)
        {
            var options = new Options();
            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {
                GenDbSchema(options);
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

        public static void GenDbSchema(IOptions options)
        {
            try
            {
                // We don't use this code generator - really need to remove this dependency from 
                IMigrationGenerator generator = new SqlServer2008Generator();
                IAnnouncer announcer = new ConsoleAnnouncer { ShowElapsedTime = true, ShowSql = false };
                IMigrationProcessorOptions processorOptions = new ProcessorOptions();
                IDbFactory factory = new SqlServerDbFactory();

                if (options.DbName != null)
                {
                    using (IDbConnection cnn = GetDbConnection(options.DbName))
                    {
                        cnn.Open();

                        var processor = new SqlServerProcessor(cnn, generator, announcer, processorOptions, factory);
                        IDbSchemaReader reader = new SqlServerSchemaReader(processor, announcer);

                        IMigrationWriter migrationWriter = new FmInitialMigrationWriter(options, reader);
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

                        var processor1 = new SqlServerProcessor(cnn1, generator, announcer, processorOptions, factory);
                        IDbSchemaReader reader1 = new SqlServerSchemaReader(processor1, announcer);

                        var processor2 = new SqlServerProcessor(cnn2, generator, announcer, processorOptions, factory);
                        IDbSchemaReader reader2 = new SqlServerSchemaReader(processor2, announcer);

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
                Console.WriteLine("Press any key to continue.");
                Console.ReadKey();
                Environment.Exit(1);
            }
        }
    }
}
