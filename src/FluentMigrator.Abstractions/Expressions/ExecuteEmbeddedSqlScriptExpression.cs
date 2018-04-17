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
    public sealed class ExecuteEmbeddedSqlScriptExpression : ExecuteEmbeddedSqlScriptExpressionBase
    {
        /// <summary>
        /// Gets or sets the SQL script name
        /// </summary>
        public string SqlScript { get; set; }

        /// <summary>
        /// Gets or sets the migration assemblies
        /// </summary>
        [Obsolete]
        public IAssemblyCollection MigrationAssemblies { get; set; }

        /// <inheritdoc />
        public override void ExecuteWith(IMigrationProcessor processor)
        {
#pragma warning disable 612
            var resourceNames = MigrationAssemblies.GetManifestResourceNames()
                .Select(item => (name: item.Name, assembly: item.Assembly))
                .ToList();
#pragma warning restore 612

            var embeddedResourceNameWithAssembly = GetQualifiedResourcePath(resourceNames, SqlScript);
            string sqlText;

            using (var stream = embeddedResourceNameWithAssembly
                .assembly.GetManifestResourceStream(embeddedResourceNameWithAssembly.name))
            using (var reader = new StreamReader(stream))
            {
                sqlText = reader.ReadToEnd();
            }

            Execute(processor, sqlText);
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
    }
}
