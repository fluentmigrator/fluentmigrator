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
        private readonly IEmbeddedResourceProvider _embeddedResourceProvider;

        [Obsolete]
        protected AutoScriptMigration()
        {
        }

        protected AutoScriptMigration([NotNull] IEmbeddedResourceProvider embeddedResourceProvider)
        {
            _embeddedResourceProvider = embeddedResourceProvider;
        }

        /// <inheritdoc />
        public sealed override void Up()
        {
#pragma warning disable 612
            var expression = new ExecuteEmbeddedAutoSqlScriptExpression(
                _embeddedResourceProvider ?? new DefaultEmbeddedResourceProvider(_context.MigrationAssemblies),
                GetType(),
                GetDatabaseNames(),
                MigrationDirection.Up)
            {
                MigrationAssemblies = _context.MigrationAssemblies,
#pragma warning restore 612
            };

            _context.Expressions.Add(expression);
        }

        /// <inheritdoc />
        public sealed override void Down()
        {
#pragma warning disable 612
            var expression = new ExecuteEmbeddedAutoSqlScriptExpression(
                _embeddedResourceProvider ?? new DefaultEmbeddedResourceProvider(_context.MigrationAssemblies),
                GetType(),
                GetDatabaseNames(),
                MigrationDirection.Down)
            {
                MigrationAssemblies = _context.MigrationAssemblies,
#pragma warning restore 612
            };

            _context.Expressions.Add(expression);
        }

        private IList<string> GetDatabaseNames()
        {
            var dbNames = new List<string>() { _context.QuerySchema.DatabaseType };
            dbNames.AddRange(_context.QuerySchema.DatabaseTypeAliases);
            return dbNames;
        }

        private sealed class ExecuteEmbeddedAutoSqlScriptExpression :
            ExecuteEmbeddedSqlScriptExpressionBase,
            IAutoNameExpression
        {
            [NotNull]
            private readonly IEmbeddedResourceProvider _embeddedResourceProvider;

            public ExecuteEmbeddedAutoSqlScriptExpression([NotNull] IEmbeddedResourceProvider embeddedResourceProvider, Type migrationType, IList<string> databaseNames, MigrationDirection direction)
            {
                _embeddedResourceProvider = embeddedResourceProvider;
                MigrationType = migrationType;
                DatabaseNames = databaseNames;
                Direction = direction;
            }

            [Obsolete]
            public ExecuteEmbeddedAutoSqlScriptExpression(IAssemblyCollection assemblyCollection, Type migrationType, IList<string> databaseNames, MigrationDirection direction)
            {
                _embeddedResourceProvider = new DefaultEmbeddedResourceProvider(assemblyCollection);
                MigrationType = migrationType;
                DatabaseNames = databaseNames;
                Direction = direction;
            }

            public IList<string> AutoNames { get; set; }
            public AutoNameContext AutoNameContext { get; } = AutoNameContext.EmbeddedResource;
            public Type MigrationType { get; }
            public IList<string> DatabaseNames { get; }
            public MigrationDirection Direction { get; }

            /// <summary>
            /// Gets or sets the migration assemblies
            /// </summary>
            [Obsolete]
            [CanBeNull]
            public IAssemblyCollection MigrationAssemblies { get; set; }

            /// <inheritdoc />
            public override void ExecuteWith(IMigrationProcessor processor)
            {
                IReadOnlyCollection<(string name, Assembly Assembly)> resourceNames;
#pragma warning disable 612
                if (MigrationAssemblies != null)
                {
                    resourceNames = MigrationAssemblies.GetManifestResourceNames()
                        .Select(item => (name: item.Name, assembly: item.Assembly))
                        .ToList();
#pragma warning restore 612
                }
                else
                {
                    resourceNames = _embeddedResourceProvider.GetEmbeddedResources().ToList();
                }

                var embeddedResourceNameWithAssembly = GetQualifiedResourcePath(resourceNames, AutoNames.ToArray());
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
                if (AutoNames.Count == 0)
                    errors.Add(ErrorMessages.SqlScriptCannotBeNullOrEmpty);
            }
        }
    }
}
