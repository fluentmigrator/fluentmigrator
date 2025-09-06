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
using System.Data;

using FluentMigrator.Builders.Create.Column;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Infrastructure.Extensions;
using FluentMigrator.Model;
using FluentMigrator.Runner;
using FluentMigrator.SqlServer;

using Microsoft.Extensions.DependencyInjection;

using Moq;

namespace FluentMigrator.Tests.Unit.Generators
{
    public static class GeneratorTestHelper
    {
        public static string TestTableName1 = "TestTable1";
        public static string TestTableName2 = "TestTable2";
        public static string TestColumnName1 = "TestColumn1";
        public static string TestColumnName2 = "TestColumn2";
        public static string TestColumnName3 = "TestColumn3";
        public static string TestIndexName = "TestIndex";
        public static string TestTableDescription = "TestDescription";
        public static string TestColumn1Description = "TestColumn1Description";
        public static string TestColumn2Description = "TestColumn2Description";
        public static string TestColumnCollationName = "Latin1_General_CS_AS";
        public static Dictionary<string, string> TestAdditionalColumnDescriptions1 = new Dictionary<string, string>()
        {
            {"AdditionalColumnDescriptionKey1", "AdditionalColumnDescriptionValue1" }
        };
        public static Dictionary<string, string> TestAdditionalColumnDescriptions2 = new Dictionary<string, string>()
        {
            {"AdditionalColumnDescriptionKey2", "AdditionalColumnDescriptionValue2" }
        };

        public static Guid TestGuid = Guid.NewGuid();

        public static CreateTableExpression GetCreateTableExpression()
        {
            CreateTableExpression expression = new CreateTableExpression() { TableName = TestTableName1, };
            expression.Columns.Add(new ColumnDefinition { Name = TestColumnName1, Type = DbType.String });
            expression.Columns.Add(new ColumnDefinition { Name = TestColumnName2, Type = DbType.Int32 });
            return expression;
        }

        public static CreateColumnExpression GetCreateBooleanColumnExpression()
        {
            ColumnDefinition column = new ColumnDefinition { Name = TestColumnName1, Type = DbType.Boolean, };
            return new CreateColumnExpression { TableName = TestTableName1, Column = column };
        }

        public static CreateTableExpression GetCreateTableWithDefaultValue()
        {
            CreateTableExpression expression = new CreateTableExpression() { TableName = TestTableName1, };
            expression.Columns.Add(new ColumnDefinition { Name = TestColumnName1, Type = DbType.String, DefaultValue = "Default", TableName = TestTableName1 });
            expression.Columns.Add(new ColumnDefinition { Name = TestColumnName2, Type = DbType.Int32, DefaultValue = 0, TableName = TestTableName1 });
            return expression;
        }

        public static CreateTableExpression GetCreateTableWithPrimaryKeyExpression()
        {
            var expression = new CreateTableExpression { TableName = TestTableName1 };
            expression.Columns.Add(new ColumnDefinition { Name = TestColumnName1, IsPrimaryKey = true, Type = DbType.String });
            expression.Columns.Add(new ColumnDefinition { Name = TestColumnName2, Type = DbType.Int32 });
            return expression;
        }

        public static CreateTableExpression GetCreateTableWithNamedPrimaryKeyExpression()
        {
            var expression = new CreateTableExpression { TableName = TestTableName1 };
            expression.Columns.Add(new ColumnDefinition { Name = TestColumnName1, IsPrimaryKey = true, PrimaryKeyName = "TestKey", Type = DbType.String });
            expression.Columns.Add(new ColumnDefinition { Name = TestColumnName2, Type = DbType.Int32 });
            return expression;
        }

        public static CreateTableExpression GetCreateTableWithNamedMultiColumnPrimaryKeyExpression()
        {
            var expression = new CreateTableExpression { TableName = TestTableName1 };
            expression.Columns.Add(new ColumnDefinition { Name = TestColumnName1, IsPrimaryKey = true, PrimaryKeyName = "TestKey", Type = DbType.String });
            expression.Columns.Add(new ColumnDefinition { Name = TestColumnName2, Type = DbType.Int32, IsPrimaryKey = true });
            return expression;
        }

        public static CreateTableExpression GetCreateTableWithAutoIncrementExpression()
        {
            var expression = new CreateTableExpression { TableName = TestTableName1 };
            expression.Columns.Add(new ColumnDefinition { Name = TestColumnName1, IsIdentity = true, Type = DbType.Int32 });
            expression.Columns.Add(new ColumnDefinition { Name = TestColumnName2, Type = DbType.Int32 });
            return expression;
        }

