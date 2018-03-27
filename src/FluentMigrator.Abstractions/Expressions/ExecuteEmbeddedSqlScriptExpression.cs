using System;
using System.Collections.Generic;
using System.Linq;
using FluentMigrator.Infrastructure;
using System.Reflection;
using System.IO;

namespace FluentMigrator.Expressions
{
    public class ExecuteEmbeddedSqlScriptExpression : MigrationExpressionBase
    {
        public string SqlScript { get; set; }

        public IAssemblyCollection MigrationAssemblies { get; set; }

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


            // since all the Processors are using String.Format() in their Execute method
            //  we need to escape the brackets with double brackets or else it throws an incorrect format error on the String.Format call
            sqlText = sqlText.Replace("{", "{{").Replace("}", "}}");
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

        public override void ApplyConventions(IMigrationConventions conventions)
        {

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
