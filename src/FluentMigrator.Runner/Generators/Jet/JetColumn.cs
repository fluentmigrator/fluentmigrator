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

namespace FluentMigrator.Runner.Generators.Jet
{
    internal class JetColumn : ColumnBase
    {
        public JetColumn()
            : base(new JetTypeMap(), new JetQuoter())
        {
        }

        protected override string FormatType(ColumnDefinition column)
        {
            if (column.IsIdentity)
            {
                // In Jet an identity column always of type COUNTER which is a integer type
                if (column.Type != DbType.Int32)
                {
                    throw new ArgumentException("Jet Engine only allows identity columns on integer columns");
                }

                return "COUNTER";
            }
            return base.FormatType(column);
        }

        protected override string FormatDefaultValue(ColumnDefinition column)
        {
            if (column == null)
                throw new ArgumentNullException("column");

            if (column.DefaultValue is FunctionValue || column.DefaultValue is SystemMethods)
                throw new DatabaseOperationNotSupportedException(
                    "Sorry, Jet does not support function correctly as the default values ​​for columns");

            return base.FormatDefaultValue(column);
        }

        protected override string FormatIdentity(ColumnDefinition column)
        {
            //Indentity type is handled by FormartType
            return string.Empty;
        }

        protected override FunctionValue FormatSystemMethods(SystemMethods systemMethod)
        {
            throw new NotImplementedException();
        }
    }
}