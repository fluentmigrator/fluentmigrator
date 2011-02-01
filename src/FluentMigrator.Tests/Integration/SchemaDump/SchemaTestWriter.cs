using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentMigrator;
using FluentMigrator.Model;

namespace FluentMigrator.Tests.Integration.SchemaDump {
    public class SchemaTestWriter : FluentMigrator.Runner.SchemaWriters.SchemaWriterBase {
        public override void WriteToStream(ICollection<TableDefinition> tables, System.IO.StreamWriter output) {
            int tableCount = 0, indexCount = 0, keyCount = 0, columnCount = 0;
            foreach (TableDefinition table in tables) {
                tableCount++;
                foreach (ColumnDefinition column in table.Columns) { columnCount++; }
                foreach (IndexDefinition index in table.Indexes) { indexCount++; }
                foreach (ForeignKeyDefinition fkey in table.ForiengKeys) { keyCount++; }
            }

            output.WriteLine(String.Format("tables: {0}; columns: {1}; indexes: {2}; keys: {3}", 
                tableCount, columnCount, indexCount, keyCount));
        }
    }
}
