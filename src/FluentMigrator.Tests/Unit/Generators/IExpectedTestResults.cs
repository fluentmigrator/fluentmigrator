namespace FluentMigrator.Tests.Unit.Generators
{
	public interface IExpectedTestResults
	{
		string AddColumn();
		string AddDecimalColumn();
		string CreateForeignKey();
		string CreateUniqueClusteredIndex();
		string DropForeignKey();
		string DropTable();
		string InsertData();
		string InsertGuidData();
		string RenameColumn();
		string RenameTable();
		string DropColumn();
	}
}