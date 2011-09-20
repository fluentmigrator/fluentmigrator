#region License
// 
// Copyright (c) 2007-2009, Sean Chambers <schambers80@gmail.com>
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
using System.Data;
using System.Linq;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators.Base;

namespace FluentMigrator.Runner.Generators.SQLite
{
    internal class SqliteColumn : ColumnBase
    {
        public SqliteColumn()
            : base(new SqliteTypeMap(), new SqliteQuoter())
        {
        }

        protected override string FormatDefaultValue(ColumnDefinition column)
        {
            if (column == null)
                throw new ArgumentNullException("column");

            if (column.DefaultValue != null && column.DefaultValue is SystemMethods && (SystemMethods)column.DefaultValue == SystemMethods.CurrentDateTime)
            {
                var defaultValue = "DEFAULT " + FormatSystemMethods(SystemMethods.CurrentDateTime);
                return defaultValue;
            }

            return base.FormatDefaultValue(column);
        }

        protected override string FormatIdentity(ColumnDefinition column)
        {
            //SQLite only supports the concept of Identity in combination with a single primary key
            //see: http://www.sqlite.org/syntaxdiagrams.html#column-constraint syntax details
            if (column.IsIdentity && !column.IsPrimaryKey && column.Type != DbType.Int32)
            {
                throw new ArgumentException("SQLite only supports identity on single integer, primary key coulmns");
            }
            return string.Empty;
        }

        public override bool ShouldPrimaryKeysBeAddedSeparatley(IEnumerable<ColumnDefinition> primaryKeyColumns)
        {
            //If there are no identity column then we can add as a separate constrint
            var result = !primaryKeyColumns.Any(x => x.IsIdentity) && primaryKeyColumns.Any(x => x.IsPrimaryKey);
            return result;
        }

        protected override string FormatPrimaryKey(ColumnDefinition column)
        {
            if (!column.IsPrimaryKey)
                return string.Empty;

            var result = column.IsIdentity ? "PRIMARY KEY AUTOINCREMENT" : string.Empty;
            return result;
        }

        protected override ExpressionString FormatSystemMethods(SystemMethods systemMethod)
        {
            switch (systemMethod)
            {
                case SystemMethods.CurrentDateTime:
                    return "CURRENT_TIMESTAMP";
                case SystemMethods.NewGuid:
                    throw new DatabaseOperationNotSupportedException(
                        "Sorry, SQLite does not support GUID as default values ​​for columns");
            }

            throw new NotImplementedException();
        }
    }
}