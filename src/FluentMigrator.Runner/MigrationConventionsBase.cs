#region License
// Copyright (c) 2007-2009, Sean Chambers <schambers80@gmail.com>
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
using System.Collections.Generic;
using FluentMigrator.Infrastructure;
using FluentMigrator.Model;

// TODO Why is IMigrationConventions full of delegates? Bad idea, get rid of them completely instead of using wrappers.
namespace FluentMigrator.Runner
{
    public class MigrationConventionsBase : IMigrationConventions
    {
        Func<ForeignKeyDefinition, string> IMigrationConventions.GetForeignKeyName
        {
            get { return GetForeignKeyName; }
            set { throw NewUnusedException(); }
        }

        Func<IndexDefinition, string> IMigrationConventions.GetIndexName
        {
            get { return GetIndexName; }
            set { throw NewUnusedException(); }
        }

        Func<string, string> IMigrationConventions.GetPrimaryKeyName
        {
            get { return GetPrimaryKeyName; }
            set { throw NewUnusedException(); }
        }

        Func<Type, bool> IMigrationConventions.TypeIsMigration
        {
            get { return TypeIsMigration; }
            set { throw NewUnusedException(); }
        }

        Func<Type, bool> IMigrationConventions.TypeIsProfile
        {
            get { return TypeIsProfile; }
            set { throw NewUnusedException(); }
        }

        Func<Type, bool> IMigrationConventions.TypeIsVersionTableMetaData
        {
            get { return TypeIsVersionTableMetaData; }
            set { throw NewUnusedException(); }
        }

        Func<string> IMigrationConventions.GetWorkingDirectory
        {
            get { return () => SqlScriptsDirectory; }
            set { SqlScriptsDirectory = value(); }
        }

        Func<IMigration, IMigrationInfo> IMigrationConventions.GetMigrationInfo
        {
            get { return GetMigrationInfoFor; }
            set { throw NewUnusedException(); }
        }

        Func<ConstraintDefinition, string> IMigrationConventions.GetConstraintName
        {
            get { return GetConstraintName; }
            set { throw NewUnusedException(); }
        }

        Func<Type, bool> IMigrationConventions.TypeHasTags
        {
            get { return TypeHasTags; }
            set { throw NewUnusedException(); }
        }

        Func<Type, IEnumerable<string>, bool> IMigrationConventions.TypeHasMatchingTags
        {
            get { return TypeHasMatchingTags; }
            set { throw NewUnusedException(); }
        }

        public virtual string SqlScriptsDirectory { get; private set; }

        public virtual string GetPrimaryKeyName(string tableName)
        {
            return DefaultMigrationConventions.GetPrimaryKeyName(tableName);
        }

        public virtual string GetForeignKeyName(ForeignKeyDefinition foreignKey)
        {
            return DefaultMigrationConventions.GetForeignKeyName(foreignKey);
        }

        public virtual string GetIndexName(IndexDefinition index)
        {
            return DefaultMigrationConventions.GetIndexName(index);
        }

        public virtual bool TypeIsMigration(Type type)
        {
            return DefaultMigrationConventions.TypeIsMigration(type);
        }

        public virtual bool TypeIsProfile(Type type)
        {
            return DefaultMigrationConventions.TypeIsProfile(type);
        }

        public virtual bool TypeIsVersionTableMetaData(Type type)
        {
            return DefaultMigrationConventions.TypeIsVersionTableMetaData(type);
        }

        public virtual IMigrationInfo GetMigrationInfoFor(IMigration migration)
        {
            return DefaultMigrationConventions.GetMigrationInfoFor(migration);
        }

        public virtual string GetConstraintName(ConstraintDefinition expression)
        {
            return DefaultMigrationConventions.GetConstraintName(expression);
        }

        public virtual bool TypeHasTags(Type type)
        {
            return DefaultMigrationConventions.TypeHasTags(type);
        }

        public virtual bool TypeHasMatchingTags(Type type, IEnumerable<string> tagsToMatch)
        {
            return DefaultMigrationConventions.TypeHasMatchingTags(type, tagsToMatch);
        }

        private static Exception NewUnusedException()
        {
            return new NotSupportedException("Unused class member.");
        }
    }
}