        public static CreateTableExpression GetCreateTableWithMultiColumnPrimaryKeyExpression()
        {
            var expression = new CreateTableExpression { TableName = TestTableName1 };
            expression.Columns.Add(new ColumnDefinition { Name = TestColumnName1, IsPrimaryKey = true, Type = DbType.String });
            expression.Columns.Add(new ColumnDefinition { Name = TestColumnName2, IsPrimaryKey = true, Type = DbType.Int32 });
            return expression;

        }

        public static CreateTableExpression GetCreateTableWithNullableColumn()
        {
            var expression = new CreateTableExpression { TableName = TestTableName1 };
            expression.Columns.Add(new ColumnDefinition { Name = TestColumnName1, IsNullable = true, Type = DbType.String });
            expression.Columns.Add(new ColumnDefinition { Name = TestColumnName2, Type = DbType.Int32 });
            return expression;
        }

        public static CreateTableExpression GetCreateTableWithTableDescription()
        {
            var expression = new CreateTableExpression { TableName = TestTableName1, TableDescription = TestTableDescription };

            return expression;
        }

        public static CreateTableExpression GetCreateTableWithTableDescriptionAndColumnDescriptions()
        {
            var expression = new CreateTableExpression { TableName = TestTableName1, TableDescription = TestTableDescription };
            expression.Columns.Add(new ColumnDefinition
            {
                Name = TestColumnName1,
                IsNullable = true,
                Type = DbType.String,
                ColumnDescription = TestColumn1Description
            });
            expression.Columns.Add(new ColumnDefinition
            {
                Name = TestColumnName2,
                Type = DbType.Int32,
                ColumnDescription = TestColumn2Description
            });

            return expression;
        }

        public static CreateTableExpression GetCreateTableWithTableDescriptionAndColumnDescriptionsAndAdditionalDescriptions()
        {
            var expression = new CreateTableExpression { TableName = TestTableName1, TableDescription = TestTableDescription };
            expression.Columns.Add(new ColumnDefinition
            {
                Name = TestColumnName1,
                IsNullable = true,
                Type = DbType.String,
                ColumnDescription = TestColumn1Description,
                AdditionalColumnDescriptions = TestAdditionalColumnDescriptions1
            });
            expression.Columns.Add(new ColumnDefinition
            {
                Name = TestColumnName2,
                Type = DbType.Int32,
                ColumnDescription = TestColumn2Description,
                AdditionalColumnDescriptions = TestAdditionalColumnDescriptions2
            });

            return expression;
        }

        public static CreateTableExpression GetCreateTableWithForeignKey()
        {
            var expression = new CreateTableExpression { TableName = TestTableName1 };
            expression.Columns.Add(new ColumnDefinition { Name = TestColumnName1, Type = DbType.String });
            expression.Columns.Add(new ColumnDefinition
            {
                Name = TestColumnName2,
                Type = DbType.Int32,
                IsForeignKey = true,
                ForeignKey = new ForeignKeyDefinition()
                {
                    PrimaryTable = TestTableName2,
                    ForeignTable = TestTableName1,
                    PrimaryColumns = new[] { TestColumnName2 },
                    ForeignColumns = new[] { TestColumnName1 }
                }
            });
            return expression;
        }

        public static CreateTableExpression GetCreateTableWithMultiColumnForeignKey()
        {
            var expression = new CreateTableExpression { TableName = TestTableName1 };
            expression.Columns.Add(new ColumnDefinition { Name = TestColumnName1, Type = DbType.String });
            expression.Columns.Add(new ColumnDefinition
            {
                Name = TestColumnName2,
                Type = DbType.Int32,
                IsForeignKey = true,
                ForeignKey = new ForeignKeyDefinition()
                {
                    PrimaryTable = TestTableName2,
                    ForeignTable = TestTableName1,
                    PrimaryColumns = new[] { TestColumnName2, "TestColumn4" },
                    ForeignColumns = new[] { TestColumnName1, "TestColumn3" }
                }
            });
            return expression;
        }

        public static CreateTableExpression GetCreateTableWithNameForeignKey()
        {
            var expression = new CreateTableExpression { TableName = TestTableName1 };
            expression.Columns.Add(new ColumnDefinition { Name = TestColumnName1, Type = DbType.String });
            expression.Columns.Add(new ColumnDefinition
            {
                Name = TestColumnName2,
                Type = DbType.Int32,
                IsForeignKey = true,
                ForeignKey = new ForeignKeyDefinition()
                {
                    Name = "FK_Test",
                    PrimaryTable = TestTableName2,
                    ForeignTable = TestTableName1,
                    PrimaryColumns = new[] { TestColumnName2 },
                    ForeignColumns = new[] { TestColumnName1 }
                }
            });
            return expression;
        }

