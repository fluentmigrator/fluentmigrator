using System;
using System.Collections.Generic;
using System.Data;
using FluentMigrator.Builders.Execute;
using FluentMigrator.Expressions;

namespace FluentMigrator.Runner.Processors {
    public class MultiDatabaseMigrationProcessor : IMultiDatabaseMigrationProcessor {
        public IMigrationProcessor DefaultProcessor { get; private set; }
        private readonly IDictionary<string, IMigrationProcessor> _otherProcessors;
        private readonly IDictionary<IMigrationExpression, string> _expressionDatabaseKeys = new Dictionary<IMigrationExpression, string>();

        public MultiDatabaseMigrationProcessor(IMigrationProcessor defaultProcessor, IDictionary<string, IMigrationProcessor> otherProcessors)
        {
            if (defaultProcessor == null) throw new ArgumentNullException("defaultProcessor");
            if (otherProcessors == null) throw new ArgumentNullException("otherProcessors");

            DefaultProcessor = defaultProcessor;
            _otherProcessors = otherProcessors;
        }

        public bool SchemaExists(string schemaName)
        {
            return DefaultProcessor.SchemaExists(schemaName);
        }

        public bool TableExists(string schemaName, string tableName)
        {
            return DefaultProcessor.TableExists(schemaName, tableName);
        }

        public bool ColumnExists(string schemaName, string tableName, string columnName)
        {
            return DefaultProcessor.ColumnExists(schemaName, tableName, columnName);
        }

        public bool ConstraintExists(string schemaName, string tableName, string constraintName)
        {
            return DefaultProcessor.ConstraintExists(schemaName, tableName, constraintName);
        }

        public bool IndexExists(string schemaName, string tableName, string indexName)
        {
            return DefaultProcessor.IndexExists(schemaName, tableName, indexName);
        }

        public bool SequenceExists(string schemaName, string sequenceName)
        {
            return DefaultProcessor.SequenceExists(schemaName, sequenceName);
        }

        public bool DefaultValueExists(string schemaName, string tableName, string columnName, object defaultValue)
        {
            return DefaultProcessor.DefaultValueExists(schemaName, tableName, columnName, defaultValue);
        }

        public string DatabaseType
        {
            get { return DefaultProcessor.DatabaseType; }
        }

        public void Dispose()
        {
            DefaultProcessor.Dispose();
            foreach (var pair in _otherProcessors)
            {
                pair.Value.Dispose();
            }
        }

        public IMigrationProcessorOptions Options
        {
            get { return DefaultProcessor.Options; }
        }

        public string ConnectionString
        {
            get { return DefaultProcessor.ConnectionString; }
        }

        public void Execute(string template, params object[] args)
        {
            DefaultProcessor.Execute(template, args);
        }

        public DataSet ReadTableData(string schemaName, string tableName)
        {
            return DefaultProcessor.ReadTableData(schemaName, tableName);
        }

        public DataSet Read(string template, params object[] args)
        {
            return DefaultProcessor.Read(template, args);
        }

        public bool Exists(string template, params object[] args)
        {
            return DefaultProcessor.Exists(template, args);
        }

        public void BeginTransaction()
        {
            // this is imperfect, but significantly improving it would be too large of an effort
            DefaultProcessor.BeginTransaction();
            foreach (var processor in _otherProcessors)
            {
                processor.Value.BeginTransaction();
            }
        }

        public void CommitTransaction()
        {
            DefaultProcessor.CommitTransaction();
            foreach (var processor in _otherProcessors)
            {
                processor.Value.CommitTransaction();
            }
        }

        public void RollbackTransaction()
        {
            DefaultProcessor.RollbackTransaction();
            foreach (var processor in _otherProcessors)
            {
                processor.Value.RollbackTransaction();
            }
        }

        public void Process(CreateSchemaExpression expression)
        {
            GetProcessor(expression).Process(expression);
        }

        public void Process(DeleteSchemaExpression expression)
        {
            GetProcessor(expression).Process(expression);
        }

        public void Process(AlterTableExpression expression)
        {
            GetProcessor(expression).Process(expression);
        }

        public void Process(AlterColumnExpression expression)
        {
            GetProcessor(expression).Process(expression);
        }

        public void Process(CreateTableExpression expression)
        {
            GetProcessor(expression).Process(expression);
        }

        public void Process(CreateColumnExpression expression)
        {
            GetProcessor(expression).Process(expression);
        }

        public void Process(DeleteTableExpression expression)
        {
            GetProcessor(expression).Process(expression);
        }

        public void Process(DeleteColumnExpression expression)
        {
            GetProcessor(expression).Process(expression);
        }

        public void Process(CreateForeignKeyExpression expression)
        {
            GetProcessor(expression).Process(expression);
        }

        public void Process(DeleteForeignKeyExpression expression)
        {
            GetProcessor(expression).Process(expression);
        }

        public void Process(CreateIndexExpression expression)
        {
            GetProcessor(expression).Process(expression);
        }

        public void Process(DeleteIndexExpression expression)
        {
            GetProcessor(expression).Process(expression);
        }

        public void Process(RenameTableExpression expression)
        {
            GetProcessor(expression).Process(expression);
        }

        public void Process(RenameColumnExpression expression)
        {
            GetProcessor(expression).Process(expression);
        }

        public void Process(InsertDataExpression expression)
        {
            GetProcessor(expression).Process(expression);
        }

        public void Process(AlterDefaultConstraintExpression expression)
        {
            GetProcessor(expression).Process(expression);
        }

        public void Process(PerformDBOperationExpression expression)
        {
            GetProcessor(expression).Process(expression);
        }

        public void Process(DeleteDataExpression expression)
        {
            GetProcessor(expression).Process(expression);
        }

        public void Process(UpdateDataExpression expression)
        {
            GetProcessor(expression).Process(expression);
        }

        public void Process(AlterSchemaExpression expression)
        {
            GetProcessor(expression).Process(expression);
        }

        public void Process(CreateSequenceExpression expression)
        {
            GetProcessor(expression).Process(expression);
        }

        public void Process(DeleteSequenceExpression expression)
        {
            GetProcessor(expression).Process(expression);
        }

        public void Process(CreateConstraintExpression expression)
        {
            GetProcessor(expression).Process(expression);
        }

        public void Process(DeleteConstraintExpression expression)
        {
            GetProcessor(expression).Process(expression);
        }

        public void Process(DeleteDefaultConstraintExpression expression)
        {
            GetProcessor(expression).Process(expression);
        }

        public void AssignDatabaseKey(IMigrationExpression expression, string databaseKey)
        {
            if (!_otherProcessors.ContainsKey(databaseKey))
                throw new ArgumentException("Database key '" + databaseKey + "' is not known.", "databaseKey");

            _expressionDatabaseKeys[expression] = databaseKey;
        }

        public bool HasDatabaseKey(string databaseKey)
        {
            return _otherProcessors.ContainsKey(databaseKey);
        }

        public IEnumerable<string> GetDatabaseKeys()
        {
            return _otherProcessors.Keys;
        }

        public IMigrationProcessor GetProcessorByDatabaseKey(string databaseKey) 
        {
            IMigrationProcessor processor;
            if (!_otherProcessors.TryGetValue(databaseKey, out processor))
                throw new ArgumentException("Database key '" + databaseKey + "' is not known.", "databaseKey");

            return processor;
        }

        private IMigrationProcessor GetProcessor(IMigrationExpression expression)
        {
            string databaseKey;
            if (!_expressionDatabaseKeys.TryGetValue(expression, out databaseKey))
                return DefaultProcessor;

            return _otherProcessors[databaseKey];
        }
    }
}
