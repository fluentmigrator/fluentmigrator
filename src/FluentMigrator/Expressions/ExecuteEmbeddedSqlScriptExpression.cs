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

        public override void ExecuteWith(IMigrationProcessor processor)
        {
            string sqlText;
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(SqlScript))
            {
                using (var reader = new StreamReader(stream))
                {
                    sqlText = reader.ReadToEnd();
                }
            }
            processor.Execute(sqlText);
        }

        public override void ApplyConventions(IMigrationConventions conventions)
        {
            var resources = Assembly.GetExecutingAssembly().GetManifestResourceNames();
            var fullManifestPath = resources.SkipWhile(x => !x.EndsWith(SqlScript));           
            SqlScript = fullManifestPath.FirstOrDefault();
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
