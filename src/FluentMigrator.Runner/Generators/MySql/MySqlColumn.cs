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
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators.Base;

namespace FluentMigrator.Runner.Generators.MySql
{
    internal class MySqlColumn : ColumnBase
    {
        public MySqlColumn()
            : base(new MySqlTypeMap(), new MySqlQuoter())
        {
        }

        protected override string FormatDefaultValue(ColumnDefinition column)
        {
            if (column.DefaultValue is FunctionValue || column.DefaultValue is SystemMethods)
                throw new DatabaseOperationNotSupportedException(
                    "Sorry, MySql does not support functions as default values ​​for columns");

            return base.FormatDefaultValue(column);
        }

        protected override string FormatIdentity(ColumnDefinition column)
        {
            return column.IsIdentity ? "AUTO_INCREMENT" : string.Empty;
        }

        protected override FunctionValue FormatSystemMethods(SystemMethods systemMethod)
        {
            throw new NotImplementedException();
        }
    }
}