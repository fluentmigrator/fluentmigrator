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

            // since all the Processors are using String.Format() in their Execute method
            //  we need to escape the brackets with double brackets or else it throws an incorrect format error on the String.Format call
            sqlText = sqlText.Replace("{", "{{").Replace("}", "}}");
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

		public override string ToString()
		{
			return base.ToString() + SqlScript;
		}
	}
}
