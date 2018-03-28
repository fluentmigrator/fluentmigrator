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

using FluentMigrator.Model;
using FluentMigrator.Runner.Generators.Base;

namespace FluentMigrator.Runner.Generators.SqlAnywhere
{
    internal class SqlAnywhereColumn : ColumnBase
    {
        public SqlAnywhereColumn(ITypeMap typeMap)
            : base(typeMap, new SqlAnywhereQuoter())
        {
            // Add UNIQUE before IDENTITY and after PRIMARY KEY
            ClauseOrder.Insert(ClauseOrder.Count - 2, FormatUniqueConstraint);
        }

        protected override string FormatNullable(ColumnDefinition column)
        {
            if (column.IsNullable.HasValue && column.IsNullable.Value)
            {
                return "NULL";
            }
            else
            {
                return "NOT NULL";
            }
        }

        protected override string FormatDefaultValue(ColumnDefinition column)
        {
            if (DefaultValueIsSqlFunction(column.DefaultValue))
                return "DEFAULT " + column.DefaultValue;

            var defaultValue = base.FormatDefaultValue(column);

            if (!string.IsNullOrEmpty(defaultValue))
                return defaultValue;

            return string.Empty;
        }

        private static bool DefaultValueIsSqlFunction(object defaultValue)
        {
            return defaultValue is string && defaultValue.ToString().EndsWith("()");
        }

        protected virtual string FormatUniqueConstraint(ColumnDefinition column)
        {
            // Define unique constraints on columns in addition to creating a unique index
            return column.IsUnique ? "UNIQUE" : string.Empty;
        }

        protected override string FormatIdentity(ColumnDefinition column)
        {
            return column.IsIdentity ? GetIdentityString() : string.Empty;
        }

        private static string GetIdentityString()
        {
            return "DEFAULT AUTOINCREMENT";
        }

        protected override string FormatSystemMethods(SystemMethods systemMethod)
        {
            switch (systemMethod)
            {
                case SystemMethods.NewGuid:
                    return "NEWID()";
                case SystemMethods.NewSequentialId:
                    return "NEWSEQUENTIALID()";
                case SystemMethods.CurrentDateTime:
                    return "GETDATE()";
                case SystemMethods.CurrentUTCDateTime:
                    return "GETUTCDATE()";
                case SystemMethods.CurrentUser:
                    return "CURRENT_USER";
            }

            return null;
        }

        public string FormatDefaultValue(object defaultValue)
        {
            if (DefaultValueIsSqlFunction(defaultValue))
                return defaultValue.ToString();

            if (defaultValue is SystemMethods methods)
                return FormatSystemMethods(methods);

            return Quoter.QuoteValue(defaultValue);
        }

        public static string GetDefaultConstraintName(string tableName, string columnName)
        {
            return $"DF_{tableName}_{columnName}";
        }
    }
}
