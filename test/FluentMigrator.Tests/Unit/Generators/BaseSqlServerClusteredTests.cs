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

namespace FluentMigrator.Tests.Unit.Generators
{
    public abstract class BaseSqlServerClusteredTests
    {
        public abstract void CanCreateClusteredIndexWithCustomSchema();
        public abstract void CanCreateClusteredIndexWithDefaultSchema();
        public abstract void CanCreateMultiColumnClusteredIndexWithCustomSchema();
        public abstract void CanCreateMultiColumnClusteredIndexWithDefaultSchema();
        public abstract void CanCreateNamedClusteredPrimaryKeyConstraintWithCustomSchema();
        public abstract void CanCreateNamedClusteredPrimaryKeyConstraintWithDefaultSchema();
        public abstract void CanCreateNamedClusteredUniqueConstraintWithCustomSchema();
        public abstract void CanCreateNamedClusteredUniqueConstraintWithDefaultSchema();
        public abstract void CanCreateNamedMultiColumnClusteredPrimaryKeyConstraintWithCustomSchema();
        public abstract void CanCreateNamedMultiColumnClusteredPrimaryKeyConstraintWithDefaultSchema();
        public abstract void CanCreateNamedMultiColumnClusteredUniqueConstraintWithCustomSchema();
        public abstract void CanCreateNamedMultiColumnClusteredUniqueConstraintWithDefaultSchema();
        public abstract void CanCreateNamedMultiColumnNonClusteredPrimaryKeyConstraintWithCustomSchema();
        public abstract void CanCreateNamedMultiColumnNonClusteredPrimaryKeyConstraintWithDefaultSchema();
        public abstract void CanCreateNamedMultiColumnNonClusteredUniqueConstraintWithCustomSchema();
        public abstract void CanCreateNamedMultiColumnNonClusteredUniqueConstraintWithDefaultSchema();
        public abstract void CanCreateNamedNonClusteredPrimaryKeyConstraintWithCustomSchema();
        public abstract void CanCreateNamedNonClusteredPrimaryKeyConstraintWithDefaultSchema();
        public abstract void CanCreateNamedNonClusteredUniqueConstraintWithCustomSchema();
        public abstract void CanCreateNamedNonClusteredUniqueConstraintWithDefaultSchema();
        public abstract void CanCreateUniqueClusteredIndexWithCustomSchema();
        public abstract void CanCreateUniqueClusteredIndexWithDefaultSchema();
        public abstract void CanCreateUniqueClusteredMultiColumnIndexWithCustomSchema();
        public abstract void CanCreateUniqueClusteredMultiColumnIndexWithDefaultSchema();
    }
}
