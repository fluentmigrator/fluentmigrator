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

using Microsoft.SqlServer.Dac;
using Microsoft.SqlServer.Dac.Model;

namespace FluentMigrator.SqlProj
{
    /// <summary>
    /// Extracts database schema model from SQL Server Database Project
    /// </summary>
    public class SqlProjModelExtractor
    {
        /// <summary>
        /// Builds a TSqlModel from a .sqlproj file
        /// </summary>
        /// <param name="sqlProjPath">Path to the .sqlproj file</param>
        /// <returns>TSqlModel representing the database schema</returns>
        public TSqlModel BuildModelFromSqlProj(string sqlProjPath)
        {
            if (!File.Exists(sqlProjPath))
            {
                throw new FileNotFoundException($"SqlProj file not found: {sqlProjPath}");
            }

            // Use DacFx to build the model from the project file
            // This will parse all SQL scripts and build a complete schema model
            var model = new TSqlModel(sqlProjPath, DacSchemaModelStorageType.Memory);
            
            return model;
        }

        /// <summary>
        /// Gets table information from the TSqlModel
        /// </summary>
        /// <param name="model">The TSqlModel</param>
        /// <returns>List of table definitions</returns>
        public List<TableDefinition> GetTables(TSqlModel model)
        {
            var tables = new List<TableDefinition>();
            
            foreach (var table in model.GetObjects(DacQueryScopes.UserDefined, Table.TypeClass))
            {
                var tableName = table.Name;
                var schemaName = tableName.Parts.Count > 1 ? tableName.Parts[0] : "dbo";
                var objectName = tableName.Parts.Count > 1 ? tableName.Parts[1] : tableName.Parts[0];

                var tableDefinition = new TableDefinition
                {
                    SchemaName = schemaName,
                    TableName = objectName,
                    Columns = GetColumns(table)
                };

                tables.Add(tableDefinition);
            }

            return tables;
        }

        private List<ColumnDefinition> GetColumns(TSqlObject table)
        {
            var columns = new List<ColumnDefinition>();
            
            foreach (var column in table.GetReferenced(Table.Columns))
            {
                var columnName = column.Name.Parts.Last();
                var dataType = column.GetReferenced(Column.DataType).FirstOrDefault();
                
                var columnDefinition = new ColumnDefinition
                {
                    Name = columnName,
                    DataType = GetDataTypeName(dataType),
                    IsNullable = column.GetProperty<bool>(Column.Nullable),
                    IsIdentity = column.GetProperty<bool>(Column.IsIdentity),
                    Length = GetLength(column, dataType),
                    Precision = GetPrecision(column, dataType),
                    Scale = GetScale(column, dataType)
                };

                columns.Add(columnDefinition);
            }

            return columns;
        }

        private string GetDataTypeName(TSqlObject? dataType)
        {
            if (dataType == null)
                return "UNKNOWN";

            var sqlDataType = dataType.GetProperty<SqlDataType>(DataType.SqlDataType);
            return sqlDataType.ToString().ToUpperInvariant();
        }

        private int? GetLength(TSqlObject column, TSqlObject? dataType)
        {
            if (dataType == null)
                return null;

            var sqlDataType = dataType.GetProperty<SqlDataType>(DataType.SqlDataType);
            
            // For string types
            if (sqlDataType == SqlDataType.VarChar || 
                sqlDataType == SqlDataType.NVarChar || 
                sqlDataType == SqlDataType.Char || 
                sqlDataType == SqlDataType.NChar)
            {
                var length = column.GetProperty<int>(Column.Length);
                return length == 0 ? null : length;
            }

            return null;
        }

        private int? GetPrecision(TSqlObject column, TSqlObject? dataType)
        {
            if (dataType == null)
                return null;

            var sqlDataType = dataType.GetProperty<SqlDataType>(DataType.SqlDataType);
            
            // For numeric types
            if (sqlDataType == SqlDataType.Decimal || sqlDataType == SqlDataType.Numeric)
            {
                return column.GetProperty<int>(Column.Precision);
            }

            return null;
        }

        private int? GetScale(TSqlObject column, TSqlObject? dataType)
        {
            if (dataType == null)
                return null;

            var sqlDataType = dataType.GetProperty<SqlDataType>(DataType.SqlDataType);
            
            // For numeric types
            if (sqlDataType == SqlDataType.Decimal || sqlDataType == SqlDataType.Numeric)
            {
                return column.GetProperty<int>(Column.Scale);
            }

            return null;
        }
    }

    /// <summary>
    /// Represents a table definition extracted from SQL project
    /// </summary>
    public class TableDefinition
    {
        public string SchemaName { get; set; } = "dbo";
        public string TableName { get; set; } = string.Empty;
        public List<ColumnDefinition> Columns { get; set; } = new();
    }

    /// <summary>
    /// Represents a column definition
    /// </summary>
    public class ColumnDefinition
    {
        public string Name { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
        public bool IsNullable { get; set; }
        public bool IsIdentity { get; set; }
        public int? Length { get; set; }
        public int? Precision { get; set; }
        public int? Scale { get; set; }
    }
}
