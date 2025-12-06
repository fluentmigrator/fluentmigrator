#region License
// Copyright (c) 2007-2024, Fluent Migrator Project
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

using FluentMigrator.Generation;
using FluentMigrator.Infrastructure.Extensions;
using FluentMigrator.Model;
using FluentMigrator.Oracle;

namespace FluentMigrator.Runner.Generators.Oracle
{
    internal class Oracle12CColumn : OracleColumn
    {
        protected override int OracleObjectNameMaxLength => 128;

        /// <inheritdoc />
        public Oracle12CColumn(IQuoter quoter) : base(quoter)
        {
        }

        private static string GetIdentityString(ColumnDefinition column)
        {
            string generationType;
            switch (column.GetAdditionalFeature(OracleExtensions.IdentityGeneration, OracleGenerationType.Always))
            {
                case OracleGenerationType.Always:
                    generationType = "ALWAYS";
                    break;
                case OracleGenerationType.ByDefault:
                    generationType = "BY DEFAULT";
                    break;
                case OracleGenerationType.ByDefaultOnNull:
                    generationType = "BY DEFAULT ON NULL";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var options = new List<string>();

            var startWith = column.GetAdditionalFeature(OracleExtensions.IdentityStartWith, (long?)null);
            if (startWith != null)
            {
                options.Add($"START WITH {startWith.Value:D}");
            }

            var incrementBy = column.GetAdditionalFeature(OracleExtensions.IdentityIncrementBy, (int?)null);
            if (incrementBy != null)
            {
                options.Add($"INCREMENT BY {incrementBy.Value:D}");
            }

            var minValue = column.GetAdditionalFeature(OracleExtensions.IdentityMinValue, (long?)null);
            if (minValue != null)
            {
                options.Add($"MINVALUE {minValue.Value:D}");
            }

            var maxValue = column.GetAdditionalFeature(OracleExtensions.IdentityMaxValue, (long?)null);
            if (maxValue != null)
            {
                options.Add($"MAXVALUE {maxValue.Value:D}");
            }

            var optionsString = "";
            if (options.Count > 0)
            {
                optionsString = $" ({string.Join(" ", options)})";
            }

            return $"GENERATED {generationType} AS IDENTITY{optionsString}";
        }

        /// <inheritdoc />
        protected override string FormatIdentity(ColumnDefinition column)
        {
            return column.IsIdentity ? GetIdentityString(column) : string.Empty;
        }

        /// <inheritdoc />
        protected override string FormatNullable(ColumnDefinition column)
        {
            //Creates always return Not Null unless is nullable is true
            if (column.ModificationType == ColumnModificationType.Create)
            {
                if (column.IsNullable.HasValue && column.IsNullable.Value || column.IsIdentity)
                {
                    return string.Empty;
                }
                else
                {
                    return "NOT NULL";
                }
            }

            //alter only returns "Not Null" if IsNullable is explicitly set
            if (column.IsNullable.HasValue)
            {
                return column.IsNullable.Value ? "NULL" : "NOT NULL";
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
