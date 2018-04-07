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

using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;

namespace FluentMigrator
{
    public abstract class AutoScriptMigration : MigrationBase
    {
        public sealed override void Up()
        {
            var expression = new ExecuteEmbeddedAutoSqlScriptExpression(
                GetType(),
                GetDatabaseNames(),
                MigrationDirection.Up)
            {
                MigrationAssemblies = _context.MigrationAssemblies,
            };
            _context.Expressions.Add(expression);
        }

        public sealed override void Down()
        {
            var expression = new ExecuteEmbeddedAutoSqlScriptExpression(
                GetType(),
                GetDatabaseNames(),
                MigrationDirection.Down)
            {
                MigrationAssemblies = _context.MigrationAssemblies,
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
            ExecuteEmbeddedSqlScriptExpression,
            IAutoNameExpression
        {
            public ExecuteEmbeddedAutoSqlScriptExpression(Type migrationType, IList<string> databaseNames, MigrationDirection direction)
            {
                MigrationType = migrationType;
                DatabaseNames = databaseNames;
                Direction = direction;
            }

            public IList<string> AutoNames { get; set; }
            public AutoNameContext AutoNameContext { get; } = AutoNameContext.EmbeddedResource;
            public Type MigrationType { get; }
            public IList<string> DatabaseNames { get; }
            public MigrationDirection Direction { get; }

            protected override ManifestResourceNameWithAssembly GetQualifiedResourcePath()
            {
                foreach (var sqlScript in AutoNames)
                {
                    var res = FindResourceName(sqlScript);
                    if (res.Length > 1)
                        throw NewNoUniqueResourceException(sqlScript, res);
                    if (res.Length == 1)
                        return res[0];
                }

                var sqlScripts = string.Concat("(", string.Join(",", AutoNames), ")");
                throw NewNotFoundException(sqlScripts);
            }
        }
    }
}
