#region License
//
// Copyright (c) 2018, Fluent Migrator Project
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
using System.Text;
using FluentMigrator.Expressions;
using FluentMigrator.Runner.Processors.Firebird;
using FluentMigrator.Model;
using System.Security.Cryptography;

namespace FluentMigrator.Runner.Generators.Firebird
{
    /// <summary>
    /// Provides logic for truncating Firebird object names to comply with maximum length restrictions.
    /// </summary>
    public class FirebirdTruncator
    {
        private readonly bool _enabled;
        private readonly bool _packKeyNames;

        /// <summary>
        /// Initializes a new instance of the <see cref="FluentMigrator.Runner.Generators.Firebird.FirebirdTruncator"/> class.
        /// </summary>
        /// <param name="enabled">Indicates whether truncation of object names is enabled.</param>
        /// <param name="packKeyNames">Indicates whether key names should be packed to reduce their length.</param>
        public FirebirdTruncator(bool enabled, bool packKeyNames)
        {
            _enabled = enabled;
            _packKeyNames = packKeyNames;
        }

        /// <summary>
        /// Truncates the names or identifiers within the provided <see cref="CreateSchemaExpression"/> 
        /// to ensure they comply with Firebird's maximum length restrictions.
        /// </summary>
        /// <param name="expression">
        /// The <see cref="CreateSchemaExpression"/> containing the schema details to be truncated.
        /// </param>
        /// <remarks>
        /// This method modifies the schema name or other identifiers in the provided expression
        /// to conform to Firebird's naming constraints.
        /// </remarks>
        public void Truncate(CreateSchemaExpression expression) { }
        /// <summary>
        /// Truncates the object names within the provided <see cref="FluentMigrator.Expressions.AlterSchemaExpression"/> 
        /// to ensure they comply with Firebird's maximum length restrictions.
        /// </summary>
        /// <param name="expression">
        /// The <see cref="FluentMigrator.Expressions.AlterSchemaExpression"/> containing the object names to be truncated.
        /// </param>
        /// <remarks>
        /// This method modifies the object names in the given expression to adhere to Firebird's constraints.
        /// </remarks>
        public void Truncate(AlterSchemaExpression expression) { }
        /// <summary>
        /// Truncates the object names within the specified <see cref="FluentMigrator.Expressions.DeleteSchemaExpression"/> 
        /// to ensure they comply with Firebird's maximum length restrictions.
        /// </summary>
        /// <param name="expression">The <see cref="FluentMigrator.Expressions.DeleteSchemaExpression"/> to process.</param>
        /// <remarks>
        /// This method modifies the schema name in the provided expression if truncation is enabled.
        /// </remarks>
        public void Truncate(DeleteSchemaExpression expression) { }

        /// <summary>
        /// Truncates the table name and its associated column definitions in the provided
        /// <see cref="FluentMigrator.Expressions.CreateTableExpression"/> to ensure compliance
        /// with Firebird's maximum length restrictions.
        /// </summary>
        /// <param name="expression">
        /// The <see cref="FluentMigrator.Expressions.CreateTableExpression"/> containing the table
        /// name and column definitions to be truncated.
        /// </param>
        /// <remarks>
        /// This method modifies the <paramref name="expression"/> in place by truncating the
        /// <see cref="FluentMigrator.Expressions.CreateTableExpression.TableName"/> and the
        /// names of its associated columns.
        /// </remarks>
        public void Truncate(CreateTableExpression expression)
        {
            expression.TableName = Truncate(expression.TableName);
            TruncateColumns(expression.Columns);
        }

        /// <summary>
        /// Truncates the table name in the provided <see cref="AlterTableExpression"/> 
        /// to ensure it adheres to Firebird's maximum length restrictions.
        /// </summary>
        /// <param name="expression">
        /// The <see cref="AlterTableExpression"/> containing the table name to be truncated.
        /// </param>
        /// <remarks>
        /// This method modifies the <see cref="AlterTableExpression.TableName"/> property directly.
        /// </remarks>
        public void Truncate(AlterTableExpression expression)
        {
            expression.TableName = Truncate(expression.TableName);
        }

        /// <summary>
        /// Truncates the table name in the provided <see cref="FluentMigrator.Expressions.DeleteTableExpression"/> 
        /// to ensure it adheres to Firebird's maximum length restrictions.
        /// </summary>
        /// <param name="expression">
        /// The <see cref="FluentMigrator.Expressions.DeleteTableExpression"/> containing the table name to be truncated.
        /// </param>
        /// <remarks>
        /// This method modifies the <see cref="FluentMigrator.Expressions.DeleteTableExpression.TableName"/> property directly.
        /// </remarks>
        public void Truncate(DeleteTableExpression expression)
        {
            expression.TableName = Truncate(expression.TableName);
        }

