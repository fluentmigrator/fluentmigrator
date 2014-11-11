using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using FluentMigrator.Model;

namespace FluentMigrator.Runner.Processors.Firebird
{
    public sealed class FirebirdTableSchema
    {
        public TableInfo TableMeta { get; private set; }
        public List<ColumnInfo> Columns { get; private set; }
        public List<IndexInfo> Indexes { get; private set; }
        public List<ConstraintInfo> Constraints { get; private set; }
        public List<TriggerInfo> Triggers { get; private set; }

        public string TableName { get; private set; }
        public FirebirdProcessor Processor { get; private set; }
        public TableDefinition Definition { get; private set; }
        public bool HasPrimaryKey { get { return Definition.Columns.Any(x => x.IsPrimaryKey); } }

        public FirebirdTableSchema(string tableName, FirebirdProcessor processor)
        {
            TableName = tableName;
            Processor = processor;
            Definition = new TableDefinition()
            {
                Name = tableName,
                SchemaName = String.Empty
            };
            Load();
        }

        public void Load()
        {
            LoadMeta();
            LoadColumns();
            LoadIndexes();
            LoadConstraints();
            LoadTriggers();
        }

        private void LoadMeta()
        {
            TableMeta = TableInfo.Read(Processor, TableName);
        }

        private void LoadColumns()
        {
            Columns = ColumnInfo.Read(Processor, TableMeta);
            foreach (ColumnInfo column in Columns)
            {
                ColumnDefinition colDef = new ColumnDefinition()
                {
                    TableName = TableMeta.Name,
                    Name = column.Name,
                    DefaultValue = column.DefaultValue == DBNull.Value ? new ColumnDefinition.UndefinedDefaultValue() : column.DefaultValue,
                    IsNullable = column.IsNullable,
                    Type = column.DBType,
                    Precision = column.Precision ?? 0,
                    Size = column.Size ?? 0
                };
                if (colDef.Type == null)
                    colDef.CustomType = column.CustomType;

                Definition.Columns.Add(colDef);
            }
        }

        private void LoadIndexes()
        {
            Indexes = IndexInfo.Read(Processor, TableMeta);
            foreach (IndexInfo index in Indexes)
            {
                IndexDefinition indexDef = new IndexDefinition()
                {
                    Name = index.Name,
                    TableName = TableMeta.Name,
                    IsUnique = index.IsUnique
                };
                index.Columns.ForEach(x => indexDef.Columns.Add(
                    new IndexColumnDefinition()
                    {
                        Name = x,
                        Direction = index.IsAscending ? Direction.Ascending : Direction.Descending
                    }));

                Definition.Indexes.Add(indexDef);
            }
        }
        private void RemoveIndex(string indexName)
        {
            if (Definition.Indexes.Any(x => x.Name == indexName))
            {
                IndexDefinition indexDef = Definition.Indexes.First(x => x.Name == indexName);
                Definition.Indexes.Remove(indexDef);
            }
        }

        private void LoadConstraints()
        {
            Constraints = ConstraintInfo.Read(Processor, TableMeta);
            foreach (ConstraintInfo constraint in Constraints)
            {
                List<string> columns = new List<string>();
                if (Indexes.Any(x => x.Name == constraint.IndexName))
                    columns = Indexes.First(x => x.Name == constraint.IndexName).Columns;

                foreach (ColumnDefinition column in Definition.Columns)
                {
                    if (columns.Contains(column.Name))
                    {
                        if (constraint.IsPrimaryKey)
                        {
                            column.IsPrimaryKey = true;
                            column.PrimaryKeyName = constraint.Name;
                            RemoveIndex(constraint.Name);
                        }

                        if (constraint.IsNotNull)
                            column.IsNullable = false;

                        if (constraint.IsUnique)
                            column.IsUnique = true;
                    }
                }

                if (constraint.IsForeignKey)
                {
                    ForeignKeyDefinition fkDef = new ForeignKeyDefinition()
                    {
                        Name = constraint.Name,
                        ForeignTable = TableMeta.Name,
                        ForeignColumns = columns,
                        PrimaryTable = constraint.ForeignIndex.TableName,
                        PrimaryColumns = constraint.ForeignIndex.Columns,
                        OnUpdate = constraint.UpdateRule,
                        OnDelete = constraint.DeleteRule
                    };

                    RemoveIndex(constraint.Name);

                    Definition.ForeignKeys.Add(fkDef);
                }
            }
        }

        private void LoadTriggers()
        {
            Triggers = TriggerInfo.Read(Processor, TableMeta);
        }

    }
}
