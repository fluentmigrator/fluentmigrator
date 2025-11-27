#region License
// Copyright (c) 2007-2024, Fluent Migrator Project
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System.Text;

namespace FluentMigrator.SqlProj
{
    /// <summary>
    /// Generates FluentMigrator C# migration code from database schema definitions
    /// </summary>
    public class MigrationCodeGenerator
    {
        private const int DefaultFixedLengthStringSize = 1;
        private readonly string _migrationNamespace;

        public MigrationCodeGenerator(string migrationNamespace = "Migrations")
        {
            _migrationNamespace = migrationNamespace;
        }

        /// <summary>
        /// Generates a complete migration class from table definitions
        /// </summary>
        /// <param name="tables">List of tables to include in migration</param>
        /// <param name="migrationName">Name of the migration</param>
        /// <param name="version">Migration version (timestamp)</param>
        /// <returns>C# source code for the migration</returns>
        public string GenerateMigration(List<TableDefinition> tables, string migrationName, long version)
        {
            var sb = new StringBuilder();
            
            // Generate using statements
            sb.AppendLine("using FluentMigrator;");
            sb.AppendLine();
            sb.AppendLine($"namespace {_migrationNamespace}");
            sb.AppendLine("{");
            sb.AppendLine($"    [Migration({version})]");
            sb.AppendLine($"    public class {migrationName} : Migration");
            sb.AppendLine("    {");
            sb.AppendLine("        public override void Up()");
            sb.AppendLine("        {");

            // Generate table creation code
            foreach (var table in tables)
            {
                GenerateCreateTable(sb, table);
            }

            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        public override void Down()");
            sb.AppendLine("        {");

            // Generate table deletion code (in reverse order)
            for (int i = tables.Count - 1; i >= 0; i--)
            {
                var table = tables[i];
                if (table.SchemaName != "dbo")
                {
                    sb.AppendLine($"            Delete.Table(\"{table.TableName}\").InSchema(\"{table.SchemaName}\");");
                }
                else
                {
                    sb.AppendLine($"            Delete.Table(\"{table.TableName}\");");
                }
            }

            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        private void GenerateCreateTable(StringBuilder sb, TableDefinition table)
        {
            // Start table creation
            if (table.SchemaName != "dbo")
            {
                sb.AppendLine($"            Create.Table(\"{table.TableName}\").InSchema(\"{table.SchemaName}\")");
            }
            else
            {
                sb.AppendLine($"            Create.Table(\"{table.TableName}\")");
            }

            // Generate columns
            foreach (var column in table.Columns)
            {
                GenerateColumn(sb, column);
            }

            sb.AppendLine("                ;");
            sb.AppendLine();
        }

        private void GenerateColumn(StringBuilder sb, ColumnDefinition column)
        {
            var columnDef = $"                .WithColumn(\"{column.Name}\")";

            // Add data type
            columnDef += GetFluentMigratorDataType(column);

            // Add nullable/not nullable
            if (!column.IsNullable)
            {
                columnDef += ".NotNullable()";
            }
            else
            {
                columnDef += ".Nullable()";
            }

            // Add identity if applicable
            if (column.IsIdentity)
            {
                columnDef += ".Identity()";
            }

            sb.AppendLine(columnDef);
        }

        private string GetFluentMigratorDataType(ColumnDefinition column)
        {
            return column.DataType.ToUpperInvariant() switch
            {
                "INT" => ".AsInt32()",
                "BIGINT" => ".AsInt64()",
                "SMALLINT" => ".AsInt16()",
                "TINYINT" => ".AsByte()",
                "BIT" => ".AsBoolean()",
                "DECIMAL" => column.Precision.HasValue && column.Scale.HasValue 
                    ? $".AsDecimal({column.Precision}, {column.Scale})" 
                    : ".AsDecimal()",
                "NUMERIC" => column.Precision.HasValue && column.Scale.HasValue 
                    ? $".AsDecimal({column.Precision}, {column.Scale})" 
                    : ".AsDecimal()",
                "MONEY" => ".AsCurrency()",
                "SMALLMONEY" => ".AsCurrency()",
                "FLOAT" => ".AsDouble()",
                "REAL" => ".AsFloat()",
                "DATE" => ".AsDate()",
                "DATETIME" => ".AsDateTime()",
                "DATETIME2" => ".AsDateTime2()",
                "SMALLDATETIME" => ".AsDateTime()",
                "TIME" => ".AsTime()",
                "DATETIMEOFFSET" => ".AsDateTimeOffset()",
                "CHAR" => column.Length.HasValue ? $".AsFixedLengthAnsiString({column.Length})" : ".AsFixedLengthAnsiString(DefaultFixedLengthStringSize)",
                "VARCHAR" => column.Length.HasValue ? $".AsAnsiString({column.Length})" : ".AsAnsiString()",
                "TEXT" => ".AsAnsiString(int.MaxValue)",
                "NCHAR" => column.Length.HasValue ? $".AsFixedLengthString({column.Length})" : ".AsFixedLengthString(DefaultFixedLengthStringSize)",
                "NVARCHAR" => column.Length.HasValue ? $".AsString({column.Length})" : ".AsString()",
                "NTEXT" => ".AsString(int.MaxValue)",
                "BINARY" => column.Length.HasValue ? $".AsBinary({column.Length})" : ".AsBinary()",
                "VARBINARY" => column.Length.HasValue ? $".AsBinary({column.Length})" : ".AsBinary()",
                "IMAGE" => ".AsBinary(int.MaxValue)",
                "UNIQUEIDENTIFIER" => ".AsGuid()",
                "XML" => ".AsXml()",
                _ => $".AsCustom(\"{column.DataType}\")"
            };
        }

        /// <summary>
        /// Generates a timestamp-based version number for migrations
        /// </summary>
        /// <returns>Version number in format YYYYMMDDHHmmss</returns>
        public static long GenerateVersionNumber()
        {
            var now = DateTime.UtcNow;
            return now.Year * 10000000000L +
                   now.Month * 100000000L +
                   now.Day * 1000000L +
                   now.Hour * 10000L +
                   now.Minute * 100L +
                   now.Second;
        }
    }
}
