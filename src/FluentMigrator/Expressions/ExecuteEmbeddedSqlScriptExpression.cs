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

        public Assembly MigrationAssembly { get; set; }

        public override void ExecuteWith(IMigrationProcessor processor)
        {

            string sqlText;
            string embeddedResourceName = GetQualifiedResourcePath();

            using (var stream = MigrationAssembly.GetManifestResourceStream(embeddedResourceName))
            using (var reader = new StreamReader(stream))
            {
                sqlText = reader.ReadToEnd();
            }

            processor.Process(sqlText, this);
        }

        private string GetQualifiedResourcePath()
        {
            var resources = MigrationAssembly.GetManifestResourceNames();

            //resource full name is in format `namespace.resourceName`
            var sqlScriptParts = SqlScript.Split('.').Reverse().ToArray();
            Func<string, bool> isNameMatch = x => x.Split('.').Reverse().Take(sqlScriptParts.Length).SequenceEqual(sqlScriptParts, StringComparer.InvariantCultureIgnoreCase);

            string result = null;
            var foundResources = resources.Where(isNameMatch).ToArray();

            if (foundResources.Length == 0)
                throw new InvalidOperationException(string.Format("Could not find resource named {0} in assembly {1}", SqlScript, MigrationAssembly.FullName));

            if (foundResources.Length > 1)
                throw new InvalidOperationException(string.Format(@"Could not find unique resource named {0} in assembly {1}.
Possible candidates are:
 
{2}
", SqlScript, MigrationAssembly.FullName, string.Join(Environment.NewLine + "\t", foundResources)));

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
