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

using FluentMigrator.Runner.Generators.Generic;

namespace FluentMigrator.Runner.Generators.SqlServer
{
    public class SqlServerQuoter : GenericQuoter
    {
        public override string OpenQuote { get { return "["; } }

        public override string CloseQuote { get { return "]"; } }

        public override string CloseQuoteEscapeString { get { return "]]"; } }

        public override string OpenQuoteEscapeString { get { return string.Empty; } }

        public override string QuoteSchemaName(string schemaName)
        {
            return (string.IsNullOrEmpty(schemaName)) ? "[dbo]" : Quote(schemaName);
        }

        public override string QuoteValue(object value)
        {
            if (value != null && value is ExplicitUnicodeString)
            {
                return string.Format("N{0}", FormatString(value.ToString()));
            }

            if (value != null && value is SystemMethods)
            {
                switch ((SystemMethods)value)
                {
                    case SystemMethods.NewGuid:
                        return "NEWID()";
                    case SystemMethods.NewSequentialId:
                        return "NEWSEQUENTIALID()";
                    case SystemMethods.CurrentDateTime:
                        return "GETDATE()";
                    case SystemMethods.CurrentUTCDateTime:
                        return "GETUTCDATE()";
                }
            }

            return base.QuoteValue(value);
        }
    }
}
