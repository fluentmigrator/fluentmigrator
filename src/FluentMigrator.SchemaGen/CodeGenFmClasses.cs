using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using FluentMigrator.Runner;
using FluentMigrator.SchemaGen.SchemaReaders;
using FluentMigrator.SchemaGen.SchemaWriters;

namespace FluentMigrator.SchemaGen
{
    /// <summary>
    /// Code generate Fluent Migrator C# classes.
    /// Entry point API if EXE is used as a DLL
    /// </summary>
    public class CodeGenFmClasses
    {
        private readonly IAnnouncer announcer;
        private readonly IOptions options;

        public CodeGenFmClasses(IOptions options, IAnnouncer announcer)
        {
            this.announcer = announcer;
            this.options = options;
        }

        public IEnumerable<string> GenClasses()
        {
            // Generate migration classes for the whole schema of a single database.
            if (options.Db != null)
            {
                using (IDbConnection cnn = new SqlConnection(options.Db))
                {
                    cnn.Open();

                    // Simulate an empty database in DB #1 so the full scheme of DB #2 is generated.
                    IDbSchemaReader reader1 = new EmptyDbSchemaReader();
                    IDbSchemaReader reader2 = new SqlServerSchemaReader(cnn, options);

                    IMigrationWriter writer = new FmDiffMigrationWriter(options, announcer, reader1, reader2);
                    return writer.WriteMigrationClasses();
                }
            }
            // Generate migration classes based on differences between two databases.
            else if (options.Db1 != null && options.Db2 != null)
            {
                using (IDbConnection cnn1 = new SqlConnection(options.Db1))
                using (IDbConnection cnn2 = new SqlConnection(options.Db2))
                {
                    cnn1.Open();
                    cnn2.Open();

                    IDbSchemaReader reader1 = new SqlServerSchemaReader(cnn1, options);
                    IDbSchemaReader reader2 = new SqlServerSchemaReader(cnn2, options);

                    IMigrationWriter writer = new FmDiffMigrationWriter(options, announcer, reader1, reader2);

                    return writer.WriteMigrationClasses();
                }
            }
            else
            {
                throw new DatabaseArgumentException("Must specificy EITHER Db OR Db1 and Db2 properties.");
            }
        }

    }
}