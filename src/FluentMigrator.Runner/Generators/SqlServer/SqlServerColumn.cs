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

namespace FluentMigrator.Runner.Generators.SqlServer
{
    internal class SqlServerColumn : ColumnBase
    {
        public SqlServerColumn(ITypeMap typeMap)
            : base(typeMap, new SqlServerQuoter())
        {
        }

        protected override string FormatDefaultValue(ColumnDefinition column)
        {
            var defaultValue = base.FormatDefaultValue(column);

            if (column.DefaultValue is ExpressionString || column.DefaultValue is SystemMethods)
                return defaultValue;

            if (!string.IsNullOrEmpty(defaultValue))
                return string.Format("CONSTRAINT DF_{0}_{1} ", column.TableName.ToUpper(), column.Name.ToUpper()) +
                       defaultValue;

            return string.Empty;
        }

        protected override string FormatIdentity(ColumnDefinition column)
        {
            return column.IsIdentity ? "IDENTITY(1,1)" : string.Empty;
        }

        protected override ExpressionString FormatSystemMethods(SystemMethods systemMethod)
        {
            switch (systemMethod)
            {
                case SystemMethods.NewGuid:
                    return new ExpressionString("NEWID()");
                case SystemMethods.CurrentDateTime:
                    return new ExpressionString("GETDATE()");
            }

            throw new NotImplementedException();
        }
    }
}