        public static CreateTableExpression GetCreateTableWithNameMultiColumnForeignKey()
        {
            var expression = new CreateTableExpression { TableName = TestTableName1 };
            expression.Columns.Add(new ColumnDefinition { Name = TestColumnName1, Type = DbType.String });
            expression.Columns.Add(new ColumnDefinition
            {
                Name = TestColumnName2,
                Type = DbType.Int32,
                IsForeignKey = true,
                ForeignKey = new ForeignKeyDefinition()
                {
                    Name = "FK_Test",
                    PrimaryTable = TestTableName2,
                    ForeignTable = TestTableName1,
                    PrimaryColumns = new[] { TestColumnName2, "TestColumn4" },
                    ForeignColumns = new[] { TestColumnName1, "TestColumn3" }
                }
            });
            return expression;
        }

        public static CreateTableExpression GetCreateTableWithFluentMultiColumnForeignKey()
        {
            var expression = new CreateTableExpression { TableName = TestTableName1 };
            expression.Columns.Add(new ColumnDefinition { Name = TestColumnName1, Type = DbType.String });
            expression.Columns.Add(new ColumnDefinition { Name = TestColumnName2, Type = DbType.Int32 });
            expression.Columns.Add(new ColumnDefinition { Name = "TestColumn3", Type = DbType.String });
            
            // This will be created using the fluent syntax with the new multi-column ForeignKey method
            // The foreign key will be defined as: ForeignKey(new[] { TestColumnName1, TestColumnName2 }, TestTableName2, new[] { TestColumnName2, "TestColumn4" })
            return expression;
        }

        public static CreateIndexExpression GetCreateIndexExpression()
        {
            var expression = new CreateIndexExpression();
            expression.Index.Name = TestIndexName;
            expression.Index.TableName = TestTableName1;
            expression.Index.IsUnique = false;
            expression.Index.Columns.Add(new IndexColumnDefinition { Direction = Direction.Ascending, Name = TestColumnName1 });
            return expression;
        }

        public static CreateSchemaExpression GetCreateSchemaExpression()
        {
            return new CreateSchemaExpression { SchemaName = "TestSchema" };
        }

        public static CreateSequenceExpression GetCreateSequenceExpression()
        {
            return new CreateSequenceExpression
            {
                Sequence =
                {
                    Cache = 10,
                    Cycle = true,
                    Increment = 2,
                    MaxValue = 100,
                    MinValue = 0,
                    Name = "Sequence",
                    StartWith = 2
                }
            };
        }

        public static CreateIndexExpression GetCreateMultiColumnCreateIndexExpression()
        {

            var expression = new CreateIndexExpression();
            expression.Index.Name = TestIndexName;
            expression.Index.TableName = TestTableName1;
            expression.Index.IsUnique = false;
            expression.Index.Columns.Add(new IndexColumnDefinition { Direction = Direction.Ascending, Name = TestColumnName1 });
            expression.Index.Columns.Add(new IndexColumnDefinition { Direction = Direction.Descending, Name = TestColumnName2 });
            return expression;
        }

        public static CreateIndexExpression GetCreateUniqueIndexExpression()
        {
            var expression = new CreateIndexExpression();
            expression.Index.Name = TestIndexName;
            expression.Index.TableName = TestTableName1;
            expression.Index.IsUnique = true;
            expression.Index.Columns.Add(new IndexColumnDefinition { Direction = Direction.Ascending, Name = TestColumnName1 });
            return expression;
        }

        public static CreateIndexExpression GetCreateUniqueMultiColumnIndexExpression()
        {
            var expression = new CreateIndexExpression();
            expression.Index.Name = TestIndexName;
            expression.Index.TableName = TestTableName1;
            expression.Index.IsUnique = true;
            expression.Index.Columns.Add(new IndexColumnDefinition { Direction = Direction.Ascending, Name = TestColumnName1 });
            expression.Index.Columns.Add(new IndexColumnDefinition { Direction = Direction.Descending, Name = TestColumnName2 });
            return expression;
        }

        public static CreateIndexExpression GetCreateIncludeIndexExpression()
        {
            var expression = new CreateIndexExpression();
            expression.Index.Name = TestIndexName;
            expression.Index.TableName = TestTableName1;
            expression.Index.IsUnique = false;
            expression.Index.Columns.Add(new IndexColumnDefinition { Direction = Direction.Ascending, Name = TestColumnName1 });
            var includes = expression.Index.GetAdditionalFeature(SqlServerExtensions.IncludesList, () => new List<IndexIncludeDefinition>());
            includes.Add(new IndexIncludeDefinition { Name = TestColumnName2 });
            return expression;
        }

