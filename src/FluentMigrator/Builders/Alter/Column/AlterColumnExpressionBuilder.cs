using System;
using System.Collections.Generic;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Model;

namespace FluentMigrator.Builders.Alter.Column
{
    public class AlterColumnExpressionBuilder : ExpressionBuilderWithColumnTypesBase<AlterColumnExpression, IAlterColumnOptionSyntax>,
                                                IAlterColumnOnTableSyntax,
                                                IAlterColumnOptionSyntax,
                                                IAlterColumnAsTypeOrInSchemaSyntax
    {
        private readonly IMigrationContext _context;

        public AlterColumnExpressionBuilder(AlterColumnExpression expression, IMigrationContext context)
            : base(expression)
        {
            _context = context;
        }

        public IAlterColumnAsTypeOrInSchemaSyntax OnTable(string name)
        {
            Expression.TableName = name;
            return this;
        }

        public IAlterColumnAsTypeSyntax InSchema(string schemaName)
        {
            Expression.SchemaName = schemaName;
            return this;
        }

        public IAlterColumnOptionSyntax WithDefault(SystemMethods method)
        {
            // we need to do a drop constraint and then add constraint to change the defualt value
            var dc = new AlterDefaultConstraintExpression
            {
                TableName = Expression.TableName,
                SchemaName = Expression.SchemaName,
                ColumnName = Expression.Column.Name,
                DefaultValue = method
            };

            _context.Expressions.Add(dc);

            Expression.Column.DefaultValue = method;

            return this;
        }

        public IAlterColumnOptionSyntax WithDefaultValue(object value)
        {
            // we need to do a drop constraint and then add constraint to change the defualt value
            var dc = new AlterDefaultConstraintExpression
                         {
                             TableName = Expression.TableName,
                             SchemaName = Expression.SchemaName,
                             ColumnName = Expression.Column.Name,
                             DefaultValue = value
                         };

            _context.Expressions.Add(dc);

            Expression.Column.DefaultValue = value;

            return this;
        }

        public IAlterColumnOptionSyntax Identity()
        {
            Expression.Column.IsIdentity = true;
            return this;
        }

        public IAlterColumnOptionSyntax Indexed()
        {
            return Indexed(null);
        }

        public IAlterColumnOptionSyntax Indexed(string indexName)
        {
            Expression.Column.IsIndexed = true;

            var index = new CreateIndexExpression
                            {
                                Index = new IndexDefinition
                                            {
                                                Name = indexName,
                                                SchemaName = Expression.SchemaName,
                                                TableName = Expression.TableName
                                            }
                            };

            index.Index.Columns.Add(new IndexColumnDefinition
                                        {
                                            Name = Expression.Column.Name
                                        });

            _context.Expressions.Add(index);

            return this;
        }

        public IAlterColumnOptionSyntax PrimaryKey()
        {
            Expression.Column.IsPrimaryKey = true;
            return this;
        }

        public IAlterColumnOptionSyntax PrimaryKey(string primaryKeyName)
        {
            Expression.Column.IsPrimaryKey = true;
            Expression.Column.PrimaryKeyName = primaryKeyName;
            return this;
        }

        public IAlterColumnOptionSyntax Nullable()
        {
            Expression.Column.IsNullable = true;
            return this;
        }

        public IAlterColumnOptionSyntax NotNullable()
        {
            Expression.Column.IsNullable = false;
            return this;
        }

        public IAlterColumnOptionSyntax Unique()
        {
            return Unique(null);
        }

        public IAlterColumnOptionSyntax Unique(string indexName)
        {
            Expression.Column.IsUnique = true;

            var index = new CreateIndexExpression
                            {
                                Index = new IndexDefinition
                                            {
                                                Name = indexName,
                                                SchemaName = Expression.SchemaName,
                                                TableName = Expression.TableName,
                                                IsUnique = true
                                            }
                            };

            index.Index.Columns.Add(new IndexColumnDefinition
                                        {
                                            Name = Expression.Column.Name
                                        });

            _context.Expressions.Add(index);

            return this;
        }

        public IAlterColumnOptionSyntax ForeignKey(string primaryTableName, string primaryColumnName)
        {
            return ForeignKey(null, null, primaryTableName, primaryColumnName);
        }

        public IAlterColumnOptionSyntax ForeignKey(string foreignKeyName, string primaryTableName, string primaryColumnName)
        {
            return ForeignKey(foreignKeyName, null, primaryTableName, primaryColumnName);
        }

        public IAlterColumnOptionSyntax ForeignKey(string foreignKeyName, string primaryTableSchema, string primaryTableName, string primaryColumnName)
        {
            Expression.Column.IsForeignKey = true;

            var fk = new CreateForeignKeyExpression
                         {
                             ForeignKey = new ForeignKeyDefinition
                                              {
                                                  Name = foreignKeyName,
                                                  PrimaryTable = primaryTableName,
                                                  PrimaryTableSchema = primaryTableSchema,
                                                  ForeignTable = Expression.TableName,
                                                  ForeignTableSchema = Expression.SchemaName
                                              }
                         };

            fk.ForeignKey.PrimaryColumns.Add(primaryColumnName);
            fk.ForeignKey.ForeignColumns.Add(Expression.Column.Name);

            _context.Expressions.Add(fk);
            return this;
        }

        public IAlterColumnOptionSyntax ReferencedBy(string foreignTableName, string foreignColumnName)
        {
            return ReferencedBy(null, null, foreignTableName, foreignColumnName);
        }

        public IAlterColumnOptionSyntax ReferencedBy(string foreignKeyName, string foreignTableName, string foreignColumnName)
        {
            return ReferencedBy(foreignKeyName, null, foreignTableName, foreignColumnName);
        }

        public IAlterColumnOptionSyntax ReferencedBy(string foreignKeyName, string foreignTableSchema, string foreignTableName, string foreignColumnName)
        {
            var fk = new CreateForeignKeyExpression
                         {
                             ForeignKey = new ForeignKeyDefinition
                                              {
                                                  Name = foreignKeyName,
                                                  PrimaryTable = Expression.TableName,
                                                  PrimaryTableSchema = Expression.SchemaName,
                                                  ForeignTable = foreignTableName,
                                                  ForeignTableSchema = foreignTableSchema
                                              }
                         };

            fk.ForeignKey.PrimaryColumns.Add(Expression.Column.Name);
            fk.ForeignKey.ForeignColumns.Add(foreignColumnName);

            _context.Expressions.Add(fk);
            return this;
        }

        [Obsolete("Use ForeignKey(string,string)")]
        public IAlterColumnOptionSyntax ForeignKey()
        {
            Expression.Column.IsForeignKey = true;
            return this;
        }

        [Obsolete("Use ReferencedBy")]
        public IAlterColumnOptionSyntax References(string foreignKeyName, string foreignTableName, IEnumerable<string> foreignColumnNames)
        {
            return References(foreignKeyName, null, foreignTableName, foreignColumnNames);
        }

        [Obsolete("Use ReferencedBy")]
        public IAlterColumnOptionSyntax References(string foreignKeyName, string foreignTableSchema, string foreignTableName,
                                                   IEnumerable<string> foreignColumnNames)
        {
            var fk = new CreateForeignKeyExpression
                         {
                             ForeignKey = new ForeignKeyDefinition
                                              {
                                                  Name = foreignKeyName,
                                                  PrimaryTable = Expression.TableName,
                                                  PrimaryTableSchema = Expression.SchemaName,
                                                  ForeignTable = foreignTableName,
                                                  ForeignTableSchema = foreignTableSchema
                                              }
                         };

            fk.ForeignKey.PrimaryColumns.Add(Expression.Column.Name);
            foreach (var foreignColumnName in foreignColumnNames)
                fk.ForeignKey.ForeignColumns.Add(foreignColumnName);

            _context.Expressions.Add(fk);
            return this;
        }

        protected override ColumnDefinition GetColumnForType()
        {
            return Expression.Column;
        }
    }
}