        /// <summary>
        /// Truncates the old and new table names in the provided <see cref="RenameTableExpression"/> 
        /// to ensure they conform to Firebird's naming constraints.
        /// </summary>
        /// <param name="expression">The <see cref="RenameTableExpression"/> containing the old and new table names to truncate.</param>
        /// <remarks>
        /// This method modifies the <paramref name="expression"/> in place by truncating its 
        /// <see cref="RenameTableExpression.OldName"/> and <see cref="RenameTableExpression.NewName"/> properties.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the <paramref name="expression"/> is <c>null</c>.
        /// </exception>
        public void Truncate(RenameTableExpression expression)
        {
            expression.OldName = Truncate(expression.OldName);
            expression.NewName = Truncate(expression.NewName);
        }

        /// <summary>
        /// Truncates the specified <see cref="ColumnDefinition"/> object to ensure its properties comply with Firebird's maximum length restrictions.
        /// </summary>
        /// <param name="column">The <see cref="ColumnDefinition"/> object to truncate.</param>
        /// <remarks>
        /// This method truncates the <see cref="ColumnDefinition.Name"/> and <see cref="ColumnDefinition.TableName"/> properties.
        /// If the column is a primary key, the <see cref="ColumnDefinition.PrimaryKeyName"/> is also truncated or packed based on the configuration.
        /// </remarks>
        public void Truncate(ColumnDefinition column)
        {
            column.Name = Truncate(column.Name);
            column.TableName = Truncate(column.TableName);
            if (column.IsPrimaryKey)
                column.PrimaryKeyName = _packKeyNames ? Pack(column.PrimaryKeyName) : Truncate(column.PrimaryKeyName);
        }

        /// <summary>
        /// Truncates the table name and column definition within the provided <see cref="CreateColumnExpression"/> 
        /// to ensure they comply with Firebird's maximum length restrictions.
        /// </summary>
        /// <param name="expression">The <see cref="CreateColumnExpression"/> to be truncated.</param>
        /// <remarks>
        /// This method modifies the <paramref name="expression"/> in place by truncating its <see cref="CreateColumnExpression.TableName"/> 
        /// and its associated <see cref="CreateColumnExpression.Column"/>.
        /// </remarks>
        public void Truncate(CreateColumnExpression expression)
        {
            expression.TableName = Truncate(expression.TableName);
            Truncate(expression.Column);
        }

        /// <summary>
        /// Truncates the table name and column definition within the specified <see cref="FluentMigrator.Expressions.AlterColumnExpression"/> 
        /// to ensure they comply with Firebird's maximum length restrictions.
        /// </summary>
        /// <param name="expression">The <see cref="FluentMigrator.Expressions.AlterColumnExpression"/> to be truncated.</param>
        /// <remarks>
        /// This method modifies the <paramref name="expression"/> in place by truncating its <see cref="FluentMigrator.Expressions.AlterColumnExpression.TableName"/> 
        /// and the associated <see cref="FluentMigrator.Expressions.AlterColumnExpression.Column"/>.
        /// </remarks>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if the <paramref name="expression"/> or its <see cref="FluentMigrator.Expressions.AlterColumnExpression.TableName"/> is <c>null</c>.
        /// </exception>
        public void Truncate(AlterColumnExpression expression)
        {
            expression.TableName = Truncate(expression.TableName);
            Truncate(expression.Column);
        }

        /// <summary>
        /// Truncates the table name and column names in the provided <see cref="DeleteColumnExpression"/> 
        /// to ensure they comply with Firebird's maximum length restrictions.
        /// </summary>
        /// <param name="expression">The <see cref="DeleteColumnExpression"/> containing the table and column names to truncate.</param>
        /// <remarks>
        /// This method modifies the <paramref name="expression"/> in place, updating its <see cref="DeleteColumnExpression.TableName"/> 
        /// and <see cref="DeleteColumnExpression.ColumnNames"/> properties with truncated values.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the <paramref name="expression"/> or its <see cref="DeleteColumnExpression.TableName"/> is <c>null</c>.
        /// </exception>
        public void Truncate(DeleteColumnExpression expression)
        {
            expression.TableName = Truncate(expression.TableName);
            expression.ColumnNames = TruncateNames(expression.ColumnNames);
        }

