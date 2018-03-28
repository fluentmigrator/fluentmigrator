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

using FluentMigrator.Runner.Generators.Generic;

namespace FluentMigrator.Runner.Generators.SqlAnywhere
{
    public class SqlAnywhereQuoter : GenericQuoter
    {
        public override string FormatEnum(object value)
        {
            if (value is SystemMethods methods)
            {
                switch(methods)
                {
                    case SystemMethods.NewGuid:
                        return "NEWID()";
                    case SystemMethods.NewSequentialId:
                        return "AUTOINCREMENT";
                    case SystemMethods.CurrentDateTime:
                        return "TIMESTAMP";
                    case SystemMethods.CurrentUTCDateTime:
                        return "UTC TIMESTAMP";
                    case SystemMethods.CurrentUser:
                        return "LAST USER";
                    default:
                        throw new NotImplementedException("FormatEnum not implemented for SystemMethods." + methods.ToString());
                }
            }

            return base.FormatEnum(value);
        }

        public override string OpenQuote => "[";

        public override string CloseQuote => "]";

        public override string CloseQuoteEscapeString => "]]";

        public override string OpenQuoteEscapeString => string.Empty;

        public override string QuoteSchemaName(string schemaName)
        {
            return (string.IsNullOrEmpty(schemaName)) ? "[dbo]" : Quote(schemaName);
        }

        public override string QuoteValue(object value)
        {
            if (value is ExplicitUnicodeString)
            {
                return $"N{FormatString(value.ToString())}";
            }

            return base.QuoteValue(value);
        }
    }
}
