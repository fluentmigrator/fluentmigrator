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

using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.Generators
{
    [Category("Generator")]
    [Category("Index")]
    public abstract class BaseIndexTests
    {
        public abstract void CanCreateIndexWithCustomSchema();
        public abstract void CanCreateIndexWithDefaultSchema();
        public abstract void CanCreateMultiColumnIndexWithCustomSchema();
        public abstract void CanCreateMultiColumnIndexWithDefaultSchema();
        public abstract void CanCreateMultiColumnUniqueIndexWithCustomSchema();
        public abstract void CanCreateMultiColumnUniqueIndexWithDefaultSchema();
        public abstract void CanCreateUniqueIndexWithCustomSchema();
        public abstract void CanCreateUniqueIndexWithDefaultSchema();
        public abstract void CanDropIndexWithCustomSchema();
        public abstract void CanDropIndexWithDefaultSchema();
    }
}
