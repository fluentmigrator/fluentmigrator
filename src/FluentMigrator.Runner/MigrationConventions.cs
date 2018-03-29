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
using FluentMigrator.Runner.Infrastructure;

namespace FluentMigrator.Runner
{
    public class MigrationConventions : IMigrationConventions
    {
        private static readonly IMigrationConventions _default = DefaultMigrationConventions.Instance;

        public Func<string, string> GetPrimaryKeyName { get; set; }
        public Func<ForeignKeyDefinition, string> GetForeignKeyName { get; set; }
        public Func<IndexDefinition, string> GetIndexName { get; set; }
        public Func<Type, bool> TypeIsMigration { get; set; }
        public Func<Type, bool> TypeIsProfile { get; set; }
        public Func<Type, MigrationStage?> GetMaintenanceStage { get; set; }
        public Func<Type, bool> TypeIsVersionTableMetaData { get; set; }
        public Func<Type, IMigrationInfo> GetMigrationInfo { get; set; }
        public Func<string> GetWorkingDirectory { get; set; }
        public Func<ConstraintDefinition, string> GetConstraintName { get; set; }
        public Func<Type, bool> TypeHasTags { get; set; }
        public Func<Type, IEnumerable<string>, bool> TypeHasMatchingTags { get; set; }
        public Func<Type, string, string> GetAutoScriptUpName { get; set; }
        public Func<Type, string, string> GetAutoScriptDownName { get; set; }
        public Func<string> GetDefaultSchema { get; set; }

        public MigrationConventions()
        {
            GetPrimaryKeyName = _default.GetPrimaryKeyName;
            GetForeignKeyName = _default.GetForeignKeyName;
            GetIndexName = _default.GetIndexName;
            TypeIsMigration = _default.TypeIsMigration;
            TypeIsProfile = _default.TypeIsProfile;
            GetMaintenanceStage = _default.GetMaintenanceStage;
            TypeIsVersionTableMetaData = _default.TypeIsVersionTableMetaData;
            GetMigrationInfo = _default.GetMigrationInfo;
            GetWorkingDirectory = _default.GetWorkingDirectory;
            GetConstraintName = _default.GetConstraintName;
            TypeHasTags = _default.TypeHasTags;
            TypeHasMatchingTags = _default.TypeHasMatchingTags;
            GetAutoScriptUpName = _default.GetAutoScriptUpName;
            GetAutoScriptDownName = _default.GetAutoScriptDownName;
            GetDefaultSchema = _default.GetDefaultSchema;
        }
    }
}
