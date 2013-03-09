#region License
// 
// Copyright (c) 2007-2009, Sean Chambers <schambers80@gmail.com>
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


namespace FluentMigrator.Infrastructure
{
    public static class ErrorMessages
    {
        public const string ColumnNameCannotBeNullOrEmpty = "The column's name cannot be null or an empty string.";
        public const string ColumnTypeMustBeDefined = "The column does not have a type defined.";
        public const string ColumnNamesMustBeUnique = "Column names must be unique.";
        public const string SchemaNameCannotBeNullOrEmpty = "The schema's name cannot be null or an empty string.";
        public const string TableNameCannotBeNullOrEmpty = "The table's name cannot be null or an empty string.";
        public const string OldColumnNameCannotBeNullOrEmpty = "The old column name cannot be null or empty.";
        public const string NewColumnNameCannotBeNullOrEmpty = "The new column name cannot be null or empty.";
        public const string OldTableNameCannotBeNullOrEmpty = "The old table name cannot be null or empty.";
        public const string NewTableNameCannotBeNullOrEmpty = "The new table name cannot be null or empty.";
        public const string ForeignTableNameCannotBeNullOrEmpty = "The foreign table name cannot be null or empty.";
        public const string PrimaryTableNameCannotBeNullOrEmpty = "The primary table name cannot be null or empty.";
        public const string ForeignKeyNameCannotBeNullOrEmpty = "The foreign key's name cannot be null or an empty string.";
        public const string ForeignKeyMustHaveOneOrMoreForeignColumns = "The foreign key must have one or more foreign columns.";
        public const string ForeignKeyMustHaveOneOrMorePrimaryColumns = "The foreign key must have one or more primary columns.";
        public const string IndexNameCannotBeNullOrEmpty = "The index's name cannot be null or an empty string.";
        public const string IndexMustHaveOneOrMoreColumns = "The index must have one or more columns.";
        public const string SqlStatementCannotBeNullOrEmpty = "The sql statement cannot be null or an empty string.";
        public const string SqlScriptCannotBeNullOrEmpty = "The sql script cannot be null or an empty string.";
        public const string OperationCannotBeNull = "The operation to be performed using the database connection cannot be null.";
        public const string DestinationSchemaCannotBeNull = "The destination schema's name cannot be null or an empty string.";
        public const string SequenceNameCannotBeNullOrEmpty = "The sequence's name cannot be null or an empty string.";
        public const string UpdateDataExpressionMustSpecifyWhereClauseOrAllRows = "Update statement is missing a condition. Specify one by calling .Where() or target all rows by calling .AllRows().";
        public const string UpdateDataExpressionMustNotSpecifyBothWhereClauseAndAllRows = "Update statement specifies both a .Where() condition and that .AllRows() should be targeted. Specify one or the other, but not both.";
        public const string DefaultValueCannotBeNull = "The default value cannot be null.";
        public const string ConstraintMustHaveAtLeastOneColumn = "The constraint must have at least one column specified";
    }
}