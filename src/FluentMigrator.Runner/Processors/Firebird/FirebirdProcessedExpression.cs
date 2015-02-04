using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using FluentMigrator.Builders.Execute;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators.Firebird;

namespace FluentMigrator.Runner.Processors.Firebird
{
    public abstract class FirebirdProcessedExpressionBase
    {
        protected Type expressionType;
        public FirebirdProcessor Processor { get; private set; }
        public IMigrationExpression Expression { get; private set; }
        public bool CanUndo { get; private set; }
        public List<IMigrationExpression> UndoExpressions { get; private set; }

        protected FirebirdProcessedExpressionBase(IMigrationExpression expression, Type expressionType, FirebirdProcessor processor)
        {
            Processor = processor;
            Expression = expression;
            this.expressionType = expressionType;
            if (processor.FBOptions.UndoEnabled && !processor.IsRunningOutOfMigrationScope())
                SetupUndoExpressions();
        }

        protected string GenerateSql(IMigrationExpression expression)
        {
            string result = null;
            try
            {
                MethodInfo generatorMethod = Processor.Generator.GetType().GetMethod("Generate", new Type[] { expression.GetType() });
                if (generatorMethod == null)
                    throw new ArgumentOutOfRangeException(String.Format("Can't find generator for {0}", expression.ToString()));

                result = generatorMethod.Invoke(Processor.Generator, new object[] { expression }) as string;
            }
            catch (Exception e)
            {
                throw new ArgumentOutOfRangeException(String.Format("Can't find generator for {0}", expression.ToString()), e);
            }
            return result;
        }

        private bool IsReversible()
        {
            bool success = false;
            try
            {
                Expression.Reverse();
                success = true;
            }
            catch (NotSupportedException) { }
            return success;
        }

        #region Undo setup

        protected void SetupUndoExpressions()
        {
            UndoExpressions = new List<IMigrationExpression>();
            CanUndo = false;
            if (IsReversible())
            {
                CanUndo = true;
                UndoExpressions.Add(Expression.Reverse());
            }
            else
            {
                if (Expression is DeleteTableExpression)
                    SetupUndoDeleteTable(Expression as DeleteTableExpression);

                if (Expression is DeleteIndexExpression)
                    SetupUndoDeleteIndex(Expression as DeleteIndexExpression);

                if(Expression is DeleteColumnExpression)
                    SetupUndoDeleteColumn(Expression as DeleteColumnExpression);

                if (Expression is AlterColumnExpression)
                    SetupUndoAlterColumn(Expression as AlterColumnExpression);

                if (Expression is CreateSequenceExpression)
                    SetupUndoCreateSequence(Expression as CreateSequenceExpression);

                if (Expression is DeleteSequenceExpression)
                    SetupUndoDeleteSequence(Expression as DeleteSequenceExpression);

                if (Expression is UpdateDataExpression)
                    SetupUndoUpdateData(Expression as UpdateDataExpression);

                if (Expression is DeleteDataExpression)
                    SetupUndoDeleteData(Expression as DeleteDataExpression);

                //Skippables
                if (Expression is AlterTableExpression 
                    || Expression is DeleteSchemaExpression 
                    || Expression is CreateSchemaExpression
                    )
                    CanUndo = true;

            }
            
        }

        protected void SetupUndoAlterColumn(AlterColumnExpression expression)
        {
            CanUndo = true;
            FirebirdSchemaProvider schema = new FirebirdSchemaProvider(Processor);
            FirebirdTableSchema table = schema.GetTableSchema(expression.TableName);
            var quoter = new FirebirdQuoter();
            AlterColumnExpression alter = new AlterColumnExpression()
            {
                SchemaName = String.Empty,
                TableName = expression.TableName,
                Column = table.Definition.Columns.First(x => x.Name == quoter.ToFbObjectName(expression.Column.Name))
            };
            UndoExpressions.Add(alter);
        }

        protected void SetupUndoDeleteData(DeleteDataExpression expression)
        {
            CanUndo = true;
            FirebirdSchemaProvider schema = new FirebirdSchemaProvider(Processor);
            FirebirdTableSchema table = schema.GetTableSchema(expression.TableName);
            using (DataSet ds = Processor.ReadTableData(String.Empty, expression.TableName))
            {
                foreach (DeletionDataDefinition deletion in expression.Rows)
                {
                    InsertDataExpression insert = new InsertDataExpression() { SchemaName = String.Empty, TableName = expression.TableName };
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        bool match = true;
                        if (!expression.IsAllRows)
                        {
                            foreach (var where in deletion)
                            {
                                if (dr[where.Key].ToString() != where.Value.ToString())
                                {
                                    match = false;
                                    break;
                                }
                            }
                        }
                        if (match)
                        {
                            InsertionDataDefinition insertion = new InsertionDataDefinition();
                            foreach (ColumnDefinition colDef in table.Definition.Columns)
                            {
                                insertion.Add(new KeyValuePair<string, object>(colDef.Name, dr[colDef.Name]));
                            }
                            insert.Rows.Add(insertion);
                        }
                    }
                    UndoExpressions.Add(insert);
                }
            }
        }

