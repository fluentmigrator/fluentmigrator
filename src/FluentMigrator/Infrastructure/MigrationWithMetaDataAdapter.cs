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

namespace FluentMigrator.Infrastructure
{
    public class MigrationWithMetaDataAdapter : IMigration, IMigrationMetadata
    {
        public MigrationWithMetaDataAdapter(IMigration migration, IMigrationMetadata metadata)
        {
            if (migration == null) throw new ArgumentNullException("migration");
            if (metadata == null) throw new ArgumentNullException("metadata");
            Migration = migration;
            MetaData = metadata;
        }

        public Type Type
        {
            get { return MetaData.Type; }
        }

        public long Version
        {
            get { return MetaData.Version; }
        }

        public IMigration Migration { get; private set; }

        public IMigrationMetadata MetaData { get; private set; }

        public object ApplicationContext
        {
            get { return Migration.ApplicationContext; }
        }

        public object Trait(string name)
        {
            return MetaData.Trait(name);
        }

        public bool HasTrait(string name)
        {
            return MetaData.HasTrait(name);
        }

        public void GetUpExpressions(IMigrationContext context)
        {
            Migration.GetUpExpressions(context);
        }

        public void GetDownExpressions(IMigrationContext context)
        {
            Migration.GetDownExpressions(context);
        }
    }
}