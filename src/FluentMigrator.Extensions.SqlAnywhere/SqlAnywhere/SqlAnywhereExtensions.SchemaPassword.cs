#region License
// Copyright (c) 2007-2018, FluentMigrator Project
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

using FluentMigrator.Builders.Create.Schema;
using FluentMigrator.Infrastructure;
using FluentMigrator.Infrastructure.Extensions;

namespace FluentMigrator.SqlAnywhere
{
    /// <summary>
    /// Extension methods for SQL Anywhere
    /// </summary>
    public static partial class SqlAnywhereExtensions
    {
        /// <summary>
        /// Sets the schema password used during schema creation
        /// </summary>
        /// <param name="expression">The schema creation expression</param>
        /// <param name="password">The password to use</param>
        /// <returns>The next step</returns>
        public static ICreateSchemaOptionsSyntax Password(this ICreateSchemaOptionsSyntax expression, string password)
        {
            var additionalFeatures = expression as ISupportAdditionalFeatures ??
                throw new InvalidOperationException(UnsupportedMethodMessage(nameof(Password), nameof(ISupportAdditionalFeatures)));
            additionalFeatures.SetAdditionalFeature(SchemaPassword, password);
            return expression;
        }
    }
}