        protected void SetupUndoUpdateData(UpdateDataExpression expression)
        {
            FirebirdSchemaProvider schema = new FirebirdSchemaProvider(Processor);
            FirebirdTableSchema table = schema.GetTableSchema(expression.TableName);
            bool hasPrimary = table.HasPrimaryKey;
            
            CanUndo = true;

            using (DataSet ds = Processor.ReadTableData(String.Empty, expression.TableName))
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    if (!hasPrimary)
                    {
                        CanUndo = false;
                        return;
                    }
                    bool match = true;
                    if (!expression.IsAllRows)
                    {
                        foreach (var where in expression.Where)
                        {
                            if (dr[where.Key].ToString() != where.Value.ToString())
                            {
                                match = false;
                                break;
                            }
                        }
                    }
                    if (match)
                    {
                        UpdateDataExpression update = new UpdateDataExpression() 
                        { 
                            SchemaName = expression.SchemaName, 
                            TableName = expression.TableName, 
                            IsAllRows = false,
                            Set = new List<KeyValuePair<string, object>>()
                        };
                        foreach (var set in expression.Set)
                        {
                            update.Set.Add(new KeyValuePair<string, object>(set.Key, dr[set.Key]));
                        }
                        foreach (ColumnDefinition colDef in table.Definition.Columns)
                        {
                            if (colDef.IsPrimaryKey)
                            {
                                if (update.Where == null)
                                    update.Where = new List<KeyValuePair<string, object>>();
                                update.Where.Add(new KeyValuePair<string, object>(colDef.Name, dr[colDef.Name]));
                            }
                        }
                        UndoExpressions.Add(update);
                    }
                }
            }
        }

        protected void SetupUndoDeleteSequence(DeleteSequenceExpression expression)
        {
            CanUndo = true;
            FirebirdSchemaProvider schema = new FirebirdSchemaProvider(Processor);
            SequenceInfo sequence = schema.GetSequence(expression.SequenceName);
            CreateSequenceExpression createSequence = new CreateSequenceExpression()
            {
                Sequence = new SequenceDefinition()
                {
                    Name = sequence.Name,
                    StartWith = sequence.CurrentValue
                }
            };
            UndoExpressions.Add(createSequence);
        }

        protected void SetupUndoCreateSequence(CreateSequenceExpression expression)
        {
            CanUndo = true;
            UndoExpressions.Add(new DeleteSequenceExpression() { SequenceName = expression.Sequence.Name });
        }

        protected void SetupUndoDeleteTable(DeleteTableExpression expression)
        {
            CanUndo = true;
            FirebirdSchemaProvider schema = new FirebirdSchemaProvider(Processor);
            
            //Table and columns
            CreateTableExpression table = new CreateTableExpression()
            {
                TableName = expression.TableName,
                SchemaName = expression.SchemaName,
            };

            TableDefinition tableDef = schema.GetTableDefinition(expression.TableName);

            foreach (ColumnDefinition colDef in tableDef.Columns)
                table.Columns.Add(colDef);
            
            UndoExpressions.Add(table);

            //Indices
            foreach (IndexDefinition indexDef in tableDef.Indexes)
            {
                CreateIndexExpression indexExpression = new CreateIndexExpression()
                {
                    Index = indexDef
                };
                UndoExpressions.Add(indexExpression);
            }

            //Foreign keys
            foreach (ForeignKeyDefinition fkDef in tableDef.ForeignKeys)
            {
                CreateForeignKeyExpression fkExpression = new CreateForeignKeyExpression()
                {
                    ForeignKey = fkDef
                };
                UndoExpressions.Add(fkExpression);
            }

            //Data
            InsertDataExpression values = new InsertDataExpression()
            {
                TableName = expression.TableName,
                SchemaName = expression.SchemaName
            };

            using (DataSet data = Processor.ReadTableData(String.Empty, expression.TableName))
            {
                int columnCount = data.Tables[0].Columns.Count;

                foreach (DataRow row in data.Tables[0].Rows)
                {
                    InsertionDataDefinition insert = new InsertionDataDefinition();
                    for (int i = 0; i < columnCount; i++)
                    {
                        insert.Add(new KeyValuePair<string, object>(data.Tables[0].Columns[i].ColumnName, row.ItemArray[i]));
                    }
                    values.Rows.Add(insert);
                }
            }

            UndoExpressions.Add(values);
            
            //Triggers
            FirebirdTableSchema tableSchema = schema.GetTableSchema(expression.TableName);
            foreach (TriggerInfo trigger in tableSchema.Triggers)
            {
                PerformDBOperationExpression createTrigger = Processor.CreateTriggerExpression(expression.TableName, trigger);
                UndoExpressions.Add(createTrigger);
            }


        }

        protected void SetupUndoDeleteIndex(DeleteIndexExpression expression)
        {
            CanUndo = true;
            FirebirdSchemaProvider schema = new FirebirdSchemaProvider(Processor);
            
            CreateIndexExpression index = new CreateIndexExpression()
            {
                Index = schema.GetIndex(expression.Index.TableName, expression.Index.Name)
            };

            UndoExpressions.Add(index);
        }

        protected void SetupUndoDeleteColumn(DeleteColumnExpression expression)
        {
            
            FirebirdSchemaProvider schema = new FirebirdSchemaProvider(Processor);
            FirebirdTableSchema table = schema.GetTableSchema(expression.TableName);
            bool hasPrimary = table.HasPrimaryKey;

            CanUndo = true;

            using (DataSet ds = Processor.ReadTableData(String.Empty, expression.TableName))
            {
                //Create columns
                foreach (string columnName in expression.ColumnNames)
                {
                    CreateColumnExpression createColumn = new CreateColumnExpression()
                    {
                        SchemaName = String.Empty,
                        TableName = expression.TableName,
                        Column = table.Definition.Columns.First(x => x.Name.ToUpper() == columnName.ToUpper())
                    };
                    UndoExpressions.Add(createColumn);

                    //NB: No need to recreate indices? Drop should fail if detects existing user created indices

                }

                //Update data
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    if (!hasPrimary)
                    {
                        CanUndo = false;
                        return;
                    }
                    UpdateDataExpression update = new UpdateDataExpression() 
                    { 
                        TableName = expression.TableName, 
                        IsAllRows = false, 
                        Set = new List<KeyValuePair<string, object>>(),
                        Where = new List<KeyValuePair<string, object>>()
                    };
                    foreach (string columnName in expression.ColumnNames)
                    {
                        update.Set.Add(new KeyValuePair<string, object>(columnName, dr[columnName]));
                    }
                    table.Definition.Columns.ToList().ForEach(col =>
                    {
                        if (col.IsPrimaryKey)
                            update.Where.Add(new KeyValuePair<string, object>(col.Name, dr[col.Name]));
                    });
                    UndoExpressions.Add(update);
                }
            }

        }

        public void AddUndoExpression(IMigrationExpression expression)
        {
            CanUndo = true;
            UndoExpressions.Add(expression);
        }

        #endregion

        public void Undo(IDbConnection connection)
        {
            UndoExpressions.ForEach(x => {
                using(IDbTransaction transaction = connection.BeginTransaction())
                {
                    Run(x, connection, transaction);
                    transaction.Commit();
                }
            });
        }

        protected void Run(IMigrationExpression expression, IDbConnection connection, IDbTransaction transaction)
        {
            if (expression is PerformDBOperationExpression)
            {
                (expression as PerformDBOperationExpression).Operation(connection, transaction);
                return;
            }
            string sql = GenerateSql(expression);
            if (String.IsNullOrEmpty(sql))
                return;
            Processor.Announcer.Sql(sql);
            using (var command = Processor.Factory.CreateCommand(sql, connection, transaction))
            {
                command.CommandTimeout = Processor.Options.Timeout;
                command.ExecuteNonQuery();
            }
        }

        public override string ToString()
        {
            return Expression.ToString();
        }

    }

    public sealed class FirebirdProcessedExpression<T> : FirebirdProcessedExpressionBase where T : IMigrationExpression
    {
        public FirebirdProcessedExpression(T expression, FirebirdProcessor processor)
            : base(expression, typeof(T), processor)
        {
        }
    }

    public sealed class FirebirdProcessedExpression : FirebirdProcessedExpressionBase
    {
        public FirebirdProcessedExpression(IMigrationExpression expression, Type expressionType, FirebirdProcessor processor)
            : base(expression, expressionType, processor)
        {
        }
    }
}
