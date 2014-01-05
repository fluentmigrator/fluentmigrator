﻿using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Model;
using System;
using System.Collections.Generic;

namespace FluentMigrator.Builders
{
    /// <summary>
    /// This class provides a common location for logic pertaining to setting and maintaining 
    /// expressions for expression builders which manipulate the the ColumnDefinition.
    /// </summary>
    /// <remarks>
    /// This is a support class for the migrator framework and is not intended for external use.
    /// TODO: make this internal, and the change assmebly info so InternalsVisibleTo is set for the test assemblies.
    /// </remarks>
    public class ColumnExpressionBuilderHelper
    {
        /// <summary>
        /// For each distinct column which has an existing row default, an instance of this
        /// will be stored in the _expressionsByColumn.
        /// </summary>
        private class ExistingRowsData
        {
            public UpdateDataExpression SetExistingRowsExpression;
            public AlterColumnExpression SetColumnNotNullableExpression;
        }

        private IColumnExpressionBuilder _builder;
        private IMigrationContext _context;
        private Dictionary<ColumnDefinition, ExistingRowsData> _existingRowsDataByColumn { get; set; }

        /// <summary>
        /// For easy mockability only.
        /// </summary>
        protected ColumnExpressionBuilderHelper() { }

        public ColumnExpressionBuilderHelper(IColumnExpressionBuilder builder, IMigrationContext context)
        {
            _builder = builder;
            _context = context;
            _existingRowsDataByColumn = new Dictionary<ColumnDefinition, ExistingRowsData>();
        }

        /// <summary>
        /// Either updates the IsNullable flag on the column, or creates/removes the SetNotNull expression, depending
        /// on whether the column has a 'Set existing rows' expression.
        /// </summary>
        public virtual void SetNullable(bool isNullable)
        {
            var column = _builder.Column;
            ExistingRowsData exRowExpr;
            if (_existingRowsDataByColumn.TryGetValue(column, out exRowExpr))
            {
                if (exRowExpr.SetExistingRowsExpression != null)
                {
                    if (isNullable)
                    {
                        //Remove additional expression to set column to not null.
                        _context.Expressions.Remove(exRowExpr.SetColumnNotNullableExpression);
                        exRowExpr.SetColumnNotNullableExpression = null;
                    }
                    else
                    {
                        //Add expression to set column to not null.
                        //If it already exists, just leave it.
                        if (exRowExpr.SetColumnNotNullableExpression == null)
                        {
                            //stuff that matters shouldn't change at this point, so we're free to make a
                            //copy of the col def.
                            //TODO: make a SetColumnNotNullExpression, which just takes the bare minimum.
                            ColumnDefinition notNullColDef = new ColumnDefinition
                            {
                                ModificationType = ColumnModificationType.Alter,
                                TableName = column.TableName,
                                Name = column.Name,
                                Type = column.Type,
                                CustomType = column.CustomType,
                                IsNullable = false,
                            };

                            exRowExpr.SetColumnNotNullableExpression = new AlterColumnExpression
                            {
                                Column = notNullColDef,
                                TableName = _builder.TableName,
                                SchemaName = _builder.SchemaName
                            };

                            _context.Expressions.Add(exRowExpr.SetColumnNotNullableExpression);
                        }
                    }

                    //Setting column explicitly to nullable, as the actual nullable value
                    //will be set by the SetColumnNotNullableExpression after the column is created and populated.
                    column.IsNullable = true;
                    return;
                }
            }

            //At this point, we know there's no existing row expression, so just pass it onto the 
            //underlying column.
            column.IsNullable = isNullable;
        }

        /// <summary>
        /// Adds the existing row default value.  If the column has a value for IsNullable, this will also
        /// call SetNullable to create the expression, and will then set the column IsNullable to false.
        /// </summary>
        public virtual void SetExistingRowsTo(object existingRowValue)
        {
            //TODO: validate that 'value' isn't set to null for non nullable columns.  If set to
            //null, maybe just remove the expressions?.. not sure of best way to handle this.

            var column = _builder.Column;
            if (column.ModificationType == ColumnModificationType.Create)
            {
                //ensure an UpdateDataExpression is created and cached for this column

                ExistingRowsData exRowExpr;
                if (!_existingRowsDataByColumn.TryGetValue(column, out exRowExpr))
                {
                    exRowExpr = new ExistingRowsData();
                    _existingRowsDataByColumn.Add(column, exRowExpr);
                }

                if (exRowExpr.SetExistingRowsExpression == null)
                {
                    exRowExpr.SetExistingRowsExpression = new UpdateDataExpression
                    {
                        TableName = _builder.TableName,
                        SchemaName = _builder.SchemaName,
                        IsAllRows = true,
                    };
                    _context.Expressions.Add(exRowExpr.SetExistingRowsExpression);

                    //Call SetNullable, to ensure that not-null columns are correctly set to 
                    //not null after existing rows have data populated.
                    SetNullable(column.IsNullable ?? true);
                }

                exRowExpr.SetExistingRowsExpression.Set = new List<KeyValuePair<string, object>>  {
                   new KeyValuePair<string, object>(column.Name, existingRowValue)
                };
            }
        }

