namespace FluentMigrator
{
	public interface IQuerySchema
	{
		bool TableExists(string tableName);
		bool ColumnExists(string tableName, string columnName);
		bool ConstraintExists(string tableName, string constraintName);
	}
}