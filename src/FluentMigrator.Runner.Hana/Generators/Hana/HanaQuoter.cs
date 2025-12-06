#region License
// Copyright (c) 2018, Fluent Migrator Project
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

using System.Globalization;

using FluentMigrator.Runner.Generators.Generic;

namespace FluentMigrator.Runner.Generators.Hana
{
    /// <summary>
    /// The SAP HANA SQL quoter for FluentMigrator.
    /// </summary>
    public class HanaQuoter : GenericQuoter
    {
        /// <inheritdoc />
        public override string FormatNationalString(string value)
        {
            return $"N{FormatAnsiString(value)}";
        }

        /// <inheritdoc />
        public override string FormatSystemMethods(SystemMethods value)
        {
            switch (value)
            {
                case SystemMethods.CurrentDateTime:
                    return "CURRENT_TIMESTAMP";
                case SystemMethods.CurrentUTCDateTime:
                    return "CURRENT_UTCTIMESTAMP";
            }

            return base.FormatSystemMethods(value);
        }

        /// <inheritdoc />
        public override string QuoteSchemaName(string schemaName)
        {
            return string.Empty;
        }

        /// <inheritdoc />
        public override string QuoteValue(object value)
        {
            if (value is bool boolean)
            {
                return boolean.ToString(CultureInfo.InvariantCulture).ToUpperInvariant();
            }

            return base.QuoteValue(value);
        }
    }
}
