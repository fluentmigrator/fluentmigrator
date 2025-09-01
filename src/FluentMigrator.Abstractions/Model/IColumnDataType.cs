#region License
// Copyright (c) 2018, Fluent Migrator Project
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
    public interface IColumnDataType
    {
        /// <summary>
        /// Gets or sets the column type
        /// </summary>
        DbType? Type { get; set; }

        /// <summary>
        /// Gets or sets the column type size (read: precision or length)
        /// </summary>
        int? Size { get; set; }

        /// <summary>
        /// Gets or sets the column type precision (read: scale)
        /// </summary>
        int? Precision { get; set; }

        /// <summary>
        /// Gets or sets the collation name if the column has a string or ansi string type
        /// </summary>
        string CollationName { get; set; }

        /// <summary>
        /// Gets or sets a database specific custom column type
        /// </summary>
        string CustomType { get; set; }

        /// <summary>
        /// Gets or sets an expression that defines the column
        /// </summary>
        string Expression { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the calculated value is stored
        /// </summary>
        bool ExpressionStored { get; set; }
    }
}