        /// <summary>
        /// Truncates the names of the old column, new column, and table in the provided
        /// <see cref="FluentMigrator.Expressions.RenameColumnExpression"/> to ensure they comply
        /// with Firebird's maximum length restrictions.
        /// </summary>
        /// <param name="expression">
        /// The <see cref="FluentMigrator.Expressions.RenameColumnExpression"/> containing the names
        /// to be truncated.
        /// </param>
        /// <remarks>
        /// This method modifies the <paramref name="expression"/> directly by truncating the
        /// <see cref="FluentMigrator.Expressions.RenameColumnExpression.OldName"/>,
        /// <see cref="FluentMigrator.Expressions.RenameColumnExpression.NewName"/>, and
        /// <see cref="FluentMigrator.Expressions.RenameColumnExpression.TableName"/> properties.
        /// </remarks>
        public void Truncate(RenameColumnExpression expression)
        {
            expression.OldName = Truncate(expression.OldName);
            expression.NewName = Truncate(expression.NewName);
            expression.TableName = Truncate(expression.TableName);
        }

        /// <summary>
        /// Truncates the names of the specified <see cref="IndexDefinition"/> to ensure they comply with Firebird's maximum length restrictions.
        /// </summary>
        /// <param name="index">The <see cref="IndexDefinition"/> whose names will be truncated.</param>
        /// <remarks>
        /// This method truncates the table name, index name, and column names within the provided <see cref="IndexDefinition"/>.
        /// If the <c>_packKeyNames</c> flag is enabled, the index name will be packed instead of truncated.
        /// </remarks>
        public void Truncate(IndexDefinition index)
        {
            index.TableName = Truncate(index.TableName);
            index.Name = _packKeyNames ? Pack(index.Name) : Truncate(index.Name);
            index.Columns.ToList().ForEach(x => x.Name = Truncate(x.Name));
        }

        /// <summary>
        /// Truncates the object names within the provided <see cref="CreateIndexExpression"/> 
        /// to ensure they comply with Firebird's maximum length restrictions.
        /// </summary>
        /// <param name="expression">The <see cref="CreateIndexExpression"/> containing the index to be truncated.</param>
        /// <remarks>
        /// This method modifies the <see cref="CreateIndexExpression.Index"/> property by truncating its name and other relevant properties.
        /// </remarks>
        public void Truncate(CreateIndexExpression expression)
        {
            Truncate(expression.Index);
        }

        /// <summary>
        /// Truncates the object names within the provided <see cref="DeleteIndexExpression"/> 
        /// to ensure they comply with Firebird's maximum length restrictions.
        /// </summary>
        /// <param name="expression">The <see cref="DeleteIndexExpression"/> containing the index to truncate.</param>
        /// <remarks>
        /// This method modifies the <see cref="IndexDefinition"/> within the provided expression
        /// by truncating its name and associated properties as necessary.
        /// </remarks>
        public void Truncate(DeleteIndexExpression expression)
        {
            Truncate(expression.Index);
        }

        /// <summary>
        /// Truncates the specified constraint definition to ensure that its table name, 
        /// constraint name, and column names comply with Firebird's maximum length restrictions.
        /// </summary>
        /// <param name="constraint">The <see cref="ConstraintDefinition"/> to truncate.</param>
        /// <remarks>
        /// The table name and constraint name are truncated or packed based on the configuration.
        /// Column names are also truncated to adhere to the length restrictions.
        /// </remarks>
        public void Truncate(ConstraintDefinition constraint)
        {
            constraint.TableName = Truncate(constraint.TableName);
            constraint.ConstraintName = _packKeyNames ? Pack(constraint.ConstraintName) : Truncate(constraint.ConstraintName);
            constraint.Columns = TruncateNames(constraint.Columns);
        }

        /// <summary>
        /// Truncates the object names within the provided <see cref="CreateConstraintExpression"/> to ensure they comply with Firebird's maximum length restrictions.
        /// </summary>
        /// <param name="expression">The <see cref="CreateConstraintExpression"/> containing the constraint definition to truncate.</param>
        /// <remarks>
        /// This method modifies the <see cref="ConstraintDefinition"/> within the provided expression by truncating its names.
        /// </remarks>
        public void Truncate(CreateConstraintExpression expression)
        {
            Truncate(expression.Constraint);
        }

