#region License
// Copyright (c) 2018, Fluent Migrator Project
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using FluentMigrator.Infrastructure;

namespace FluentMigrator.Expressions
{
    public class ExecuteEmbeddedSqlScriptExpression : MigrationExpressionBase
    {
        public string SqlScript { get; set; }

        public IAssemblyCollection MigrationAssemblies { get; set; }

        /// <summary>
        /// Gets or sets parameters to be replaced before script execution
        /// </summary>
        public IDictionary<string, string> Parameters { get; set; }

        public override void ExecuteWith(IMigrationProcessor processor)
        {

            string sqlText;
            var embeddedResourceNameWithAssembly = GetQualifiedResourcePath();

            using (var stream = embeddedResourceNameWithAssembly
                .Assembly.GetManifestResourceStream(embeddedResourceNameWithAssembly.Name))
            using (var reader = new StreamReader(stream))
            {
                sqlText = reader.ReadToEnd();
            }

            sqlText = SqlScriptTokenReplacer.ReplaceSqlScriptTokens(sqlText, Parameters);

            processor.Execute(sqlText);
        }

        private ManifestResourceNameWithAssembly GetQualifiedResourcePath()
        {
            var resources = MigrationAssemblies.GetManifestResourceNames();

            //resource full name is in format `namespace.resourceName`
            var sqlScriptParts = SqlScript.Split('.').Reverse().ToArray();
            Func<ManifestResourceNameWithAssembly, bool> isNameMatch = x =>
                x.Name.Split('.')
                .Reverse()
                .Take(sqlScriptParts.Length)
                .SequenceEqual(sqlScriptParts, StringComparer.InvariantCultureIgnoreCase);

            var foundResources = resources.Where(isNameMatch).ToArray();

            if (foundResources.Length == 0)
                throw new InvalidOperationException(string.Format("Could not find resource named {0} in assemblies {1}", SqlScript, string.Join(", ", MigrationAssemblies.Assemblies.Select(a => a.FullName).ToArray())));

            if (foundResources.Length > 1)
                throw new InvalidOperationException(string.Format(@"Could not find unique resource named {0} in assemblies {1}.
Possible candidates are:

{2}
",
 SqlScript,
 string.Join(", ", MigrationAssemblies.Assemblies.Select(a => a.FullName).ToArray()),
 string.Join(Environment.NewLine + "\t", foundResources.Select(r => r.Name).ToArray())));

            return foundResources[0];
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
