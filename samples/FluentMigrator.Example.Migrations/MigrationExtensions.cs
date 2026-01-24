#region License
//
// Copyright (c) 2007-2024, Fluent Migrator Project
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

using FluentMigrator.Builders.Create;
using FluentMigrator.Builders.Create.Table;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Example.Migrations
{
    internal static class MigrationExtensions
    {
        public static ICreateTableColumnOptionOrWithColumnSyntax WithIdColumn(this ICreateTableWithColumnSyntax tableWithColumnSyntax)
        {
            return tableWithColumnSyntax
                .WithColumn("Id")
                .AsInt32()
                .NotNullable()
                .PrimaryKey()
                .Identity();
        }

        public static ICreateTableColumnOptionOrWithColumnSyntax WithTimeStamps(this ICreateTableWithColumnSyntax tableWithColumnSyntax)
        {
            return tableWithColumnSyntax
                .WithColumn("CreatedAt").AsDateTime().NotNullable()
                .WithColumn("ModifiedAt").AsDateTime().NotNullable();
        }

        /// <summary>
        /// Creates an audit table suffixes with "Audit", with the same columns as the current tale, plus ModifiedAt and ModifiedBy columns
        /// </summary>
        /// <param name="tableWithColumnSyntax"></param>
        /// <returns></returns>
        public static void WithAuditTable(this ICreateTableWithColumnSyntax tableWithColumnSyntax)
        {
            if (tableWithColumnSyntax is not CreateTableExpressionBuilder createTable)
            {
                throw new InvalidOperationException("WithAuditTable can only be used within a Create.Table expression");
            }

            var context = ((IMigrationContextAccessor)createTable).GetMigrationContext();

            // Determine audit table name
            var auditTableName = createTable.Expression.TableName + "Audit";

            // Create audit table
            var createExpression = new CreateExpressionRoot(context)
                .Table(auditTableName)
                .InSchema(createTable.Expression.SchemaName);

            // Copy columns from original table
            foreach (var column in createTable.Expression.Columns)
            {
                var col = createExpression
                    .WithColumn(column.Name)
                    .AsColumnDataType(column);

                if (column.IsNullable == false)
                {
                    col.NotNullable();
                }
                else
                {
                    col.Nullable();
                }
            }

            // Add audit columns
            createExpression
                .WithColumn("ModifiedAt").AsDateTime().NotNullable()
                .WithColumn("ModifiedBy").AsString(255).NotNullable();
        }
    }
}
