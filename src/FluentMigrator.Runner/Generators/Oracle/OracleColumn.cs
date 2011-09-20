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
using System.Linq;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators.Base;

namespace FluentMigrator.Runner.Generators.Oracle
{
    internal class OracleColumn : ColumnBase
    {
        private const int OracleObjectNameMaxLength = 30;

        public OracleColumn()
            : base(new OracleTypeMap(), new OracleQuoter())
        {
            int a = ClauseOrder.IndexOf(FormatDefaultValue);
            int b = ClauseOrder.IndexOf(FormatNullable);

            // Oracle requires DefaultValue before nullable
            if (a > b)
            {
                ClauseOrder[b] = FormatDefaultValue;
                ClauseOrder[a] = FormatNullable;
            }
        }

        protected override string FormatIdentity(ColumnDefinition column)
        {
            if (column.IsIdentity)
            {
                throw new DatabaseOperationNotSupportedException("Oracle does not support identity columns. Please use a SEQUENCE instead");
            }
            return string.Empty;
        }

        protected override FunctionValue FormatSystemMethods(SystemMethods systemMethod)
        {
            switch (systemMethod)
            {
                case SystemMethods.NewGuid:
                    return new FunctionValue("sys_guid()");
                case SystemMethods.CurrentDateTime:
                    return new FunctionValue("sysdate");
            }

            throw new NotImplementedException();
        }

        protected override string GetPrimaryKeyConstraintName(IEnumerable<ColumnDefinition> primaryKeyColumns, string tableName)
        {
            if (primaryKeyColumns == null)
                throw new ArgumentNullException("primaryKeyColumns");
            if (tableName == null)
                throw new ArgumentNullException("tableName");

            var primaryKeyName = primaryKeyColumns.First().PrimaryKeyName;

            if (string.IsNullOrEmpty(primaryKeyName))
            {
                return string.Empty;
            }

            if (primaryKeyName.Length > OracleObjectNameMaxLength)
                throw new DatabaseOperationNotSupportedException(
                    string.Format(
                        "Oracle does not support length of primary key name greater than {0} characters. Reduce length of primary key name. ({1})",
                        OracleObjectNameMaxLength, primaryKeyName));

            var result = string.Format("CONSTRAINT {0} ", Quoter.QuoteIndexName(primaryKeyName));
            return result;
        }
    }
}