        /// <summary>
        /// Truncates the names of objects defined in the provided <see cref="DeleteConstraintExpression"/> 
        /// to ensure they comply with Firebird's maximum length restrictions.
        /// </summary>
        /// <param name="expression">
        /// The <see cref="DeleteConstraintExpression"/> containing the constraint definition to be truncated.
        /// </param>
        /// <remarks>
        /// This method internally truncates the <see cref="ConstraintDefinition"/> associated with the provided expression.
        /// </remarks>
        public void Truncate(DeleteConstraintExpression expression)
        {
            Truncate(expression.Constraint);
        }

        /// <summary>
        /// Truncates the properties of the specified <see cref="FluentMigrator.Model.ForeignKeyDefinition"/> 
        /// to ensure they comply with Firebird's maximum length restrictions.
        /// </summary>
        /// <param name="foreignKey">
        /// The <see cref="FluentMigrator.Model.ForeignKeyDefinition"/> to be truncated.
        /// </param>
        /// <remarks>
        /// This method truncates the foreign key name, primary table name, foreign table name, 
        /// and their respective column names. If the <c>_packKeyNames</c> field is enabled, 
        /// the foreign key name is packed instead of truncated.
        /// </remarks>
        public void Truncate(ForeignKeyDefinition foreignKey)
        {
            foreignKey.Name = _packKeyNames ? Pack(foreignKey.Name) : Truncate(foreignKey.Name);
            foreignKey.PrimaryTable = Truncate(foreignKey.PrimaryTable);
            foreignKey.PrimaryColumns = TruncateNames(foreignKey.PrimaryColumns);
            foreignKey.ForeignTable = Truncate(foreignKey.ForeignTable);
            foreignKey.ForeignColumns = TruncateNames(foreignKey.ForeignColumns);
        }

        /// <summary>
        /// Truncates the names within the provided <see cref="CreateForeignKeyExpression"/> to ensure they comply with Firebird's maximum length restrictions.
        /// </summary>
        /// <param name="expression">The <see cref="CreateForeignKeyExpression"/> containing the foreign key definition to be truncated.</param>
        /// <remarks>
        /// This method delegates the truncation logic to the <see cref="Truncate(ForeignKeyDefinition)"/> method.
        /// </remarks>
        public void Truncate(CreateForeignKeyExpression expression)
        {
            Truncate(expression.ForeignKey);
        }
        
        /// <summary>
        /// Truncates the names in the provided <see cref="FluentMigrator.Expressions.DeleteForeignKeyExpression"/> 
        /// to ensure they comply with Firebird's maximum length restrictions.
        /// </summary>
        /// <param name="expression">
        /// The <see cref="FluentMigrator.Expressions.DeleteForeignKeyExpression"/> containing the foreign key definition to truncate.
        /// </param>
        /// <remarks>
        /// This method truncates the <see cref="FluentMigrator.Expressions.DeleteForeignKeyExpression.ForeignKey"/> property 
        /// by delegating to the corresponding truncation logic for <see cref="ForeignKeyDefinition"/>.
        /// </remarks>
        public void Truncate(DeleteForeignKeyExpression expression)
        {
            Truncate(expression.ForeignKey);
        }

        /// <summary>
        /// Truncates the table and column names in the provided <see cref="AlterDefaultConstraintExpression"/> 
        /// to ensure they comply with Firebird's maximum length restrictions.
        /// </summary>
        /// <param name="expression">
        /// The <see cref="AlterDefaultConstraintExpression"/> containing the table and column names to be truncated.
        /// </param>
        /// <remarks>
        /// This method modifies the <paramref name="expression"/> in place by truncating its 
        /// <see cref="AlterDefaultConstraintExpression.TableName"/> and <see cref="AlterDefaultConstraintExpression.ColumnName"/> properties.
        /// </remarks>
        public void Truncate(AlterDefaultConstraintExpression expression)
        {
            expression.TableName = Truncate(expression.TableName);
            expression.ColumnName = Truncate(expression.ColumnName);
        }

        /// <summary>
        /// Truncates the table and column names in the provided <see cref="DeleteDefaultConstraintExpression"/> 
        /// to ensure they comply with Firebird's maximum length restrictions.
        /// </summary>
        /// <param name="expression">
        /// The <see cref="DeleteDefaultConstraintExpression"/> containing the table and column names to be truncated.
        /// </param>
        /// <remarks>
        /// This method modifies the <paramref name="expression"/> directly by truncating its 
        /// <see cref="DeleteDefaultConstraintExpression.TableName"/> and <see cref="DeleteDefaultConstraintExpression.ColumnName"/> properties.
        /// </remarks>
        public void Truncate(DeleteDefaultConstraintExpression expression)
        {
            expression.TableName = Truncate(expression.TableName);
            expression.ColumnName = Truncate(expression.ColumnName);
        }

