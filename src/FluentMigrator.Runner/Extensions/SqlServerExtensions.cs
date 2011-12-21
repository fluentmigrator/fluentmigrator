using FluentMigrator.Builders.Insert;

namespace FluentMigrator.Runner.Extensions
{
	public static class SqlServerExtensions
	{
		public const string IdentityInsert = "SqlServerIdentityInsert";

		/// <summary>
		/// Inserts data using Sql Server's IDENTITY INSERT feature.
		/// </summary>
		/// <param name="expression"></param>
		/// <returns></returns>
		public static IInsertDataSyntax WithSqlServerIdentityInsert(this InsertDataExpressionBuilder expression)
		{
			expression.AddAdditionalFeature(IdentityInsert, true);
			return expression;
		}
	}
}
