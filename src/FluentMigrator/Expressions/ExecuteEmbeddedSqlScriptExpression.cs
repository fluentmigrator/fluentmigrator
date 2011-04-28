using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            string embeddedResourceName = GetQualifiedResourcePath(SqlScript);

            if (string.IsNullOrEmpty(embeddedResourceName))
            {
                throw new ArgumentNullException(string.Format("Could find resource named {0} in assembly {1}",SqlScript,MigrationAssembly.FullName));
            }

            using (var stream = MigrationAssembly.GetManifestResourceStream(embeddedResourceName))
            {
                using (var reader = new StreamReader(stream))
                {
                    sqlText = reader.ReadToEnd();
                }
            }

            // since all the Processors are using String.Format() in their Execute method
            //  we need to escape the brackets with double brackets or else it throws an incorrect format error on the String.Format call
            sqlText = sqlText.Replace("{", "{{").Replace("}", "}}");
            processor.Execute(sqlText);
        }

        private string GetQualifiedResourcePath(string resourceName)
        {
            var resources = MigrationAssembly.GetManifestResourceNames();
            var fullManifestPath = resources.Where(x => x.ToLowerInvariant().EndsWith(SqlScript.ToLowerInvariant()));
            return fullManifestPath.FirstOrDefault();
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
