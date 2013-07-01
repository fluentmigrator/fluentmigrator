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
    public interface IColumnOptionSyntax<TNext,TNextFk> : IFluentSyntax
        where TNext : IFluentSyntax
        where TNextFk : IFluentSyntax
    {
        TNext WithDefault(SystemMethods method);
        TNext WithDefaultValue(object value);
        TNext WithColumnDescription(string description);

        TNext Identity();
        TNext Indexed();
        TNext Indexed(string indexName);

        TNext PrimaryKey();
        TNext PrimaryKey(string primaryKeyName);
        TNext Nullable();
        TNext NotNullable();
        TNext Unique();
        TNext Unique(string indexName);

        TNextFk ForeignKey(string primaryTableName, string primaryColumnName);
        TNextFk ForeignKey(string foreignKeyName, string primaryTableName, string primaryColumnName);
        TNextFk ForeignKey(string foreignKeyName, string primaryTableSchema, string primaryTableName, string primaryColumnName);
        TNextFk ForeignKey();

        TNextFk ReferencedBy(string foreignTableName, string foreignColumnName);
        TNextFk ReferencedBy(string foreignKeyName, string foreignTableName, string foreignColumnName);
        TNextFk ReferencedBy(string foreignKeyName, string foreignTableSchema, string foreignTableName, string foreignColumnName);

        [Obsolete("Please use ReferencedBy syntax. This method will be removed in the next version")]
        TNext References(string foreignKeyName, string foreignTableName, IEnumerable<string> foreignColumnNames);
        [Obsolete("Please use ReferencedBy syntax. This method will be removed in the next version")]
        TNext References(string foreignKeyName, string foreignTableSchema, string foreignTableName, IEnumerable<string> foreignColumnNames);
    }
}
