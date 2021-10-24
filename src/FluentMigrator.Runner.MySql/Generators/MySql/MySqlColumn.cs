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

using System;
using System.Linq;

using FluentMigrator.Model;
using FluentMigrator.Runner.Generators.Base;

namespace FluentMigrator.Runner.Generators.MySql
{
    internal class MySqlColumn : ColumnBase
    {
        public MySqlColumn(ITypeMap typeMap, IQuoter quoter)
            : base(typeMap, quoter)
        {
            ClauseOrder.Add(FormatDescription);
        }

        internal string FormatDefaultValue(object defaultValue)
        {
            string formatDefaultValue = base.FormatDefaultValue(new ColumnDefinition { DefaultValue = defaultValue });
            return formatDefaultValue;
        }

        protected string FormatDescription(ColumnDefinition column)
        {
            if (column.ColumnDescriptions.Count > 1)
                throw new InvalidOperationException("Only one description is allowed for the column " + column.Name);

            if (column.ColumnDescriptions.Count == 0)
                return string.Empty;

            return string.Format("COMMENT {0}", Quoter.QuoteValue(column.ColumnDescriptions.First().Value));
        }

        /// <inheritdoc />
        protected override string FormatIdentity(ColumnDefinition column)
        {
            return column.IsIdentity ? "AUTO_INCREMENT" : string.Empty;
        }
    }
}
