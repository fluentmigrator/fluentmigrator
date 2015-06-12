using System;
using FluentMigrator.Runner.Initialization;

namespace FluentMigrator.Runner.Processors
{
    public class ConnectionlessProcessor: IMigrationProcessor
    {
        public IMigrationGenerator Generator { get; set; }
        public IRunnerContext Context { get; set; }
        public IAnnouncer Announcer { get; set; }
        public IMigrationProcessorOptions Options {get;set;}

        public ConnectionlessProcessor(IMigrationGenerator generator, IRunnerContext context, IMigrationProcessorOptions options)
        {
            Generator = generator;
            Context = context;
            Announcer = Context.Announcer;
            Options = options;
        }

        public string ConnectionString
        {
            get { return "No connection"; }
        }

        public void Execute(string template, params object[] args)
        {
            Process(string.Format(template, args));
        }

        public System.Data.DataSet ReadTableData(string schemaName, string tableName)
        {
            throw new NotImplementedException("Method is not supported by the connectionless processor");
        }

        public System.Data.DataSet Read(string template, params object[] args)
        {
            throw new NotImplementedException("Method is not supported by the connectionless processor");
        }

        public bool Exists(string template, params object[] args)
        {
            throw new NotImplementedException("Method is not supported by the connectionless processor");
        }

        public void BeginTransaction()
        {
            
        }

        public void CommitTransaction()
        {
            
        }

        public void RollbackTransaction()
        {
            
        }

        protected void Process(string sql)
        {
            Announcer.Sql(sql);
        }

        public void Process(Expressions.CreateSchemaExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        public void Process(Expressions.DeleteSchemaExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        public void Process(Expressions.AlterTableExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        public void Process(Expressions.AlterColumnExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        public void Process(Expressions.CreateTableExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        public void Process(Expressions.CreateColumnExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        public void Process(Expressions.DeleteTableExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        public void Process(Expressions.DeleteColumnExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        public void Process(Expressions.CreateForeignKeyExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        public void Process(Expressions.DeleteForeignKeyExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        public void Process(Expressions.CreateIndexExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        public void Process(Expressions.DeleteIndexExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        public void Process(Expressions.RenameTableExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        public void Process(Expressions.RenameColumnExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        public void Process(Expressions.InsertDataExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        public void Process(Expressions.AlterDefaultConstraintExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        public void Process(Builders.Execute.PerformDBOperationExpression expression)
        {
            Announcer.Say("Performing DB Operation");
        }

        public void Process(Expressions.DeleteDataExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        public void Process(Expressions.UpdateDataExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        public void Process(Expressions.AlterSchemaExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        public void Process(Expressions.CreateSequenceExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        public void Process(Expressions.DeleteSequenceExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        public void Process(Expressions.CreateConstraintExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        public void Process(Expressions.DeleteConstraintExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        public void Process(Expressions.DeleteDefaultConstraintExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        public bool SchemaExists(string schemaName)
        {
            throw new NotImplementedException("Method is not supported by the connectionless processor");
        }

        public bool TableExists(string schemaName, string tableName)
        {
            throw new NotImplementedException("Method is not supported by the connectionless processor");
        }

        public bool ColumnExists(string schemaName, string tableName, string columnName)
        {
            throw new NotImplementedException("Method is not supported by the connectionless processor");
        }

        public bool ConstraintExists(string schemaName, string tableName, string constraintName)
        {
            throw new NotImplementedException("Method is not supported by the connectionless processor");
        }

        public bool IndexExists(string schemaName, string tableName, string indexName)
        {
            throw new NotImplementedException("Method is not supported by the connectionless processor");
        }

        public bool SequenceExists(string schemaName, string sequenceName)
        {
            throw new NotImplementedException("Method is not supported by the connectionless processor");
        }

        public bool DefaultValueExists(string schemaName, string tableName, string columnName, object defaultValue)
        {
            throw new NotImplementedException("Method is not supported by the connectionless processor");
        }

        public string DatabaseType
        {
            get { return Context.Database; }
        }

        public void Dispose()
        {
            
        }
    }
}
