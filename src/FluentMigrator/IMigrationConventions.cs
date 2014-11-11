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

namespace FluentMigrator
{
    public interface IMigrationConventions
    {
        Func<Model.ForeignKeyDefinition, string> GetForeignKeyName { get; set; }
        Func<Model.IndexDefinition, string> GetIndexName { get; set; }
        Func<string, string> GetPrimaryKeyName { get; set; }
        Func<Type, bool> TypeIsMigration { get; set; }
        Func<Type, bool> TypeIsProfile { get; set; }
        Func<Type, MigrationStage?> GetMaintenanceStage { get; set; }
        Func<Type, bool> TypeIsVersionTableMetaData { get; set; }
        Func<string> GetWorkingDirectory { get; set; }
        Func<Type, IMigrationInfo> GetMigrationInfo { get; set; }
        Func<Model.ConstraintDefinition, string> GetConstraintName { get; set; }
        Func<Type, bool> TypeHasTags { get; set; }
        Func<Type, IEnumerable<string>, bool> TypeHasMatchingTags { get; set; }
        Func<Type,string,string> GetAutoScriptUpName {get;set;}
        Func<Type, string, string> GetAutoScriptDownName { get; set; }
    }
}
