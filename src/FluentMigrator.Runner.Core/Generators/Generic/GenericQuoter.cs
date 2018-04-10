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

namespace FluentMigrator.Runner.Generators.Generic
{
    using System;

    public class GenericQuoter : IQuoter
    {
        public virtual string QuoteValue(object value)
        {
            switch (value)
            {
                case null:
                case DBNull _:
                    return FormatNull();
#pragma warning disable 618
                case ExplicitUnicodeString _:
#pragma warning restore 618
                case string _:
                    return FormatNationalString(value.ToString());
                case NonUnicodeString _:
                    return FormatAnsiString(value.ToString());
                case char v:
                    return FormatChar(v);
                case bool v:
                    return FormatBool(v);
                case Guid v:
                    return FormatGuid(v);
                case DateTime v:
                    return FormatDateTime(v);
                case DateTimeOffset v:
                    return FormatDateTimeOffset(v);
                case double v:
                    return FormatDouble(v);
                case float v:
                    return FormatFloat(v);
                case SystemMethods v:
                    return FormatSystemMethods(v);
                case Enum v:
                    return FormatEnum(v);
                case decimal v:
                    return FormatDecimal(v);
                case RawSql v:
                    return v.Value;
                case byte[] v:
                    return FormatByteArray(v);
                case TimeSpan v:
                    return FromTimeSpan(v);
            }

            return value.ToString();
        }

        public virtual string FromTimeSpan(TimeSpan value)
        {
            return ValueQuote + value.ToString() + ValueQuote;
        }

        protected virtual string FormatByteArray(byte[] value)
        {
            var hex = new System.Text.StringBuilder((value.Length * 2)+2);
            hex.Append("0x");
            foreach (byte b in value)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        private string FormatDecimal(decimal value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        private string FormatFloat(float value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        private string FormatDouble(double value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        public virtual string FormatNull()
        {
            return "NULL";
        }

        public virtual string FormatAnsiString(string value)
        {
            return ValueQuote + value.Replace(ValueQuote, EscapeValueQuote) + ValueQuote;
        }

        public virtual string FormatNationalString(string value)
        {
            return ValueQuote + value.Replace(ValueQuote, EscapeValueQuote) + ValueQuote;
        }

        public virtual string FormatSystemMethods(SystemMethods value)
        {
            throw new NotSupportedException($"The system method {value} is not supported.");
        }

        public virtual string FormatChar(char value)
        {
            return ValueQuote + value + ValueQuote;
        }

        public virtual string FormatBool(bool value)
        {
            return (value) ? 1.ToString() : 0.ToString();
        }

        public virtual string FormatGuid(Guid value)
        {
            return ValueQuote + value.ToString() + ValueQuote;
        }

        public virtual string FormatDateTime(DateTime value)
        {
            return ValueQuote + (value).ToString("yyyy-MM-ddTHH:mm:ss",CultureInfo.InvariantCulture) + ValueQuote;
        }

        public virtual string FormatDateTimeOffset(DateTimeOffset value)
        {
            return ValueQuote + (value).ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture) + ValueQuote;
        }

        public virtual string FormatEnum(object value)
        {
            return ValueQuote + value.ToString() + ValueQuote;
        }

        public virtual string ValueQuote { get { return "'"; } }

        public virtual string EscapeValueQuote { get { return ValueQuote + ValueQuote; } }



        /// <summary>
        /// Returns the opening quote identifier - " is the standard according to the specification
        /// </summary>
        public virtual string OpenQuote => "\"";

        /// <summary>
        /// Returns the closing quote identifier - " is the standard according to the specification
        /// </summary>
        public virtual string CloseQuote => "\"";

        public virtual string OpenQuoteEscapeString { get { return OpenQuote.PadRight(2, OpenQuote.ToCharArray()[0]); } }
        public virtual string CloseQuoteEscapeString { get { return CloseQuote.PadRight(2, CloseQuote.ToCharArray()[0]); } }

        /// <summary>
        /// Returns true is the value starts and ends with a close quote
        /// </summary>
        public virtual bool IsQuoted(string name)
        {
            if (String.IsNullOrEmpty(name)) return false;
            //This can return true incorrectly in some cases edge cases.
            //If a string say [myname]] is passed in this is not correctly quote for MSSQL but this function will
            //return true.
            return (name.StartsWith(OpenQuote) && name.EndsWith(CloseQuote));
        }

        protected virtual bool ShouldQuote(string name)
        {
            return (!string.IsNullOrEmpty(OpenQuote) || !string.IsNullOrEmpty(CloseQuote)) && !string.IsNullOrEmpty(name);
        }

        /// <summary>
        /// Returns a quoted string that has been correctly escaped
        /// </summary>
        public virtual string Quote(string name)
        {
            //Exit early if not quoting is needed
            if (!ShouldQuote(name))
                return name;

            string quotedName = name;
            if (!string.IsNullOrEmpty(OpenQuoteEscapeString))
            {
                quotedName = name.Replace(OpenQuote, OpenQuoteEscapeString);
            }

            //If closing quote is the same as the opening quote then no need to escape again
            if (OpenQuote != CloseQuote)
            {
                if (!string.IsNullOrEmpty(CloseQuoteEscapeString))
                {
                    quotedName = quotedName.Replace(CloseQuote, CloseQuoteEscapeString);
                }
            }

            return OpenQuote + quotedName + CloseQuote;
        }

        /// <summary>
        /// Quotes a column name
        /// </summary>
        public virtual string QuoteColumnName(string columnName)
        {
            return IsQuoted(columnName) ? columnName : Quote(columnName);
        }

        /// <summary>
        /// Quotes a constraint name
        /// </summary>
        public virtual string QuoteConstraintName(string constraintName)
        {
            return IsQuoted(constraintName) ? constraintName : Quote(constraintName);
        }

        /// <summary>
        /// Quote an index name
        /// </summary>
        /// <param name="indexName"></param>
        /// <returns></returns>
        public virtual string QuoteIndexName(string indexName)
        {
            return IsQuoted(indexName) ? indexName : Quote(indexName);
        }

        /// <summary>
        /// Quotes a Table name
        /// </summary>
        public virtual string QuoteTableName(string tableName)
        {
            return IsQuoted(tableName) ? tableName : Quote(tableName);
        }

        /// <summary>
        /// Quotes a Schema Name
        /// </summary>
        public virtual string QuoteSchemaName(string schemaName)
        {
            return IsQuoted(schemaName) ? schemaName : Quote(schemaName);
        }

        /// <summary>
        /// Quotes a Sequence name
        /// </summary>
        public virtual string QuoteSequenceName(string sequenceName)
        {
            return IsQuoted(sequenceName) ? sequenceName : Quote(sequenceName);
        }

        /// <summary>
        /// Provides and unquoted, unescaped string
        /// </summary>
        public virtual string UnQuote(string quoted)
        {
            string unquoted;

            if (IsQuoted(quoted))
            {
                unquoted = quoted.Substring(1, quoted.Length - 2);
            }
            else
            {
                unquoted = quoted;
            }

            unquoted = unquoted.Replace(OpenQuoteEscapeString, OpenQuote);

            if (OpenQuote != CloseQuote)
            {
                unquoted = unquoted.Replace(CloseQuoteEscapeString, CloseQuote);
            }

            return unquoted;
        }
    }
}