        public static CreateIndexExpression GetCreateMultiIncludeIndexExpression()
        {
            var expression = new CreateIndexExpression();
            expression.Index.Name = TestIndexName;
            expression.Index.TableName = TestTableName1;
            expression.Index.IsUnique = false;
            expression.Index.Columns.Add(new IndexColumnDefinition { Direction = Direction.Ascending, Name = TestColumnName1 });

            var includes = expression.Index.GetAdditionalFeature(SqlServerExtensions.IncludesList, () => new List<IndexIncludeDefinition>());
            includes.Add(new IndexIncludeDefinition { Name = TestColumnName2 });
            includes.Add(new IndexIncludeDefinition { Name = TestColumnName3 });
            return expression;
        }

        public static InsertDataExpression GetInsertDataExpression()
        {
            var expression = new InsertDataExpression();
            expression.TableName = TestTableName1;
            expression.Rows.Add(new InsertionDataDefinition
                                    {
                                        new KeyValuePair<string, object>("Id", 1),
                                        new KeyValuePair<string, object>("Name", "Just'in"),
                                        new KeyValuePair<string, object>("Website", "codethinked.com")
                                    });
            expression.Rows.Add(new InsertionDataDefinition
                                    {
                                        new KeyValuePair<string, object>("Id", 2),
                                        new KeyValuePair<string, object>("Name", @"Na\te"),
                                        new KeyValuePair<string, object>("Website", "kohari.org")
                                    });

            return expression;
        }

        public static UpdateDataExpression GetUpdateDataExpression()
        {
            var expression = new UpdateDataExpression();
            expression.TableName = TestTableName1;

            expression.Set = new List<KeyValuePair<string, object>>
                                 {
                                     new KeyValuePair<string, object>("Name", "Just'in"),
                                     new KeyValuePair<string, object>("Age", 25)
                                 };

            expression.Where = new List<KeyValuePair<string, object>>
                                   {
                                       new KeyValuePair<string, object>("Id", 9),
                                       new KeyValuePair<string, object>("Homepage", null)
                                   };
            return expression;
        }

        public static UpdateDataExpression GetUpdateDataExpressionWithDbNullValue()
        {
            var expression = new UpdateDataExpression();
            expression.TableName = TestTableName1;

            expression.Set = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("Name", "Just'in"),
                new KeyValuePair<string, object>("Age", 25)
            };

