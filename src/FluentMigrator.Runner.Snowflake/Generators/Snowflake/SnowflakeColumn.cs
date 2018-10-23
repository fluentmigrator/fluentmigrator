#region License
// Copyright (c) 2007-2018, Sean Chambers and the FluentMigrator Project
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

using FluentMigrator.Infrastructure.Extensions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators.Base;
using FluentMigrator.Runner.Processors.Snowflake;
using FluentMigrator.Snowflake;

using JetBrains.Annotations;

namespace FluentMigrator.Runner.Generators.Snowflake
{
    internal class SnowflakeColumn : ColumnBase
    {
        public SnowflakeColumn([NotNull] SnowflakeOptions sfOptions) : base(new SnowflakeTypeMap(), new SnowflakeQuoter(sfOptions.QuoteIdentifiers)) { }

        /// <inheritdoc />
        protected override string FormatIdentity(ColumnDefinition column)
        {
            return column.IsIdentity ? GetIdentityString(column) : string.Empty;
        }

        private static string GetIdentityString(ColumnDefinition column)
        {
            return $"IDENTITY({column.GetAdditionalFeature(SnowflakeExtensions.IdentitySeed, 1)},{column.GetAdditionalFeature(SnowflakeExtensions.IdentityIncrement, 1)})";
        }
    }
}
