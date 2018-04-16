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
    /// <summary>
    /// Expression to execute an embedded SQL script
    /// </summary>
    public class ExecuteEmbeddedSqlScriptExpression : MigrationExpressionBase
    {
        /// <summary>
        /// Gets or sets the SQL script name
        /// </summary>
        public string SqlScript { get; set; }

        /// <summary>
        /// Gets or sets the migration assemblies
        /// </summary>
        public IAssemblyCollection MigrationAssemblies { get; set; }

        /// <summary>
        /// Gets or sets parameters to be replaced before script execution
        /// </summary>
        public IDictionary<string, string> Parameters { get; set; }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public override void CollectValidationErrors(ICollection<string> errors)
        {
            if (string.IsNullOrEmpty(SqlScript))
                errors.Add(ErrorMessages.SqlScriptCannotBeNullOrEmpty);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return base.ToString() + SqlScript;
        }

        /// <summary>
        /// Creates an exception about a missing SQL script
        /// </summary>
        /// <param name="sqlScript">The name of the SQL script</param>
        /// <returns>The exception to be thrown</returns>
        protected Exception NewNotFoundException(string sqlScript)
        {
            return new InvalidOperationException(string.Format("Could not find resource named {0} in assemblies {1}", sqlScript, string.Join(", ", MigrationAssemblies.Assemblies.Select(a => a.FullName).ToArray())));
        }

        /// <summary>
        /// An exception to be thrown when the name of the embedded SQL script is ambiguous.
        /// </summary>
        /// <param name="sqlScript">The name of the SQL script</param>
        /// <param name="foundResources">The found resource names</param>
        /// <returns>The exception to be thrown</returns>
        protected Exception NewNoUniqueResourceException(string sqlScript, IEnumerable<ManifestResourceNameWithAssembly> foundResources)
        {
            return new InvalidOperationException(string.Format(@"Could not find unique resource named {0} in assemblies {1}.
Possible candidates are:

{2}
",
                sqlScript,
                string.Join(", ", MigrationAssemblies.Assemblies.Select(a => a.FullName).ToArray()),
                string.Join(Environment.NewLine + "\t", foundResources.Select(r => r.Name).ToArray())));
        }

        /// <summary>
        /// Gets the fully qualified ressource name and assembly
        /// </summary>
        /// <returns>the fully qualified ressource name and assembly</returns>
        protected virtual ManifestResourceNameWithAssembly GetQualifiedResourcePath()
        {
            var foundResources = FindResourceName(SqlScript);

            if (foundResources.Length == 0)
                throw NewNotFoundException(SqlScript);

            if (foundResources.Length > 1)
                throw NewNoUniqueResourceException(SqlScript, foundResources);

            return foundResources[0];
        }

        /// <summary>
        /// Finds ressources with the given name
        /// </summary>
        /// <param name="sqlScript">The name of the SQL script ressource to be found</param>
        /// <returns>The found ressources</returns>
        protected virtual ManifestResourceNameWithAssembly[] FindResourceName(string sqlScript)
        {
            var resources = MigrationAssemblies.GetManifestResourceNames();

            //resource full name is in format `namespace.resourceName`
            var sqlScriptParts = sqlScript.Split('.').Reverse().ToArray();
            Func<ManifestResourceNameWithAssembly, bool> isNameMatch = x =>
                x.Name.Split('.')
                    .Reverse()
                    .Take(sqlScriptParts.Length)
                    .SequenceEqual(sqlScriptParts, StringComparer.InvariantCultureIgnoreCase);

            var foundResources = resources.Where(isNameMatch).ToArray();
            return foundResources;
        }
    }
}