            expression.Where = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("Id", 9),
                new KeyValuePair<string, object>("Homepage", DBNull.Value)
            };
            return expression;
        }

        public static UpdateDataExpression GetUpdateDataExpressionWithAllRows()
        {
            var expression = new UpdateDataExpression();
            expression.TableName = TestTableName1;

            expression.Set = new List<KeyValuePair<string, object>>
                                 {
                                     new KeyValuePair<string, object>("Name", "Just'in"),
                                     new KeyValuePair<string, object>("Age", 25)
                                 };

            expression.IsAllRows = true;

            return expression;
        }

        public static InsertDataExpression GetInsertGUIDExpression()
        {
            return GetInsertGUIDExpression(TestGuid);
        }

        public static InsertDataExpression GetInsertGUIDExpression(Guid guid)
        {
            var expression = new InsertDataExpression { TableName = TestTableName1 };
            expression.Rows.Add(new InsertionDataDefinition { new KeyValuePair<string, object>("guid", guid) });

            return expression;
        }

        public static DeleteDataExpression GetDeleteDataExpression()
        {
            var expression = new DeleteDataExpression();
            expression.TableName = TestTableName1;
            expression.Rows.Add(new DeletionDataDefinition
                                    {
                                        new KeyValuePair<string, object>("Name", "Just'in"),
                                        new KeyValuePair<string, object>("Website", null)
                                    });

            return expression;
        }

        public static DeleteDataExpression GetDeleteDataExpressionWithDbNullValue()
        {
            var expression = new DeleteDataExpression();
            expression.TableName = TestTableName1;
            expression.Rows.Add(new DeletionDataDefinition
            {
                new KeyValuePair<string, object>("Name", "Just'in"),
                new KeyValuePair<string, object>("Website", DBNull.Value)
            });

            return expression;
        }

        public static DeleteDataExpression GetDeleteDataMultipleRowsExpression()
        {
            var expression = new DeleteDataExpression();
            expression.TableName = TestTableName1;
            expression.Rows.Add(new DeletionDataDefinition
                                    {
                                        new KeyValuePair<string, object>("Name", "Just'in"),
                                        new KeyValuePair<string, object>("Website", null)
                                    });
            expression.Rows.Add(new DeletionDataDefinition
                                    {
                                        new KeyValuePair<string, object>("Website", "github.com")
                                    });

            return expression;
        }

        public static DeleteDataExpression GetDeleteDataAllRowsExpression()
        {
            var expression = new DeleteDataExpression();
            expression.TableName = TestTableName1;
            expression.IsAllRows = true;
            return expression;
        }

        public static RenameColumnExpression GetRenameColumnExpression()
        {
            return new RenameColumnExpression { OldName = TestColumnName1, NewName = TestColumnName2, TableName = TestTableName1 };
        }

        public static CreateColumnExpression GetCreateDecimalColumnExpression()
        {
            ColumnDefinition column = new ColumnDefinition { Name = TestColumnName1, Type = DbType.Decimal, Size = 19, Precision = 2 };
            return new CreateColumnExpression { TableName = TestTableName1, Column = column };
        }

        public static CreateColumnExpression GetCreateCurrencyColumnExpression()
        {
            ColumnDefinition column = new ColumnDefinition { Name = TestColumnName1, Type = DbType.Currency };
            return new CreateColumnExpression { TableName = TestTableName1, Column = column };
        }

        public static CreateColumnExpression GetCreateColumnExpression()
        {
            ColumnDefinition column = new ColumnDefinition { Name = TestColumnName1, Type = DbType.String, Size = 5 };
            return new CreateColumnExpression { TableName = TestTableName1, Column = column };
        }

        public static ICollection<IMigrationExpression> GetCreateColumnWithSystemMethodExpression(string schemaName = null)
        {
            var serviceProvider = new ServiceCollection().BuildServiceProvider();
            var querySchema = new Mock<IQuerySchema>();
            var context = new MigrationContext(querySchema.Object, serviceProvider, null);
            var expr = new CreateColumnExpression
            {
                TableName = TestTableName1,
                SchemaName = schemaName,
                Column = new ColumnDefinition { Name = TestColumnName1, Type = DbType.DateTime }
            };
            context.Expressions.Add(expr);
            var builder = new CreateColumnExpressionBuilder(expr, context);
            builder.SetExistingRowsTo(SystemMethods.CurrentDateTime);
            return context.Expressions;
        }

        public static CreateColumnExpression GetCreateColumnExpressionWithDescription()
        {
            CreateColumnExpression columnExpression = GetCreateColumnExpression();
            columnExpression.Column.ColumnDescription = TestColumn1Description;
            return columnExpression;
        }

        public static CreateColumnExpression GetCreateColumnExpressionWithDescriptionWithAdditionalDescriptions()
        {
            CreateColumnExpression columnExpression = GetCreateColumnExpression();
            columnExpression.Column.ColumnDescription = TestColumn1Description;
            columnExpression.Column.AdditionalColumnDescriptions = TestAdditionalColumnDescriptions1;
            return columnExpression;
        }

        public static CreateColumnExpression GetCreateColumnExpressionWithCollation()
        {
            CreateColumnExpression columnExpression = GetCreateColumnExpression();
            columnExpression.Column.CollationName = TestColumnCollationName;
            return columnExpression;
        }

        public static CreateColumnExpression GetAlterTableAutoIncrementColumnExpression()
        {
            ColumnDefinition column = new ColumnDefinition { Name = TestColumnName1, IsIdentity = true, Type = DbType.Int32 };
            return new CreateColumnExpression { TableName = TestTableName1, Column = column };
        }

        public static AlterTableExpression GetAlterTableWithDescriptionExpression()
        {
            return new AlterTableExpression() { TableName = TestTableName1, TableDescription = TestTableDescription };
        }

        public static AlterTableExpression GetAlterTable()
        {
            return new AlterTableExpression() { TableName = TestTableName1 };
        }

        public static RenameTableExpression GetRenameTableExpression()
        {
            var expression = new RenameTableExpression();
            expression.OldName = TestTableName1;
            expression.NewName = TestTableName2;
            return expression;
        }

        public static AlterColumnExpression GetAlterColumnAddAutoIncrementExpression()
        {
            ColumnDefinition column = new ColumnDefinition { Name = TestColumnName1, IsIdentity = true, IsPrimaryKey = true, Type = DbType.Int32, ModificationType = ColumnModificationType.Alter };
            return new AlterColumnExpression { TableName = TestTableName1, Column = column };
        }

        public static AlterColumnExpression GetAlterColumnExpression()
        {
            var expression = new AlterColumnExpression();
            expression.TableName = TestTableName1;

            expression.Column = new ColumnDefinition();
            expression.Column.Name = TestColumnName1;
            expression.Column.Type = DbType.String;
            expression.Column.Size = 20;
            expression.Column.IsNullable = false;
            expression.Column.ModificationType = ColumnModificationType.Alter;

            return expression;
        }

        public static CreateColumnExpression GetCreateColumnExpressionWithDateTimeOffsetType()
        {
            var expression = new CreateColumnExpression();
            expression.TableName = TestTableName1;

            expression.Column = new ColumnDefinition();
            expression.Column.Name = TestColumnName1;
            expression.Column.IsNullable = true;
            expression.Column.Type = DbType.DateTimeOffset;
            expression.Column.ModificationType = ColumnModificationType.Create;

            return expression;
        }

        public static CreateColumnExpression GetCreateColumnExpressionWithNullableCustomType()
        {
            var expression = new CreateColumnExpression();
            expression.TableName = TestTableName1;

            expression.Column = new ColumnDefinition();
            expression.Column.Name = TestColumnName1;
            expression.Column.IsNullable = true;
            expression.Column.CustomType = "MyDomainType";
            expression.Column.ModificationType = ColumnModificationType.Create;

            return expression;
        }

        public static CreateColumnExpression GetCreateColumnExpressionWithComputed()
        {
            ColumnDefinition column = new ColumnDefinition
            {
                Name = TestColumnName1,
                Expression = "Price * Quantity",
                ExpressionStored = false,
                ModificationType = ColumnModificationType.Create
            };
            return new CreateColumnExpression { TableName = TestTableName1, Column = column };
        }

        public static CreateColumnExpression GetCreateColumnExpressionWithStoredComputed()
        {
            ColumnDefinition column = new ColumnDefinition
            {
                Name = TestColumnName1,
                Expression = "Price * Quantity",
                ExpressionStored = true,
                ModificationType = ColumnModificationType.Create
            };
            return new CreateColumnExpression { TableName = TestTableName1, Column = column };
        }

        public static AlterColumnExpression GetAlterColumnExpressionWithComputed()
        {
            var expression = new AlterColumnExpression();
            expression.TableName = TestTableName1;

            expression.Column = new ColumnDefinition();
            expression.Column.Name = TestColumnName1;
            expression.Column.Expression = "Price * Quantity";
            expression.Column.ExpressionStored = false;
            expression.Column.ModificationType = ColumnModificationType.Alter;

            return expression;
        }

        public static AlterColumnExpression GetAlterColumnExpressionWithStoredComputed()
        {
            var expression = new AlterColumnExpression();
            expression.TableName = TestTableName1;

            expression.Column = new ColumnDefinition();
            expression.Column.Name = TestColumnName1;
            expression.Column.Expression = "Price * Quantity";
            expression.Column.ExpressionStored = true;
            expression.Column.ModificationType = ColumnModificationType.Alter;

            return expression;
        }

        public static CreateTableExpression GetCreateTableWithComputedColumn()
        {
            CreateTableExpression expression = new CreateTableExpression() { TableName = TestTableName1 };
            expression.Columns.Add(new ColumnDefinition { Name = "Price", Type = DbType.Decimal, Size = 10, Precision = 2 });
            expression.Columns.Add(new ColumnDefinition { Name = "Quantity", Type = DbType.Int32 });
            expression.Columns.Add(new ColumnDefinition
            {
                Name = "Total",
                Expression = "Price * Quantity",
                ExpressionStored = false
            });
            return expression;
        }

        public static CreateTableExpression GetCreateTableWithStoredComputedColumn()
        {
            CreateTableExpression expression = new CreateTableExpression() { TableName = TestTableName1 };
            expression.Columns.Add(new ColumnDefinition { Name = "Price", Type = DbType.Decimal, Size = 10, Precision = 2 });
            expression.Columns.Add(new ColumnDefinition { Name = "Quantity", Type = DbType.Int32 });
            expression.Columns.Add(new ColumnDefinition
            {
                Name = "Total",
                Expression = "Price * Quantity",
                ExpressionStored = true
            });
            return expression;
        }

        public static AlterColumnExpression GetAlterColumnExpressionWithDescription()
        {
            var columnExpression = GetAlterColumnExpression();
            columnExpression.Column.ColumnDescription = TestColumn1Description;
            return columnExpression;
        }

        public static AlterColumnExpression GetAlterColumnExpressionWithDescriptionWithAdditionalDescriptions()
        {
            var columnExpression = GetAlterColumnExpression();
            columnExpression.Column.ColumnDescription = TestColumn1Description;
            columnExpression.Column.AdditionalColumnDescriptions = TestAdditionalColumnDescriptions1;
            return columnExpression;
        }

        public static AlterColumnExpression GetAlterColumnExpressionWithCollation()
        {
            var columnExpression = GetAlterColumnExpression();
            columnExpression.Column.CollationName = TestColumnCollationName;
            return columnExpression;
        }

        public static AlterSchemaExpression GetAlterSchemaExpression()
        {
            return new AlterSchemaExpression { DestinationSchemaName = "TestSchema2", SourceSchemaName = "TestSchema1", TableName = "TestTable" };
        }

        public static CreateForeignKeyExpression GetCreateForeignKeyExpression()
        {
            var expression = new CreateForeignKeyExpression
            {
                ForeignKey =
                {
                    PrimaryTable = TestTableName2,
                    ForeignTable = TestTableName1,
                    PrimaryColumns = new[] {TestColumnName2},
                    ForeignColumns = new[] {TestColumnName1}
                }
            };

            var processed = expression.Apply(ConventionSets.NoSchemaName);

            return processed;
        }

        public static CreateForeignKeyExpression GetCreateMultiColumnForeignKeyExpression()
        {
            var expression = new CreateForeignKeyExpression
            {
                ForeignKey =
                {
                    PrimaryTable = TestTableName2,
                    ForeignTable = TestTableName1,
                    PrimaryColumns = new[] {TestColumnName2, "TestColumn4"},
                    ForeignColumns = new[] {TestColumnName1, "TestColumn3"}
                }
            };

            var processed = expression.Apply(ConventionSets.NoSchemaName);

            return processed;
        }

        public static CreateForeignKeyExpression GetCreateNamedForeignKeyExpression()
        {
            var expression = new CreateForeignKeyExpression();
            expression.ForeignKey.Name = "FK_Test";
            expression.ForeignKey.PrimaryTable = TestTableName2;
            expression.ForeignKey.ForeignTable = TestTableName1;
            expression.ForeignKey.PrimaryColumns = new[] { TestColumnName2 };
            expression.ForeignKey.ForeignColumns = new[] { TestColumnName1 };

            return expression;
        }

        public static CreateForeignKeyExpression GetCreateNamedMultiColumnForeignKeyExpression()
        {
            var expression = new CreateForeignKeyExpression();
            expression.ForeignKey.Name = "FK_Test";
            expression.ForeignKey.PrimaryTable = TestTableName2;
            expression.ForeignKey.ForeignTable = TestTableName1;
            expression.ForeignKey.PrimaryColumns = new[] { TestColumnName2, "TestColumn4" };
            expression.ForeignKey.ForeignColumns = new[] { TestColumnName1, "TestColumn3" };

            return expression;
        }

        public static DeleteTableExpression GetDeleteTableExpression()
        {
            return new DeleteTableExpression { TableName = TestTableName1 };
        }

        public static DeleteTableExpression GetDeleteTableIfExistsExpression()
        {
            return new DeleteTableExpression { TableName = TestTableName1, IfExists = true };
        }

        public static DeleteColumnExpression GetDeleteColumnExpression()
        {
            return GetDeleteColumnExpression(new[] { TestColumnName1 });
        }

        public static DeleteColumnExpression GetDeleteColumnExpression(string[] columns)
        {
            return new DeleteColumnExpression { TableName = TestTableName1, ColumnNames = columns };
        }

        public static DeleteIndexExpression GetDeleteIndexExpression()
        {
            IndexDefinition indexDefinition = new IndexDefinition { Name = TestIndexName, TableName = TestTableName1 };
            return new DeleteIndexExpression { Index = indexDefinition };
        }

        public static DeleteForeignKeyExpression GetDeleteForeignKeyExpression()
        {
            var expression = new DeleteForeignKeyExpression();
            expression.ForeignKey.Name = "FK_Test";
            expression.ForeignKey.ForeignTable = TestTableName1;
            return expression;
        }

        public static DeleteConstraintExpression GetDeletePrimaryKeyExpression()
        {
            var expression = new DeleteConstraintExpression(ConstraintType.PrimaryKey);
            expression.Constraint.TableName = TestTableName1;
            expression.Constraint.ConstraintName = "TESTPRIMARYKEY";
            return expression;
        }

        public static DeleteSchemaExpression GetDeleteSchemaExpression()
        {
            return new DeleteSchemaExpression { SchemaName = "TestSchema" };
        }

        public static DeleteSequenceExpression GetDeleteSequenceExpression()
        {
            return new DeleteSequenceExpression { SequenceName = "Sequence" };
        }

        public static DeleteConstraintExpression GetDeleteUniqueConstraintExpression()
        {
            var expression = new DeleteConstraintExpression(ConstraintType.Unique);
            expression.Constraint.TableName = TestTableName1;
            expression.Constraint.ConstraintName = "TESTUNIQUECONSTRAINT";
            return expression;
        }


        public static CreateConstraintExpression GetCreatePrimaryKeyExpression()
        {
            var expression = new CreateConstraintExpression(ConstraintType.PrimaryKey);
            expression.Constraint.TableName = TestTableName1;
            expression.Constraint.Columns.Add(TestColumnName1);
            var processed = expression.Apply(ConventionSets.NoSchemaName);
            return processed;
        }

        public static CreateConstraintExpression GetCreateNamedPrimaryKeyExpression()
        {
            var expression = new CreateConstraintExpression(ConstraintType.PrimaryKey);
            expression.Constraint.TableName = TestTableName1;
            expression.Constraint.Columns.Add(TestColumnName1);
            expression.Constraint.ConstraintName = "TESTPRIMARYKEY";
            return expression;
        }

        public static CreateConstraintExpression GetCreateMultiColumnPrimaryKeyExpression()
        {
            var expression = new CreateConstraintExpression(ConstraintType.PrimaryKey);
            expression.Constraint.TableName = TestTableName1;
            expression.Constraint.Columns.Add(TestColumnName1);
            expression.Constraint.Columns.Add(TestColumnName2);
            var processed = expression.Apply(ConventionSets.NoSchemaName);
            return processed;
        }

        public static CreateConstraintExpression GetCreateNamedMultiColumnPrimaryKeyExpression()
        {
            var expression = new CreateConstraintExpression(ConstraintType.PrimaryKey);
            expression.Constraint.TableName = TestTableName1;
            expression.Constraint.Columns.Add(TestColumnName1);
            expression.Constraint.Columns.Add(TestColumnName2);
            expression.Constraint.ConstraintName = "TESTPRIMARYKEY";
            return expression;
        }

        public static CreateConstraintExpression GetCreateUniqueConstraintExpression()
        {
            var expression = new CreateConstraintExpression(ConstraintType.Unique);
            expression.Constraint.TableName = TestTableName1;
            expression.Constraint.Columns.Add(TestColumnName1);
            var processed = expression.Apply(ConventionSets.NoSchemaName);
            return processed;
        }

        public static CreateConstraintExpression GetCreateNamedUniqueConstraintExpression()
        {
            var expression = new CreateConstraintExpression(ConstraintType.Unique);
            expression.Constraint.TableName = TestTableName1;
            expression.Constraint.Columns.Add(TestColumnName1);
            expression.Constraint.ConstraintName = "TESTUNIQUECONSTRAINT";
            return expression;
        }

        public static CreateConstraintExpression GetCreateMultiColumnUniqueConstraintExpression()
        {
            var expression = new CreateConstraintExpression(ConstraintType.Unique);
            expression.Constraint.TableName = TestTableName1;
            expression.Constraint.Columns.Add(TestColumnName1);
            expression.Constraint.Columns.Add(TestColumnName2);
            var processed = expression.Apply(ConventionSets.NoSchemaName);
            return processed;
        }

        public static CreateConstraintExpression GetCreateNamedMultiColumnUniqueConstraintExpression()
        {
            var expression = new CreateConstraintExpression(ConstraintType.Unique);
            expression.Constraint.TableName = TestTableName1;
            expression.Constraint.Columns.Add(TestColumnName1);
            expression.Constraint.Columns.Add(TestColumnName2);
            expression.Constraint.ConstraintName = "TESTUNIQUECONSTRAINT";
            return expression;
        }

        public static AlterDefaultConstraintExpression GetAlterDefaultConstraintExpression()
        {
            var expression = new AlterDefaultConstraintExpression
            {
                ColumnName = TestColumnName1,
                DefaultValue = 1,
                TableName = TestTableName1
            };
            return expression;
        }

        public static DeleteDefaultConstraintExpression GetDeleteDefaultConstraintExpression()
        {
            var expression = new DeleteDefaultConstraintExpression
            {
                ColumnName = TestColumnName1,
                TableName = TestTableName1
            };
            return expression;
        }
    }
}
