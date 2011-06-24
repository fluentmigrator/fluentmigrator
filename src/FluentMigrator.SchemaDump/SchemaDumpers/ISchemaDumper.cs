using System.Collections.Generic;
using System.Data;
using FluentMigrator.Model;

namespace FluentMigrator.SchemaDump.SchemaDumpers {
   public interface ISchemaDumper {
      IList<TableDefinition> ReadDbSchema();
       IList<TableDefinition> ReadTables();
      IList<ViewDefinition> ReadViews();
      DataSet ReadTableData(string schemaName, string tableName);
      IList<ProcedureDefinition> ReadProcedures();
      IList<FunctionDefinition> ReadFunctions();
   }
}