        /// <summary>
        /// Truncates the name of the specified <see cref="SequenceDefinition"/> to ensure it adheres to Firebird's maximum length restrictions.
        /// </summary>
        /// <param name="sequence">The sequence definition whose name is to be truncated.</param>
        /// <remarks>
        /// This method modifies the <see cref="SequenceDefinition.Name"/> property directly to ensure compliance with Firebird's naming constraints.
        /// </remarks>
        public void Truncate(SequenceDefinition sequence)
        {
            sequence.Name = Truncate(sequence.Name);
        }

        /// <summary>
        /// Truncates the object names within the provided <see cref="CreateSequenceExpression"/> 
        /// to ensure they comply with Firebird's maximum length restrictions.
        /// </summary>
        /// <param name="expression">The <see cref="CreateSequenceExpression"/> containing the sequence to be truncated.</param>
        /// <remarks>
        /// This method delegates the truncation logic to the <see cref="SequenceDefinition"/> 
        /// associated with the provided <paramref name="expression"/>.
        /// </remarks>
        public void Truncate(CreateSequenceExpression expression)
        {
            Truncate(expression.Sequence);
        }

        /// <summary>
        /// Truncates the name of the sequence in the provided <see cref="DeleteSequenceExpression"/> 
        /// to ensure it adheres to Firebird's maximum name length restrictions.
        /// </summary>
        /// <param name="expression">The <see cref="DeleteSequenceExpression"/> containing the sequence to be truncated.</param>
        /// <remarks>
        /// This method modifies the <see cref="DeleteSequenceExpression.SequenceName"/> property directly.
        /// </remarks>
        public void Truncate(DeleteSequenceExpression expression)
        {
            expression.SequenceName = Truncate(expression.SequenceName);
        }

        /// <summary>
        /// Truncates the table name and column names in the provided <see cref="InsertDataExpression"/> 
        /// to ensure they comply with Firebird's maximum length restrictions.
        /// </summary>
        /// <param name="expression">The <see cref="InsertDataExpression"/> containing the table name and rows to truncate.</param>
        /// <remarks>
        /// This method modifies the <paramref name="expression"/> in place by truncating the table name 
        /// and the keys of each row in the <see cref="InsertDataExpression.Rows"/> collection.
        /// </remarks>
        public void Truncate(InsertDataExpression expression)
        {
            expression.TableName = Truncate(expression.TableName);
            var insertions = new List<InsertionDataDefinition>();
            foreach (InsertionDataDefinition insertion in expression.Rows)
            {
                var newInsertion = new InsertionDataDefinition();
                foreach (var data in insertion)
                {
                    newInsertion.Add(new KeyValuePair<string, object>(Truncate(data.Key), data.Value));
                }
                insertions.Add(newInsertion);
            }
            expression.Rows.Clear();
            expression.Rows.AddRange(insertions);
        }

        /// <summary>
        /// Truncates the table name and column names in the provided <see cref="DeleteDataExpression"/> 
        /// to ensure they comply with Firebird's maximum length restrictions.
        /// </summary>
        /// <param name="expression">
        /// The <see cref="DeleteDataExpression"/> containing the table name and rows to be truncated.
        /// </param>
        /// <remarks>
        /// This method modifies the <paramref name="expression"/> in place by truncating the table name 
        /// and the keys of the deletion data rows.
        /// </remarks>
        public void Truncate(DeleteDataExpression expression)
        {
            expression.TableName = Truncate(expression.TableName);
            var deletions = new List<DeletionDataDefinition>();
            foreach (DeletionDataDefinition deletion in expression.Rows)
            {
                var newDeletion = new DeletionDataDefinition();
                foreach (var data in deletion)
                {
                    newDeletion.Add(new KeyValuePair<string, object>(Truncate(data.Key), data.Value));
                }
                deletions.Add(newDeletion);
            }
            expression.Rows.Clear();
            expression.Rows.AddRange(deletions);
        }

