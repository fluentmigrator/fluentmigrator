using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

namespace FluentMigrator.Runner.Processors.SqlServer {
    public class SqlServer2000Processor : SqlServerProcessor {

        public SqlServer2000Processor(SqlConnection connection, IMigrationGenerator generator, IAnnouncer announcer, IMigrationProcessorOptions options)
            : base(connection,generator, announcer, options) { }

        public override bool SchemaExists(string schemaName) {
            return true;
        }
    }
}
