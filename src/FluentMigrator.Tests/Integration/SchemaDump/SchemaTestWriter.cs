using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentMigrator;
using FluentMigrator.Model;

namespace FluentMigrator.Tests.Integration.SchemaDump {
    public class SchemaTestWriter : FluentMigrator.Runner.SchemaWriters.SchemaWriterBase {
        public override void WriteToStream(ICollection<TableDefinition> tables, System.IO.StreamWriter output) {
            throw new NotImplementedException();
        }
    }
}
