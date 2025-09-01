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
using System.Collections.Generic;

using JetBrains.Annotations;

namespace FluentMigrator.Runner.Generators.Oracle
{
    /// <summary>
    /// The Oracle SQL quoter for FluentMigrator.
    /// </summary>
    public class OracleQuoter : OracleQuoterQuotedIdentifier
    {
        /// <summary>
        /// https://docs.oracle.com/cd/A97630_01/appdev.920/a42525/apb.htm
        /// </summary>
        private static readonly HashSet<string> _keywords = new HashSet<string>(
            new[]
            {
                "access", "else", "modify", "start", "add", "exclusive", "noaudit", "select",
                "all", "exists", "nocompress", "session", "alter", "file", "not", "set",
                "and", "float", "notfound", "share", "any", "for", "nowait", "size",
                "arraylen", "from", "null", "smallint", "as", "grant", "number", "sqlbuf",
                "asc", "group", "of", "successful", "audit", "having", "offline", "synonym",
                "between", "identified", "on", "sysdate", "by", "immediate", "online", "table",
                "char", "in", "option", "then", "check", "increment", "or", "to",
                "cluster", "index", "order", "trigger", "column", "initial", "pctfree", "uid",
                "comment", "insert", "prior", "union", "compress", "integer", "privileges", "unique",
                "connect", "intersect", "public", "update", "create", "into", "raw", "user",
                "current", "is", "rename", "validate", "date", "level", "resource", "values",
                "decimal", "like", "revoke", "varchar", "default", "lock", "row", "varchar2",
                "delete", "long", "rowid", "view", "desc", "maxextents", "rowlabel", "whenever",
                "distinct", "minus", "rownum", "where", "drop", "mode", "rows", "with"
            },
            StringComparer.OrdinalIgnoreCase);

        /// <inheritdoc />
        [ContractAnnotation("name:null => false")]
        protected override bool ShouldQuote(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            if (_keywords.Contains(name))
            {
                return true;
            }

            // Otherwise, quote only when it's not a valid Oracle identifier
            var first = name[0];
            if (!IsLetter(first))
            {
                return true;
            }

            var len = name.Length;
            for (var i = 1; i < len; i++)
            {
                var c = name[i];
                if (!IsIdentifierChar(c))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsLetter(char c)
        {
            return c is >= 'A' and <= 'Z' or >= 'a' and <= 'z';
        }

        private static bool IsIdentifierChar(char c)
        {
            return IsLetter(c) || IsDigit(c) || c == '_' || c == '$' || c == '#';
        }

        private static bool IsDigit(char c)
        {
            return c is >= '0' and <= '9';
        }
    }
}
