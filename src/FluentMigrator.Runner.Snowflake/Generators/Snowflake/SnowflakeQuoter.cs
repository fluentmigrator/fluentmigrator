#region License
//
// Copyright (c) 2018, Fluent Migrator Project
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
using System.Globalization;

using FluentMigrator.Runner.Generators.Generic;
using FluentMigrator.Runner.Processors.Snowflake;

namespace FluentMigrator.Runner.Generators.Snowflake
{
    /// <summary>
    /// The Snowflake SQL quoter for FluentMigrator.
    /// </summary>
    public class SnowflakeQuoter : GenericQuoter
    {
        private readonly bool _quoteIdentifiers;

        /// <inheritdoc />
        public SnowflakeQuoter(SnowflakeOptions sfOptions) : this(sfOptions.QuoteIdentifiers) { }

        /// <inheritdoc />
        public SnowflakeQuoter(bool quoteIdentifiers)
        {
            _quoteIdentifiers = quoteIdentifiers;
        }

        /// <inheritdoc />
        /// <summary>
        /// If quoting is disabled, returns empty string.
        /// </summary>
        public override string OpenQuote => _quoteIdentifiers ? base.OpenQuote : string.Empty;

        /// <inheritdoc />
        /// <summary>
        /// If quoting is disabled, returns empty string.
        /// </summary>
        public override string OpenQuoteEscapeString => _quoteIdentifiers ? base.OpenQuoteEscapeString : string.Empty;

        /// <inheritdoc />
        /// <summary>
        /// If quoting is disabled, returns empty string.
        /// </summary>
        public override string CloseQuote => _quoteIdentifiers ? base.OpenQuote : string.Empty;

        /// <inheritdoc />
        /// <summary>
        /// If quoting is disabled, returns empty string.
        /// </summary>
        public override string CloseQuoteEscapeString => _quoteIdentifiers ? base.CloseQuoteEscapeString : string.Empty;

        /// <inheritdoc />
        public override bool IsQuoted(string name) => _quoteIdentifiers && base.IsQuoted(name);

        /// <inheritdoc />
        public override string QuoteSchemaName(string schemaName)
        {
            return base.QuoteSchemaName(schemaName ?? DefaultSchemaName);
        }

        /// <inheritdoc />
        public string DefaultSchemaName => "PUBLIC";

        /// <inheritdoc />
        public override string FormatDateTime(DateTime value)
        {
            return ValueQuote + value.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) + ValueQuote;
        }

        /// <inheritdoc />
        public override string FormatDateTimeOffset(DateTimeOffset value)
        {
            return ValueQuote + value.ToString("yyyy-MM-dd HH:mm:ss zzz", CultureInfo.InvariantCulture) + ValueQuote;
        }

        /// <inheritdoc />
        public override string FormatSystemMethods(SystemMethods value)
        {
            switch (value)
            {
                case SystemMethods.NewGuid:
                    return "UUID_STRING()";
                case SystemMethods.CurrentDateTimeOffset:
                case SystemMethods.CurrentDateTime:
                    return "CURRENT_TIMESTAMP()";
                case SystemMethods.CurrentUTCDateTime:
                    return "SYSDATE()";
                case SystemMethods.CurrentUser:
                    return "CURRENT_USER()";
                default:
                    return base.FormatSystemMethods(value);
            }
        }
    }
}
