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
using System.Data;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators.Base;
using System.Collections.Generic;
using System.Linq;

namespace FluentMigrator.Runner.Generators.SQLite
{
    internal class SqliteColumn : ColumnBase
    {
        public SqliteColumn()
            : base(new SqliteTypeMap(), new SqliteQuoter())
        {
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
            if (!primaryKeyColumns.Any(x => x.IsIdentity) && primaryKeyColumns.Any(x => x.IsPrimaryKey)) return true;
            return false;
        }

        protected override string FormatPrimaryKey(ColumnDefinition column)
        {
            if (!column.IsPrimaryKey) return string.Empty;

            return column.IsIdentity ? "PRIMARY KEY AUTOINCREMENT" : string.Empty;
        }

        protected override FunctionValue FormatSystemMethods(SystemMethods systemMethod)
        {
            switch (systemMethod)
            {
                case SystemMethods.CurrentDateTime:
                    return "CURRENT_TIMESTAMP";
            }

            throw new NotImplementedException();
        }
    }
}