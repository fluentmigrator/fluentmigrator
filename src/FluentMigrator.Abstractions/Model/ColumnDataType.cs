#region License
// Copyright (c) 2023, Fluent Migrator Project
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

using System.Data;

namespace FluentMigrator.Model
{
    public class ColumnDataType : IColumnDataType
    {
        /// <inheritdoc />
        public DbType? Type { get; set; }

        /// <inheritdoc />
        public int? Size { get; set; }

        /// <inheritdoc />
        public int? Precision { get; set; }

        /// <inheritdoc />
        public string CollationName { get; set; }

        /// <inheritdoc />
        public string CustomType { get; set; }

        /// <inheritdoc />
        public string Expression { get; set; }

        /// <inheritdoc />
        public bool ExpressionStored { get; set; }
    }
}
