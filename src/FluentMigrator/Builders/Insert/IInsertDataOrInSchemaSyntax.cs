using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FluentMigrator.Builders.Insert {
    public interface IInsertDataOrInSchemaSyntax : IInsertDataSyntax {
        IInsertDataSyntax InSchema(string schemaName);
    }
}
