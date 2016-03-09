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
using System.Text.RegularExpressions;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Expressions
{
    public class ExecuteSqlScriptExpression : MigrationExpressionBase
    {
        public string SqlScript { get; set; }

        public IDictionary<string, string> Parameters { get; set; }

        public override void ExecuteWith(IMigrationProcessor processor)
        {
            string sqlText;
            using (var reader = File.OpenText(SqlScript))
                sqlText = reader.ReadToEnd();

            // since all the Processors are using String.Format() in their Execute method
            //	1) we need to escape the brackets with double brackets or else it throws an incorrect format error on the String.Format call
            //	2) we need to replace tokens
            //	3) we need to replace escaped tokens
            sqlText = Regex.Replace(
                Regex.Replace(
                    sqlText.Replace("{", "{{").Replace("}", "}}"),
                    @"\$\((?<token>\w+)\)",
                    m => ((Parameters != null) && Parameters.ContainsKey(m.Groups["token"].Value))
                        ? Parameters[m.Groups["token"].Value]
                        : ""),
                @"\${2}\({2}(?<token>\w+)\){2}",
                m => string.Format("$({0})", m.Groups["token"]));

            // adding ability to pass parameters to execute function
            processor.Execute(sqlText);
        }

        public override void ApplyConventions(IMigrationConventions conventions)
        {
            SqlScript = Path.Combine(conventions.GetWorkingDirectory(), SqlScript);
        }

        public override void CollectValidationErrors(ICollection<string> errors)
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
