#region License
//
// Copyright (c) 2018, Fluent Migrator Project
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

using System;
using System.Collections.Generic;
using System.Linq;

using FluentMigrator.Model;
using FluentMigrator.Runner.Generators.Base;
using FluentMigrator.Runner.Processors.Firebird;

using JetBrains.Annotations;

namespace FluentMigrator.Runner.Generators.Firebird
{
    internal class FirebirdColumn : ColumnBase<IFirebirdTypeMap>
    {
        public FirebirdColumn([NotNull] FirebirdOptions fbOptions) : base(new FirebirdTypeMap(), new FirebirdQuoter(fbOptions.ForceQuote))
        {
            FBOptions = fbOptions;

            //In firebird DEFAULT clause precedes NULLABLE clause
            ClauseOrder = new List<Func<ColumnDefinition, string>> { FormatString, FormatType, FormatExpression, FormatDefaultValue, FormatNullable, FormatPrimaryKey, FormatIdentity };
        }

        protected FirebirdOptions FBOptions { get; }

        protected override string FormatIdentity(ColumnDefinition column)
        {
            //Identity not supported
           return string.Empty;
        }

        protected override string GetPrimaryKeyConstraintName(IEnumerable<ColumnDefinition> primaryKeyColumns, string tableName)
        {
            string primaryKeyName = primaryKeyColumns.Select(x => x.PrimaryKeyName).FirstOrDefault();

            if (string.IsNullOrEmpty(primaryKeyName))
            {
                return string.Empty;
            }
            else if (primaryKeyName.Length > FirebirdOptions.MaxNameLength)
            {
                if (!FBOptions.TruncateLongNames)
                    throw new ArgumentException($"Name too long: {primaryKeyName}");
                primaryKeyName = primaryKeyName.Substring(0, Math.Min(FirebirdOptions.MaxNameLength, primaryKeyName.Length));
            }

            return $"CONSTRAINT {Quoter.QuoteIndexName(primaryKeyName)} ";
        }

        public virtual string GenerateForTypeAlter(ColumnDefinition column)
        {
            return FormatType(column);
        }

        public virtual string GenerateForDefaultAlter(ColumnDefinition column)
        {
            return FormatDefaultValue(column);
        }

        /// <inheritdoc/>
        protected override string FormatExpression(ColumnDefinition column)
        {
            return column.Expression == null ? null : $"GENERATED ALWAYS AS ({column.Expression})";
        }

        /// <inheritdoc />
        protected override string FormatNullable(ColumnDefinition column)
        {
            if (column.Expression == null)
            {
                return base.FormatNullable(column);
            }

            // In Firebird, computed columns seem not to allow NULL/NOT NULL specification
            return string.Empty;
        }
    }
}
