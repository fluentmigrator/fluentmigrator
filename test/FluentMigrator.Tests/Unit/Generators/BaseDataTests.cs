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

using System.ComponentModel;

namespace FluentMigrator.Tests.Unit.Generators
{
    [Category("Generator")]
    public abstract class BaseDataTests
    {
        public abstract void CanDeleteDataForAllRowsWithCustomSchema();
        public abstract void CanDeleteDataForAllRowsWithDefaultSchema();
        public abstract void CanDeleteDataForMultipleRowsWithCustomSchema();
        public abstract void CanDeleteDataForMultipleRowsWithDefaultSchema();
        public abstract void CanDeleteDataWithCustomSchema();
        public abstract void CanDeleteDataWithDefaultSchema();
        public abstract void CanDeleteDataWithDbNullCriteria();
        public abstract void CanInsertDataWithCustomSchema();
        public abstract void CanInsertDataWithDefaultSchema();
        public abstract void CanInsertGuidDataWithCustomSchema();
        public abstract void CanInsertGuidDataWithDefaultSchema();
        public abstract void CanUpdateDataForAllDataWithCustomSchema();
        public abstract void CanUpdateDataForAllDataWithDefaultSchema();
        public abstract void CanUpdateDataWithCustomSchema();
        public abstract void CanUpdateDataWithDefaultSchema();
        public abstract void CanUpdateDataWithDbNullCriteria();
    }
}
