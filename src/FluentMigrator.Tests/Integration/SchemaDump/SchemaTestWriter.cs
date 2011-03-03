using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentMigrator;
using FluentMigrator.Model;
using FluentMigrator.SchemaDump.SchemaWriters;

namespace FluentMigrator.Tests.Integration.SchemaDump {
    public class SchemaTestWriter : SchemaWriterBase {
        public override void WriteToStream(ICollection<TableDefinition> tables, System.IO.StreamWriter output) {
            int tableCount = tables.Count;
            int columnCount = tables.Select(t => t.Columns.Count).Sum();
            int indexCount = tables.Select(t => t.Indexes.Count).Sum();
            int keyCount = tables.Select(t => t.ForeignKeys.Count).Sum();

            output.Write(GetMessage(tableCount, columnCount, indexCount, keyCount));
        }

        public string GetMessage(int tableCount, int columnCount, int indexCount, int keyCount) {
            return String.Format("tables: {0}; columns: {1}; indexes: {2}; keys: {3}",
                tableCount, columnCount, indexCount, keyCount);
        }
    }
}
