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

namespace FluentMigrator.Runner.Conventions
{
    /// <summary>
    /// The default implementation of a <see cref="IAutoNameConvention"/>
    /// </summary>
    public class DefaultAutoNameConvention : IAutoNameConvention
    {
        /// <inheritdoc />
        public IAutoNameExpression Apply(IAutoNameExpression expression)
        {
            if (expression.AutoNameContext != AutoNameContext.EmbeddedResource)
            {
                return expression;
            }

            var dbTypeNames = new List<string>(expression.DatabaseNames) { "Generic" };
            if (expression.Direction == MigrationDirection.Up)
            {
                expression.AutoNames = GetAutoScriptUpNameImpl(expression.MigrationType, dbTypeNames).ToList();
            }
            else
            {
                expression.AutoNames = GetAutoScriptDownNameImpl(expression.MigrationType, dbTypeNames).ToList();
            }

            return expression;
        }

        private static IEnumerable<string> GetAutoScriptUpNameImpl(Type type, IEnumerable<string> databaseTypes)
        {
            var migrationAttribute = type
                .GetCustomAttributes(typeof(MigrationAttribute), false)
                .OfType<MigrationAttribute>()
                .FirstOrDefault();
            if (migrationAttribute != null)
            {
                var version = migrationAttribute.VersionAsString;
                foreach (var databaseType in databaseTypes)
                {
                    yield return string.Format("Scripts.Up.{0}_{1}_{2}.sql"
                        , version
                        , type.Name
                        , databaseType);
                }
            }
        }

        private static IEnumerable<string> GetAutoScriptDownNameImpl(Type type, IEnumerable<string> databaseTypes)
        {
            var migrationAttribute = type
                .GetCustomAttributes(typeof(MigrationAttribute), false)
                .OfType<MigrationAttribute>()
                .FirstOrDefault();
            if (migrationAttribute != null)
            {
                var version = migrationAttribute.VersionAsString;
                foreach (var databaseType in databaseTypes)
                {
                    yield return string.Format("Scripts.Down.{0}_{1}_{2}.sql"
                        , version
                        , type.Name
                        , databaseType);
                }
            }
        }
    }
}
