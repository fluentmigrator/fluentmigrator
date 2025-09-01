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

using System.Collections.Generic;

using FluentMigrator.Model;
using FluentMigrator.Runner.Generators.Base;
using System.Linq;
using System;

namespace FluentMigrator.Runner.Generators.MySql
{
    internal class MySqlColumn : ColumnBase<IMySqlTypeMap>
    {
        public MySqlColumn(IMySqlTypeMap typeMap, IQuoter quoter)
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
            if (string.IsNullOrEmpty(column.ColumnDescription))
                return string.Empty;

            if (column.AdditionalColumnDescriptions.Count == 0)
                return string.Format("COMMENT {0}", Quoter.QuoteValue("Description:"+column.ColumnDescription));

            var descriptionsList = new List<string>
            {
                string.Format("Description:" + column.ColumnDescription)
            };
            descriptionsList.AddRange(from descriptionItem in column.AdditionalColumnDescriptions
                                      select descriptionItem.Key + ":" + descriptionItem.Value);
            return string.Format("COMMENT {0}", Quoter.QuoteValue(string.Join(Environment.NewLine, descriptionsList)));
        }

        /// <inheritdoc />
        protected override string FormatIdentity(ColumnDefinition column)
        {
            return column.IsIdentity ? "AUTO_INCREMENT" : string.Empty;
        }

        /// <inheritdoc />
        protected override string FormatExpression(ColumnDefinition column)
        {
            return column.Expression == null ? null : $"GENERATED ALWAYS AS ({column.Expression}){(column.ExpressionStored ? " STORED" : " VIRTUAL")}";
        }
    }
}
