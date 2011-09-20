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
using System.Collections.Generic;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Builders
{
    public interface IColumnOptionSyntax<TNext> : IFluentSyntax
        where TNext : IFluentSyntax
    {
        TNext WithDefaultValue(object value);
        TNext WithDefaultFunctionValue(string value);
        TNext WithDefaultGuidValue();
        TNext WithDefaultCurrentDateTimeValue();
        TNext Identity();
        TNext Indexed();

        TNext PrimaryKey();
        TNext PrimaryKey(string primaryKeyName);

        TNext Nullable();
        TNext NotNullable();
        TNext Unique();

        TNext ForeignKey(string primaryTableName, string primaryColumnName);
        TNext ForeignKey(string foreignKeyName, string primaryTableName, string primaryColumnName);
        TNext ForeignKey(string foreignKeyName, string primaryTableSchema, string primaryTableName, string primaryColumnName);
        TNext ForeignKey();

        TNext ReferencedBy(string foreignTableName, string foreignColumnName);
        TNext ReferencedBy(string foreignKeyName, string foreignTableName, string foreignColumnName);
        TNext ReferencedBy(string foreignKeyName, string foreignTableSchema, string foreignTableName, string foreignColumnName);

        [Obsolete("Please use ReferencedBy syntax. This method will be removed in the next version")]
        TNext References(string foreignKeyName, string foreignTableName, IEnumerable<string> foreignColumnNames);
        [Obsolete("Please use ReferencedBy syntax. This method will be removed in the next version")]
        TNext References(string foreignKeyName, string foreignTableSchema, string foreignTableName, IEnumerable<string> foreignColumnNames);
    }
}