namespace FluentMigrator.Tests.Unit.Generators.CreateTable
{
	public interface IExpectedCreateTableTestResults
	{
		string CreateTable();
		string CreateTableWithCustomColumnType();
		string CreateTableWithPrimaryKey();
		string CreateTableWithIdentity();
		string CreateTableWithNullField();
		string CreateTableWithDefaultValue();
		string CreateTableWithDefaultValueExplicitlySetToNull();
	}
}