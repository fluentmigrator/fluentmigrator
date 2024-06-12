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
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;

using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;

using JetBrains.Annotations;

namespace FluentMigrator
{
    /// <summary>
    /// Migration that automatically uses embedded SQL scripts depending on the database type name
    /// </summary>
    /// <remarks>
    /// The embedded SQL scripts must end in <c>Scripts.{Direction}.{Version}_{DerivedTypeName}_{DatabaseType}.sql</c>.
    /// <para>The <c>{Direction}</c> can be <c>Up</c> or <c>Down</c>.</para>
    /// <para>The <c>{Version}</c> is the migration version.</para>
    /// <para>The <c>{DerivedTypeName}</c> is the name of the type derived from <see cref="AutoScriptMigration"/>.</para>
    /// <para>The <c>{DatabaseType}</c> is the database type name. For SQL Server 2016, the variants <c>SqlServer2016</c>,
    /// <c>SqlServer</c>, and <c>Generic</c> will be tested.</para>
    /// <para>The behavior may be overriden by providing a custom <c>FluentMigrator.Runner.Conventions.IAutoNameConvention</c>.</para>
    /// </remarks>
    public abstract class AutoScriptMigration : MigrationBase
    {
        [CanBeNull]
        private readonly IReadOnlyCollection<IEmbeddedResourceProvider> _embeddedResourceProviders;

        [CanBeNull]
        private IReadOnlyCollection<IEmbeddedResourceProvider> _providers;

        protected AutoScriptMigration([NotNull] IEnumerable<IEmbeddedResourceProvider> embeddedResourceProviders)
        {
            _embeddedResourceProviders = embeddedResourceProviders.ToList();
        }

        /// <inheritdoc />
        public sealed override void Up()
        {
            var expression = new ExecuteEmbeddedAutoSqlScriptExpression(
                GetProviders(),
                GetType(),
                GetDatabaseNames(),
                MigrationDirection.Up);

            Context.Expressions.Add(expression);
        }

        /// <inheritdoc />
        public sealed override void Down()
        {
            var expression = new ExecuteEmbeddedAutoSqlScriptExpression(
                GetProviders(),
                GetType(),
                GetDatabaseNames(),
                MigrationDirection.Down);

            Context.Expressions.Add(expression);
        }

        private IList<string> GetDatabaseNames()
        {
            var dbNames = new List<string>();
            if (Context.QuerySchema != null)
            {
                dbNames.Add(Context.QuerySchema.DatabaseType);
                dbNames.AddRange(Context.QuerySchema.DatabaseTypeAliases);
            }

            return dbNames;
        }

        [NotNull]
        private IReadOnlyCollection<IEmbeddedResourceProvider> GetProviders()
        {
            if (_providers != null)
                return _providers;

            if (_embeddedResourceProviders != null)
            {
                return _providers = _embeddedResourceProviders;
            }

            var providers = new List<IEmbeddedResourceProvider>();

            return _providers = providers;
        }

        private sealed class ExecuteEmbeddedAutoSqlScriptExpression :
            ExecuteEmbeddedSqlScriptExpressionBase,
            IAutoNameExpression,
            IValidatableObject
        {
            [NotNull]
            private readonly IReadOnlyCollection<IEmbeddedResourceProvider> _embeddedResourceProviders;

            public ExecuteEmbeddedAutoSqlScriptExpression([NotNull] IEnumerable<IEmbeddedResourceProvider> embeddedResourceProviders, Type migrationType, IList<string> databaseNames, MigrationDirection direction)
            {
                _embeddedResourceProviders = embeddedResourceProviders.ToList();
                MigrationType = migrationType;
                DatabaseNames = databaseNames;
                Direction = direction;
            }

            public IList<string> AutoNames { get; set; }
            public AutoNameContext AutoNameContext { get; } = AutoNameContext.EmbeddedResource;
            public Type MigrationType { get; }
            public IList<string> DatabaseNames { get; }
            public MigrationDirection Direction { get; }

            public override void ExecuteWith(IMigrationProcessor processor)
            {
                IReadOnlyCollection<(string name, Assembly Assembly)> resourceNames;

                resourceNames = _embeddedResourceProviders
                    .SelectMany(x => x.GetEmbeddedResources())
                    .Distinct()
                    .ToList();

                var embeddedResourceNameWithAssembly = GetQualifiedResourcePath(resourceNames, AutoNames.ToArray());
                string sqlText;

                var stream = embeddedResourceNameWithAssembly
                    .assembly.GetManifestResourceStream(embeddedResourceNameWithAssembly.name);
                if (stream == null)
                {
                    throw new InvalidOperationException(
                        $"The resource {embeddedResourceNameWithAssembly.name} couldn't be found in {embeddedResourceNameWithAssembly.assembly.FullName}");
                }

                using (stream)
                using (var reader = new StreamReader(stream))
                {
                    sqlText = reader.ReadToEnd();
                }

                Execute(processor, sqlText);
            }

            /// <inheritdoc />
            public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            {
                if (AutoNames.Count == 0)
                    yield return new ValidationResult(ErrorMessages.SqlScriptCannotBeNullOrEmpty);
            }
        }
    }
}
