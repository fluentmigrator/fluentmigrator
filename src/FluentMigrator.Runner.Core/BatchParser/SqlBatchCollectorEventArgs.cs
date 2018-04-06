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

using System;

using JetBrains.Annotations;

namespace FluentMigrator.Runner.BatchParser
{
    /// <summary>
    /// Event arguments for SQL text to be collected
    /// </summary>
    internal class SqlBatchCollectorEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SqlBatchCollectorEventArgs"/> class.
        /// </summary>
        /// <param name="sqlContent">The SQL text to be collected</param>
        /// <param name="isEndOfLine"><c>true</c> when a new line character should be appended</param>
        public SqlBatchCollectorEventArgs([NotNull] string sqlContent, bool isEndOfLine = false)
        {
            SqlContent = sqlContent;
            IsEndOfLine = isEndOfLine;
        }

        /// <summary>
        /// Gets the SQL text to be collected
        /// </summary>
        [NotNull]
        public string SqlContent { get; }

        /// <summary>
        /// Gets a value indicating whether a new line character should be appended
        /// </summary>
        public bool IsEndOfLine { get; }
    }
}
