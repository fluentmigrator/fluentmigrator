using System.Collections.Generic;
using System.Linq;
using FluentMigrator.Model;

namespace FluentMigrator.Runner.Processors.Firebird
{
    public class FirebirdSchemaProvider
    {
        protected Dictionary<string, FirebirdTableSchema> tableSchemas = new Dictionary<string, FirebirdTableSchema>();
        public FirebirdProcessor Processor { get; protected set; }
        
        public FirebirdSchemaProvider(FirebirdProcessor processor)
        {
            Processor = processor;
        }

        public ColumnDefinition GetColumnDefinition(string tableName, string columnName)
        {
            TableDefinition tableDef = GetTableDefinition(tableName);
            return tableDef.Columns.First(x => x.Name == columnName);
        }

        public TableDefinition GetTableDefinition(string tableName)
        {
            return GetTableSchema(tableName).Definition;
        }

        public FirebirdTableSchema GetTableSchema(string tableName)
        {
            if (tableSchemas.ContainsKey(tableName))
                return tableSchemas[tableName];
            return LoadTableSchema(tableName);
        }

        protected FirebirdTableSchema LoadTableSchema(string tableName)
        {
            FirebirdTableSchema schema = new FirebirdTableSchema(tableName, Processor);
            tableSchemas.Add(tableName, schema);
            return schema;
        }

        public IndexDefinition GetIndex(string tableName, string indexName)
        {
            TableDefinition tableDef = GetTableDefinition(tableName);
            if (tableDef.Indexes.Any(x => x.Name == indexName))
                return tableDef.Indexes.First(x => x.Name == indexName);
            return null;
        }

        public SequenceInfo GetSequence(string sequenceName)
        {
            return SequenceInfo.Read(Processor, sequenceName);
        }
        
    }

    
}