        /// <summary>
        /// Truncates the table name, column names, and other related identifiers in the provided
        /// <see cref="FluentMigrator.Expressions.UpdateDataExpression"/> to ensure they comply with Firebird's
        /// maximum length restrictions.
        /// </summary>
        /// <param name="expression">
        /// The <see cref="FluentMigrator.Expressions.UpdateDataExpression"/> containing the table name, column names,
        /// and other data to be truncated.
        /// </param>
        /// <remarks>
        /// This method modifies the <paramref name="expression"/> in place by truncating its table name,
        /// updating the column names in the <c>Set</c> and <c>Where</c> collections, and ensuring all identifiers
        /// conform to Firebird's length constraints.
        /// </remarks>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if the <paramref name="expression"/> is <c>null</c>.
        /// </exception>
        public void Truncate(UpdateDataExpression expression)
        {
            expression.TableName = Truncate(expression.TableName);
            var newSet = new List<KeyValuePair<string, object>>();
            foreach (var data in expression.Set)
            {
                newSet.Add(new KeyValuePair<string, object>(Truncate(data.Key), data.Value));
            }
            expression.Set.Clear();
            expression.Set.AddRange(newSet);
            if (!expression.IsAllRows)
            {
                var newWhere = new List<KeyValuePair<string, object>>();
                foreach (var data in expression.Where)
                {
                    newWhere.Add(new KeyValuePair<string, object>(Truncate(data.Key), data.Value));
                }
                expression.Where.Clear();
                expression.Where.AddRange(newWhere);
            }
        }

        /// <summary>
        /// Truncates a collection of Firebird object names to ensure they comply with the maximum length restrictions.
        /// </summary>
        /// <param name="names">The collection of object names to truncate.</param>
        /// <returns>A new collection containing the truncated object names.</returns>
        /// <remarks>
        /// This method iterates through the provided collection of names, truncating each name individually
        /// using the <see cref="Truncate(string)"/> method.
        /// </remarks>
        public ICollection<string> TruncateNames(ICollection<string> names)
        {
            var ret = new List<string>();
            foreach (var item in names)
            {
                ret.Add(Truncate(item));
            }
            return ret;
        }

        /// <summary>
        /// Truncates the names of the specified collection of columns to ensure they comply with Firebird's maximum length restrictions.
        /// </summary>
        /// <param name="columns">The collection of <see cref="ColumnDefinition"/> objects whose names need to be truncated.</param>
        /// <remarks>
        /// This method iterates through each column in the provided collection and applies the truncation logic to their names.
        /// </remarks>
        public void TruncateColumns(ICollection<ColumnDefinition> columns)
        {
            foreach (ColumnDefinition colDef in columns)
            {
                Truncate(colDef);
            }
        }

        /// <summary>
        /// Truncates the specified name to ensure it complies with the maximum length restrictions
        /// defined by Firebird. If truncation is disabled and the name exceeds the maximum length,
        /// an <see cref="ArgumentException"/> is thrown.
        /// </summary>
        /// <param name="name">The name to be truncated.</param>
        /// <returns>
        /// The truncated name if it exceeds the maximum length and truncation is enabled; 
        /// otherwise, the original name.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when the name exceeds the maximum length and truncation is disabled.
        /// </exception>
        public string Truncate(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                if (name.Length <= FirebirdOptions.MaxNameLength)
                {
                    return name;
                }

                if (!_enabled)
                {
                    throw new ArgumentException($"Name too long: {name}");
                }

                return name.Substring(0, Math.Min(FirebirdOptions.MaxNameLength, name.Length));
            }
            return name;
        }

        /// <summary>
        /// Packs the specified name by generating a shortened, hashed version of it
        /// to ensure compliance with Firebird's maximum name length restrictions.
        /// </summary>
        /// <param name="name">The original name to be packed.</param>
        /// <returns>
        /// A packed version of the name if its length exceeds the maximum allowed length;
        /// otherwise, the original name is returned.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when the name exceeds the maximum allowed length and packing is disabled.
        /// </exception>
        public string Pack(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                if (name.Length > FirebirdOptions.MaxNameLength)
                {
                    if (!_enabled)
                        throw new ArgumentException($"Name too long: {name}");

                    var byteHash = MD5.Create().ComputeHash(Encoding.ASCII.GetBytes(name));
                    var hash = Convert.ToBase64String(byteHash);
                    var sb = new StringBuilder(hash.Length);
                    var hLength = hash.Length;
                    for (var i = 0; i < hLength; i++)
                    {
                        var c = hash[i];
                        if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9'))
                            sb.Append(c);
                    }
                    hash = sb.ToString();
                    return $"fk_{hash.Substring(0, Math.Min(28, hash.Length))}";
                }
            }
            return name;

        }
    }
}