        public virtual void Unique(string indexName)
        {
            var column = _builder.Column;
            column.IsUnique = true;

            var index = new CreateIndexExpression
            {
                Index = new IndexDefinition
                {
                    Name = indexName,
                    SchemaName = _builder.SchemaName,
                    TableName = _builder.TableName,
                    IsUnique = true
                }
            };

            index.Index.Columns.Add(new IndexColumnDefinition
            {
                Name = _builder.Column.Name
            });

            _context.Expressions.Add(index);
        }

        public virtual void Indexed(string indexName)
        {
            _builder.Column.IsIndexed = true;

            var index = new CreateIndexExpression
            {
                Index = new IndexDefinition
                {
                    Name = indexName,
                    SchemaName = _builder.SchemaName,
                    TableName = _builder.TableName
                }
            };

            index.Index.Columns.Add(new IndexColumnDefinition
            {
                Name = _builder.Column.Name
            });

            _context.Expressions.Add(index);
        }

        public virtual ForeignKeyDefinition ForeignKey(string foreignKeyName, string primaryTableSchema, 
            string primaryTableName, string primaryColumnName)
        {
            _builder.Column.IsForeignKey = true;

            var fk = new CreateForeignKeyExpression
            {
                ForeignKey = new ForeignKeyDefinition
                {
                    Name = foreignKeyName,
                    PrimaryTable = primaryTableName,
                    PrimaryTableSchema = primaryTableSchema,
                    ForeignTable = _builder.TableName,
                    ForeignTableSchema = _builder.SchemaName
                }
            };

            fk.ForeignKey.PrimaryColumns.Add(primaryColumnName);
            fk.ForeignKey.ForeignColumns.Add(_builder.Column.Name);

            _context.Expressions.Add(fk);
            return fk.ForeignKey;  
        }

        //Can this be merged with ForeignKey()?  essentially they're just doing the same thing in reverse to each other.
        public virtual ForeignKeyDefinition ReferencedBy(string foreignKeyName, string foreignTableSchema, 
            string foreignTableName, string foreignColumnName)
        {
            var fk = new CreateForeignKeyExpression
            {
                ForeignKey = new ForeignKeyDefinition
                {
                    Name = foreignKeyName,
                    PrimaryTable = _builder.TableName,
                    PrimaryTableSchema = _builder.SchemaName,
                    ForeignTable = foreignTableName,
                    ForeignTableSchema = foreignTableSchema
                }
            };

            fk.ForeignKey.PrimaryColumns.Add(_builder.Column.Name);
            fk.ForeignKey.ForeignColumns.Add(foreignColumnName);

            _context.Expressions.Add(fk);
            return fk.ForeignKey;
        }

        /// <summary>
        /// Supports the Obsolete 'References' fluent command.  Remove this when that command is removed.
        /// </summary>
        public virtual void References(string foreignKeyName, string foreignTableSchema, string foreignTableName,
                                                   IEnumerable<string> foreignColumnNames)
        {
            var fk = new CreateForeignKeyExpression
            {
                ForeignKey = new ForeignKeyDefinition
                {
                    Name = foreignKeyName,
                    PrimaryTable = _builder.TableName,
                    PrimaryTableSchema = _builder.SchemaName,
                    ForeignTable = foreignTableName,
                    ForeignTableSchema = foreignTableSchema
                }
            };

            fk.ForeignKey.PrimaryColumns.Add(_builder.Column.Name);
            foreach (var foreignColumnName in foreignColumnNames)
                fk.ForeignKey.ForeignColumns.Add(foreignColumnName);

            _context.Expressions.Add(fk);
        }
    }
}
