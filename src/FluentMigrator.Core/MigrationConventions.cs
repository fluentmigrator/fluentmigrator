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
using FluentMigrator.Model;

namespace FluentMigrator
{
    public class MigrationConventions : IMigrationConventions
    {
        public Func<string, string> GetPrimaryKeyName { get; set; }
        public Func<ForeignKeyDefinition, string> GetForeignKeyName { get; set; }
        public Func<IndexDefinition, string> GetIndexName { get; set; }
        public Func<Type, bool> TypeIsMigration { get; set; }
        public Func<Type, bool> TypeIsProfile { get; set; }
        public Func<Type, MigrationStage?> GetMaintenanceStage { get; set; }
        public Func<Type, bool> TypeIsVersionTableMetaData { get; set; }
        public Func<Type, IMigrationInfo> GetMigrationInfo { get; set; }
        public Func<string> GetWorkingDirectory { get; set; }
        public Func<Model.ConstraintDefinition, string> GetConstraintName { get; set; }
        public Func<Type, bool> TypeHasTags { get; set; }
        public Func<Type, IEnumerable<string>, bool> TypeHasMatchingTags { get; set; }
        public Func<Type, string, string> GetAutoScriptUpName { get; set; }
        public Func<Type, string, string> GetAutoScriptDownName { get; set; }

        public MigrationConventions()
        {
            GetPrimaryKeyName = DefaultMigrationConventions.GetPrimaryKeyName;
            GetForeignKeyName = DefaultMigrationConventions.GetForeignKeyName;
            GetIndexName = DefaultMigrationConventions.GetIndexName;
            TypeIsMigration = DefaultMigrationConventions.TypeIsMigration;
            TypeIsProfile = DefaultMigrationConventions.TypeIsProfile;
            GetMaintenanceStage = DefaultMigrationConventions.GetMaintenanceStage;
            TypeIsVersionTableMetaData = DefaultMigrationConventions.TypeIsVersionTableMetaData;
            GetMigrationInfo = DefaultMigrationConventions.GetMigrationInfoFor;
            GetWorkingDirectory = DefaultMigrationConventions.GetWorkingDirectory;
            GetConstraintName = DefaultMigrationConventions.GetConstraintName;
            TypeHasTags = DefaultMigrationConventions.TypeHasTags;
            TypeHasMatchingTags = DefaultMigrationConventions.TypeHasMatchingTags;
            GetAutoScriptUpName = DefaultMigrationConventions.GetAutoScriptUpName;
            GetAutoScriptDownName = DefaultMigrationConventions.GetAutoScriptDownName;
        }        
    }
}
