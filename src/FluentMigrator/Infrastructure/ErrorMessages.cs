using System;

namespace FluentMigrator.Infrastructure
{
	public static class ErrorMessages
	{
		public const string ColumnNameCannotBeNullOrEmpty = "The column's name cannot be null or an empty string";
		public const string ColumnTypeMustBeDefined = "The column does not have a type defined";
		public const string TableNameCannotBeNullOrEmpty = "The table's name cannot be null or an empty string";
		public const string OldColumnNameCannotBeNullOrEmpty = "The old column name cannot be null or empty";
		public const string NewColumnNameCannotBeNullOrEmpty = "The new column name cannot be null or empty";
		public const string OldTableNameCannotBeNullOrEmpty = "The old table name cannot be null or empty";
		public const string NewTableNameCannotBeNullOrEmpty = "The new table name cannot be null or empty";
		public const string ForeignTableNameCannotBeNullOrEmpty = "The foreign table name cannot be null or empty";
		public const string PrimaryTableNameCannotBeNullOrEmpty = "The primary table name cannot be null or empty";
		public const string ForeignKeyNameCannotBeNullOrEmpty = "The foreign key's name cannot be null or an empty string";
		public const string ForeignKeyMustHaveOneOrMoreForeignColumns = "The foreign key must have one or more foreign columns";
		public const string ForeignKeyMustHaveOneOrMorePrimaryColumns = "The foreign key must have one or more primary columns";
		public const string ForeignKeyCannotBeSelfReferential = "The foreign key's foreign table cannot be the same as its primary table";
		public const string IndexNameCannotBeNullOrEmpty = "The index's name cannot be null or an empty string";
		public const string IndexMustHaveOneOrMoreColumns = "The index must have one or more columns";
		public const string SqlStatementCannotBeNullOrEmpty = "The sql statement cannot be null or an empty string";
		public const string SqlScriptCannotBeNullOrEmpty = "The sql script cannot be null or an empty string";
	}
}