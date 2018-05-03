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
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.IO;

using FluentMigrator.Infrastructure;

using JetBrains.Annotations;

namespace FluentMigrator.Expressions
{
    /// <summary>
    /// Expression to execute an embedded SQL script
    /// </summary>
    public sealed class ExecuteEmbeddedSqlScriptExpression : ExecuteEmbeddedSqlScriptExpressionBase
    {
        [NotNull]
        private readonly IEmbeddedResourceProvider _embeddedResourceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecuteEmbeddedSqlScriptExpression"/> class.
        /// </summary>
        /// <param name="embeddedResourceProvider">The embedded resource provider</param>
        public ExecuteEmbeddedSqlScriptExpression([NotNull] IEmbeddedResourceProvider embeddedResourceProvider)
        {
            _embeddedResourceProvider = embeddedResourceProvider;
        }

        /// <summary>
        /// Gets or sets the SQL script name
        /// </summary>
        [Required(ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = nameof(ErrorMessages.SqlScriptCannotBeNullOrEmpty))]
        public string SqlScript { get; set; }

        /// <inheritdoc />
        public override void ExecuteWith(IMigrationProcessor processor)
        {
            var resourceNames = _embeddedResourceProvider.GetEmbeddedResources().ToList();
            var embeddedResourceNameWithAssembly = GetQualifiedResourcePath(resourceNames, SqlScript);
            string sqlText;

            var stream = embeddedResourceNameWithAssembly
                .assembly.GetManifestResourceStream(embeddedResourceNameWithAssembly.name);
            if (stream == null)
            {
                throw new InvalidOperationException(
                    $"The ressource {embeddedResourceNameWithAssembly.name} couldn't be found in {embeddedResourceNameWithAssembly.assembly.FullName}");
            }

            using (stream)
            using (var reader = new StreamReader(stream))
            {
                sqlText = reader.ReadToEnd();
            }

            Execute(processor, sqlText);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return base.ToString() + SqlScript;
        }
    }
}
