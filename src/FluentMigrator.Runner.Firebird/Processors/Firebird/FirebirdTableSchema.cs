#region License
//
// Copyright (c) 2007-2018, Fluent Migrator Project
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

using System;
using System.Collections.Generic;
using System.Linq;

using FluentMigrator.Model;
using FluentMigrator.Runner.Generators.Firebird;
using FluentMigrator.Runner.Models;

namespace FluentMigrator.Runner.Processors.Firebird
{
    internal sealed class FirebirdTableSchema
    {
        private readonly FirebirdQuoter _quoter;
        public TableInfo TableMeta { get; private set; }
        public List<ColumnInfo> Columns { get; private set; }
        public List<IndexInfo> Indexes { get; private set; }
        public List<ConstraintInfo> Constraints { get; private set; }
        public List<TriggerInfo> Triggers { get; private set; }

        public string TableName { get; }
        public bool Exists => TableMeta?.Exists ?? false;
        public FirebirdProcessor Processor { get; }
        public FirebirdTableDefinition Definition { get; }
        public bool HasPrimaryKey { get { return Definition.Columns.Any(x => x.IsPrimaryKey); } }

        public FirebirdTableSchema(string tableName, FirebirdProcessor processor, FirebirdQuoter quoter)
        {
            _quoter = quoter;
            TableName = tableName;
            Processor = processor;
            Definition = new FirebirdTableDefinition()
            {
                Name = tableName,
                SchemaName = string.Empty
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
            TableMeta = TableInfo.Read(Processor, TableName, _quoter);
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
                    Precision = column.Precision,
                    Size = column.CharacterLength
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
