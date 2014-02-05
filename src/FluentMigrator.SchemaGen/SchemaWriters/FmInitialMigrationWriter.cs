using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentMigrator.Model;
using FluentMigrator.SchemaGen.SchemaReaders;

namespace FluentMigrator.SchemaGen.SchemaWriters
{
    /// <summary>
    /// Writes a Fluent Migrator class for a database schema
    /// </summary>
    public class FmInitialMigrationWriter : FmSchemaWriterBase, IMigrationWriter
    {
        private readonly IEnumerable<TableDefinition> tables;
        
        public FmInitialMigrationWriter(IOptions options, IDbSchemaReader reader)
            : base(options)
        {
            tables = reader.Tables;
            this.tables = ApplyTableFilter(tables);
        }

        public void WriteMigrations()
        {
            string classPath = Path.Combine(options.BaseDirectory, options.ClassName + ".cs");

            using (var fs = new FileStream(classPath, FileMode.Create))
            using (var writer = new StreamWriter(fs))
            {
                this.output = writer;
                WriteClass();
                writer.Flush();
            }
        }

        protected override void WriteUpMethod()
        {
            output.WriteLine("\t\tpublic override void Up()");
            output.WriteLine("\t\t{");

            //tables = tables.Where(table => table.Name == "tblInsAsset");

            foreach (TableDefinition table in tables)
            {
                CreateTable(table);
            }

            // Create foreign keys AFTER the tables.
            foreach (TableDefinition table in tables)
            {
                if (table.ForeignKeys.Any())
                {
                    output.Write("{0}#region UP Table Foreign Keys {1}.{2}", Indent0, table.SchemaName, table.Name);
                    foreach (var fk in table.ForeignKeys)
                    {
                        CreateForeignKey(fk);
                    }
                    output.WriteLine("{0}#endregion", Indent0);
                }
            }

            output.WriteLine("\t\t}\n"); //end method

        }

        protected override void WriteDownMethod()
        {
            output.WriteLine("\t\tpublic override void Down()");
            output.WriteLine("\t\t{");

            // Drop foreign keys BEFORE the tables.
            foreach (TableDefinition table in tables)
            {
                DeleteTableForeignKeys(table);
            }

            foreach (TableDefinition table in tables)
            {
                DeleteTable(table);
            }

            output.WriteLine("\t\t}\n"); //end method
        }
    }
}