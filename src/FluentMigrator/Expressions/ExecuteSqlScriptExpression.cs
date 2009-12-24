using System.Collections.Generic;
using System.IO;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Expressions
{
	public class ExecuteSqlScriptExpression : MigrationExpressionBase
	{
		public string SqlScript { get; set; }

		public override void ExecuteWith( IMigrationProcessor processor )
		{
			string sqlText;
			using (var reader = File.OpenText(SqlScript))
				sqlText = reader.ReadToEnd();

			processor.Execute(sqlText);
		}

		public override void ApplyConventions(IMigrationConventions conventions)
		{
			SqlScript = string.Format(@"{0}\{1}", conventions.GetWorkingDirectory(), SqlScript);
		}

		public override void CollectValidationErrors( ICollection<string> errors )
		{
			if (string.IsNullOrEmpty(SqlScript))
				errors.Add(ErrorMessages.SqlScriptCannotBeNullOrEmpty);
		}
	}
}
