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

using FluentMigrator.Builders.Create.Constraint;
using FluentMigrator.Builders.Create.Index;
using FluentMigrator.Builders.Delete.Constraint;
using FluentMigrator.Builders.Delete.Index;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.SqlServer
{
    public static partial class SqlServerExtensions
    {
        /// <summary>
        /// Specifies whether underlying tables and associated indexes are available for queries and data modification during the index operation.
        /// The ONLINE option can only be specified in certain situations, please refer to documentation for SQL Server 2005 and newer.
        /// </summary>
        /// <param name="expression">The expression to use to set the <c>WITH(ONLINE=)</c> option</param>
        /// <param name="active">
        /// <c>true</c>
        /// Long-term table locks are not held. This allows queries or updates to the underlying table to continue.
        /// <c>false</c>
        /// Table locks are applied and the table is unavailable for the duration of the index operation.
        /// </param>
        public static IDeleteIndexOptionsSyntax Online(this IDeleteIndexOptionsSyntax expression, bool active = true)
        {
            var additionalFeatures = expression as ISupportAdditionalFeatures ??
                throw new InvalidOperationException(UnsupportedMethodMessage(nameof(Online), nameof(ISupportAdditionalFeatures)));
            additionalFeatures.AdditionalFeatures[OnlineIndex] = active;
            return expression;
        }

        /// <summary>
        /// Specifies whether underlying tables and associated indexes are available for queries and data modification during the index operation.
        /// The ONLINE option can only be specified in certain situations, please refer to documentation for SQL Server 2005 and newer.
        /// </summary>
        /// <param name="expression">The expression to use to set the <c>WITH(ONLINE=)</c> option</param>
        /// <param name="active">
        /// <c>true</c>
        /// Long-term table locks are not held. This allows queries or updates to the underlying table to continue.
        /// <c>false</c>
        /// Table locks are applied and the table is unavailable for the duration of the index operation.
        /// </param>
        public static IDeleteConstraintInSchemaOptionsSyntax Online(this IDeleteConstraintInSchemaOptionsSyntax expression, bool active = true)
        {
            var additionalFeatures = expression as ISupportAdditionalFeatures ??
                throw new InvalidOperationException(UnsupportedMethodMessage(nameof(Online), nameof(ISupportAdditionalFeatures)));
            additionalFeatures.AdditionalFeatures[OnlineIndex] = active;
            return expression;
        }

        /// <summary>
        /// Specifies whether underlying tables and associated indexes are available for queries and data modification during the index operation.
        /// The ONLINE option can only be specified in certain situations, please refer to documentation for SQL Server 2005 and newer.
        /// </summary>
        /// <param name="expression">The expression to use to set the <c>WITH(ONLINE=)</c> option</param>
        /// <param name="active">
        /// <c>true</c>
        /// Long-term table locks are not held. This allows queries or updates to the underlying table to continue.
        /// <c>false</c>
        /// Table locks are applied and the table is unavailable for the duration of the index operation.
        /// </param>
        public static ICreateIndexOptionsSyntax Online(this ICreateIndexOptionsSyntax expression, bool active = true)
        {
            var additionalFeatures = expression as ISupportAdditionalFeatures ??
                throw new InvalidOperationException(UnsupportedMethodMessage(nameof(Online), nameof(ISupportAdditionalFeatures)));
            additionalFeatures.AdditionalFeatures[OnlineIndex] = active;
            return expression;
        }

        /// <summary>
        /// Specifies whether underlying tables and associated indexes are available for queries and data modification during the index operation.
        /// The ONLINE option can only be specified in certain situations, please refer to documentation for SQL Server 2005 and newer.
        /// </summary>
        /// <param name="expression">The expression to use to set the <c>WITH(ONLINE=)</c> option</param>
        /// <param name="active">
        /// <c>true</c>
        /// Long-term table locks are not held. This allows queries or updates to the underlying table to continue.
        /// <c>false</c>
        /// Table locks are applied and the table is unavailable for the duration of the index operation.
        /// </param>
        public static ICreateConstraintOptionsSyntax Online(this ICreateConstraintOptionsSyntax expression, bool active = true)
        {
            var additionalFeatures = expression as ISupportAdditionalFeatures ??
                throw new InvalidOperationException(UnsupportedMethodMessage(nameof(Online), nameof(ISupportAdditionalFeatures)));
            additionalFeatures.AdditionalFeatures[OnlineIndex] = active;
            return expression;
        }
    }
}
